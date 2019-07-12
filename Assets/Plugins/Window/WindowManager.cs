#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using Zenject;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UniRx;
using System.Linq;
using System.Collections.Generic;
using Common.Activatable;
using Extensions;


namespace Common.Window
{
	// ReSharper disable InconsistentNaming
	[Serializable]
	public class WindowManagerWindowsListItem
	{
		public string Type;
		public GameObject Prefab;
	}
	// ReSharper restore InconsistentNaming

	public class WindowManager : ScriptableObjectInstaller<WindowManager>, IWindowManager
	{
		private struct DelayCall : IComparable<DelayCall>
		{
			public Action<IWindow> Callback;
			public string Type;
			public object[] Args;
			public bool IsModal;
			public bool IsUnique;
			public long Timestamp;

			public int CompareTo(DelayCall other)
			{
				if (other.IsUnique && !IsUnique) return 1;
				if (!other.IsUnique && IsUnique) return -1;
				if (Timestamp > other.Timestamp) return 1;
				if (Timestamp < other.Timestamp) return -1;
				return 0;
			}
		}

		[SerializeField] private WindowManagerWindowsListItem[] _windows = new WindowManagerWindowsListItem[0];

		private readonly Dictionary<IWindow, UnityAction> _closeHandlers = new Dictionary<IWindow, UnityAction>();
		private readonly List<IWindow> _openedWindows = new List<IWindow>();

		private readonly SortedSet<DelayCall> _delayedCalls = new SortedSet<DelayCall>();
		private bool _isUnique;

#if UNITY_EDITOR
		private const string ManagerPath = "Assets/Scripts/Common/Manager";

		[MenuItem("Tools/Game Settings/Window Manager Settings")]
		private static void GetAndSelectSettingsInstance()
		{
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = InspectorExtensions.FindOrCreateNewScriptableObject<WindowManager>(ManagerPath);
		}
#endif

		public override void InstallBindings()
		{
			Container.Bind<IWindowManager>().FromInstance(this).AsSingle();
		}

		// IWindowManager

		public bool ShowWindow(Action<IWindow> callback, string type, object[] args = null,
			bool isModal = true, bool isUnique = false, DiContainer container = null)
		{
			if (_isUnique || isUnique && _openedWindows.Count > 0)
			{
				_delayedCalls.Add(new DelayCall
				{
					Callback = callback,
					Type = type,
					Args = args,
					IsModal = isModal,
					IsUnique = isUnique,
					Timestamp = DateTime.Now.Ticks
				});
				return false;
			}

			var prefab = _windows.Single(item => item.Type == type).Prefab;
			var blend = isModal ? CreateModalBlend() : null;
			var window = (container ?? Container).InstantiatePrefabForComponent<IWindow>(prefab,
				blend != null ? blend : FindOrCreatePopupCanvas());
			var instance = (window as MonoBehaviour)?.gameObject;
			if (instance == null)
			{
				throw new NotSupportedException($"Prefab for window {type} has no" +
				                                " controller, that implements IWindow.");
			}

			if (args != null && args.Length > 0)
			{
				window.SetArgs(args);
			}

			_isUnique = isUnique;
			ListenForCloseWindow(window);
			_openedWindows.Add(window);

			var activatable = (blend != null ? blend.gameObject : instance).GetComponent<IActivatable>();
			activatable.Activate();

			callback?.Invoke(window);
			return true;
		}

		public void CloseWindow(IWindow window)
		{
			var blend = (window as MonoBehaviour)?.GetComponentInParent<ModalBlendController>();
			// Если есть бленда, то отслеживать деактивацию и бленды и окна, если нет, то только окна
			var isDeactivated = blend != null
				? blend.ActivatableState.CombineLatest(window.ActivatableState,
						(state1, state2) => state1 == ActivatableState.Inactive && state2 == ActivatableState.Inactive
							? ActivatableState.Inactive
							: ActivatableState.ToInactive)
					.Select(state => state == ActivatableState.Inactive)
					.ToReadOnlyReactiveProperty(
						blend.ActivatableState.Value == ActivatableState.Inactive &&
						window.ActivatableState.Value == ActivatableState.Inactive)
				: window.ActivatableState.Select(state => state == ActivatableState.Inactive)
					.ToReadOnlyReactiveProperty(window.ActivatableState.Value == ActivatableState.Inactive);

			var o = (blend ? blend : window as MonoBehaviour)?.gameObject;
			if (isDeactivated.Value)
			{
				if (o != null) Destroy(o);
				isDeactivated.Dispose();
			}
			else
			{
				IDisposable d = null;
				d = isDeactivated.Subscribe(value =>
				{
					if (!value) return;
					// ReSharper disable once AccessToModifiedClosure
					d?.Dispose();
					if (o != null) Destroy(o);
					isDeactivated.Dispose();
				});
			}

			UnityAction handler;
			if (_closeHandlers.TryGetValue(window, out handler))
			{
				window.CloseEvent.RemoveListener(handler);
				_closeHandlers.Remove(window);
			}

			_openedWindows.Remove(window);
			_isUnique = false;

			((IActivatable) blend ?? window).Deactivate();

			while (_delayedCalls.Any() && !_isUnique)
			{
				var call = _delayedCalls.First();
				_delayedCalls.Remove(call);
				if (!ShowWindow(call.Callback, call.Type, call.Args, call.IsModal, call.IsUnique))
				{
					break;
				}
			}
		}

		public void CloseAll(params Type[] args)
		{
			if (args.Length > 0)
			{
				_openedWindows.Where(window => args.Contains(window.GetType()))
					.ToList().ForEach(CloseWindow);
			}
			else
			{
				_openedWindows.ToList().ForEach(CloseWindow);
			}
		}

		// \IWindowManager

		private RectTransform CreateModalBlend()
		{
			Transform transform;
			var blend = Container.InstantiateComponentOnNewGameObject<ModalBlendController>("ModalBlend");
			(transform = blend.transform).SetParent(FindOrCreatePopupCanvas(), false);
			return transform as RectTransform;
		}

		private static RectTransform FindOrCreatePopupCanvas()
		{
			var instance = GameObject.FindGameObjectsWithTag("Popup").FirstOrDefault(o =>
				o.name == "PopupCanvas" && o.GetComponent<Canvas>());
			if (instance == null)
			{
				instance = new GameObject("PopupCanvas", typeof(Canvas), typeof(CanvasScaler),
					typeof(GraphicRaycaster)) {tag = "Popup"};

				var canvas = instance.GetComponent<Canvas>();
				canvas.renderMode = RenderMode.ScreenSpaceOverlay;
				canvas.sortingOrder = 1000;

				var canvasScaler = instance.GetComponent<CanvasScaler>();
				canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
				canvasScaler.referenceResolution = WindowConst.TargetDestination;
				canvasScaler.matchWidthOrHeight = 1;

				var graphicRaycaster = instance.GetComponent<GraphicRaycaster>();
				graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.All;
			}

			return (RectTransform) instance.transform;
		}

		private void ListenForCloseWindow(IWindow window)
		{
			UnityAction handler = () => { CloseWindow(window); };
			window.CloseEvent.AddListener(handler);
			_closeHandlers.Add(window, handler);
		}
	}
}