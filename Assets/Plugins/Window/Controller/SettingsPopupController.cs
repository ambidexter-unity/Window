using Common.Audio;
using Common.Locale;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Common.Window.Controller
{
	public class SettingsPopupController : PopupControllerBase
	{
#pragma warning disable 649
		[SerializeField] private Toggle _soundToggle;
		[SerializeField] private Toggle _musicToggle;
		[SerializeField] private Toggle _langRusToggle;
		[SerializeField] private Toggle _langEngToggle;
		[SerializeField] private Button _closeButton;

		[Inject] private readonly LocaleService _localeService;
		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		protected override void Start()
		{
			base.Start();

			_closeButton.onClick.AddListener(() =>
			{
				_closeEvent.Invoke();
				_audioManager.PlaySound("click_01");
			});

			switch (_localeService.CurrentLanguage.Value)
			{
				case SystemLanguage.Russian:
					_langRusToggle.isOn = true;
					break;
				default:
					_langEngToggle.isOn = true;
					break;
			}
			
			_langRusToggle.onValueChanged.AddListener(value =>
			{
				if (!value) return;
				_localeService.SetCurrentLanguage(SystemLanguage.Russian);
				_audioManager.PlaySound("click_01");
			});
			
			_langEngToggle.onValueChanged.AddListener(value =>
			{
				if (!value) return;
				_localeService.SetCurrentLanguage(SystemLanguage.English);
				_audioManager.PlaySound("click_01");
			});

			_soundToggle.isOn = !_audioManager.MuteSound;
			_soundToggle.onValueChanged.AddListener(value =>
			{
				_audioManager.MuteSound = !value;
				_audioManager.PlaySound("click_01");
			});

			_musicToggle.isOn = !_audioManager.MuteMusic;
			_musicToggle.onValueChanged.AddListener(value =>
			{
				_audioManager.MuteMusic = !value;
				_audioManager.PlaySound("click_01");
			});

			_localeService.Localize(gameObject, true);
		}

		protected override void OnDestroy()
		{
			_closeButton.onClick.RemoveAllListeners();
			_langRusToggle.onValueChanged.RemoveAllListeners();
			_langEngToggle.onValueChanged.RemoveAllListeners();
			_soundToggle.onValueChanged.RemoveAllListeners();
			_musicToggle.onValueChanged.RemoveAllListeners();

			base.OnDestroy();
		}
	}
}