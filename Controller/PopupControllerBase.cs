using Common.Activatable;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.Events;

namespace Common.Window.Controller
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
	public abstract class PopupControllerBase : MonoBehaviour, IWindow
	{
		protected readonly ReactiveProperty<ActivatableState> _activatableState =
			new ReactiveProperty<ActivatableState>(Activatable.ActivatableState.Inactive);

		protected readonly UnityEvent _closeEvent = new UnityEvent();

		// IWindow

		public IReadOnlyReactiveProperty<ActivatableState> ActivatableState => _activatableState;
		public UnityEvent CloseEvent => _closeEvent;

		public virtual void Activate(bool immediately = false)
		{
			if (this.IsActive()) return;

			var canvasGroup = GetComponent<CanvasGroup>();
			var rectTransform = (RectTransform) transform;

			if (this.IsBusy())
			{
				DOTween.Kill(canvasGroup);
				DOTween.Kill(rectTransform);
			}

			if (immediately)
			{
				canvasGroup.alpha = 1;
				canvasGroup.interactable = true;
				rectTransform.localScale = Vector3.one;
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Active);
			}
			else
			{
				canvasGroup.alpha = 0;
				canvasGroup.interactable = false;
				rectTransform.localScale = Vector3.one * 0.1f;
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.ToActive);
				DOTween.Sequence().Append(rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack))
					.Join(canvasGroup.DOFade(1, 0.2f).SetEase(Ease.Linear))
					.OnComplete(() =>
					{
						canvasGroup.interactable = true;
						_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Active);
					});
			}
		}

		public virtual void Deactivate(bool immediately = false)
		{
			if (this.IsInactive()) return;

			var canvasGroup = GetComponent<CanvasGroup>();
			var rectTransform = (RectTransform) transform;

			if (this.IsBusy())
			{
				DOTween.Kill(canvasGroup);
				DOTween.Kill(rectTransform);
			}

			canvasGroup.interactable = false;

			if (immediately)
			{
				canvasGroup.alpha = 0;
				rectTransform.localScale = Vector3.one * 0.1f;
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Inactive);
			}
			else
			{
				canvasGroup.alpha = 1;
				rectTransform.localScale = Vector3.one;
				_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.ToInactive);
				DOTween.Sequence().Append(rectTransform.DOScale(Vector3.one * 0.1f, 0.35f).SetEase(Ease.InBack))
					.Join(canvasGroup.DOFade(0, 0.15f).SetDelay(0.35f - 0.15f).SetEase(Ease.Linear))
					.OnComplete(() =>
					{
						_activatableState.SetValueAndForceNotify(Activatable.ActivatableState.Inactive);
					});
			}
		}

		void IWindow.SetArgs(object[] args)
		{
			DoSetArgs(args);
		}

		// \IWindow

		protected abstract void DoSetArgs(object[] args);

		// ReSharper disable once UnusedMember.Global
		protected virtual void OnModalBlendClick()
		{
			if (this.IsActive()) CloseEvent.Invoke();
		}

		protected virtual void Start()
		{
			if (this.IsActive()) return;

			var canvasGroup = GetComponent<CanvasGroup>();
			var rectTransform = (RectTransform) transform;

			canvasGroup.alpha = 0;
			canvasGroup.interactable = false;
			rectTransform.localScale = Vector3.one * 0.1f;
		}

		protected virtual void OnDestroy()
		{
			_closeEvent.RemoveAllListeners();

			if (!this.IsBusy()) return;

			var canvasGroup = GetComponent<CanvasGroup>();
			var rectTransform = (RectTransform) transform;

			DOTween.Kill(canvasGroup);
			DOTween.Kill(rectTransform);
		}
	}
}