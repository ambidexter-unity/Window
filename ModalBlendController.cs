using System.Linq;
using Common.Activatable;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Common.Window
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RawImage), typeof(CanvasGroup), typeof(RectTransform))]
	public class ModalBlendController : MonoBehaviour, IActivatable, IPointerDownHandler, IPointerUpHandler
	{
		private const float FadeInDuration = 0.5f;
		private const float FadeOutDuration = 0.3f;
		private const float BlendDensity = 0.5f;

		private bool _pointerDown;

		private static readonly Color ActiveColor = new Color(0, 0, 0, BlendDensity);
		private static readonly Color InactiveColor = new Color(0, 0, 0, 0);

		private RawImage _back;
		private CanvasGroup _canvasGroup;
		
		private readonly ReactiveProperty<ActivatableState> _activatableState = 
			new ReactiveProperty<ActivatableState>(Activatable.ActivatableState.Inactive);

		// IPointerDownHandler
		
		public void OnPointerDown(PointerEventData eventData)
		{
			_pointerDown = true;
		}

		// \IPointerDownHandler
		
		// IPointerUpHandler
		
		public void OnPointerUp(PointerEventData eventData)
		{
			if (_pointerDown &&
			    !eventData.hovered.Any(o => o != gameObject && o != transform.parent.gameObject))
			{
				foreach (Transform child in transform)
				{
					child.gameObject.SendMessage("OnModalBlendClick", SendMessageOptions.DontRequireReceiver);
				}
			}

			_pointerDown = false;
		}
		
		// \IPointerUpHandler
		
		// IActivatable

		public IReadOnlyReactiveProperty<ActivatableState> ActivatableState => _activatableState;

		public void Activate(bool immediately = false)
		{
			if (this.IsActive()) return;
			if (this.IsBusy()) DOTween.Kill(_back);

			_canvasGroup.blocksRaycasts = true;
			_canvasGroup.interactable = true;

			if (immediately)
			{
				_back.color = ActiveColor;
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Active);
			}
			else
			{
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.ToActive);
				_back.DOColor(ActiveColor, FadeInDuration).SetUpdate(true).OnComplete(() =>
				{
					_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Active);
				});
			}

			GetComponentInChildren<IWindow>()?.Activate(immediately);
		}

		public void Deactivate(bool immediately = false)
		{
			if (this.IsInactive()) return;
			if (this.IsBusy()) DOTween.Kill(_back);

			if (immediately)
			{
				_back.color = InactiveColor;
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Inactive);

				_canvasGroup.blocksRaycasts = false;
				_canvasGroup.interactable = false;
			}
			else
			{
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.ToInactive);
				_back.DOColor(InactiveColor, FadeOutDuration).OnComplete(() =>
				{
					_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Inactive);

					_canvasGroup.blocksRaycasts = false;
					_canvasGroup.interactable = false;
				});
			}
			
			GetComponentInChildren<IWindow>()?.Deactivate(immediately);
		}
		
		// \IActivatable
		
		private void Awake()
		{
			var rt = (RectTransform) gameObject.transform;
			rt.anchorMin = new Vector2(0, 0);
			rt.anchorMax = new Vector2(1, 1);
			rt.offsetMax = new Vector2(0, 0);
			rt.offsetMin = new Vector2(0, 0);

			_back = GetComponent<RawImage>();
			_back.color = InactiveColor;

			_canvasGroup = GetComponent<CanvasGroup>();
			_canvasGroup.interactable = false;
		}

		private void Start()
		{
//			_lockId = TouchHelper.TouchHelper.Lock();
		}

		private void OnDestroy()
		{
//			TouchHelper.TouchHelper.Unlock(_lockId);
		}
	}
}