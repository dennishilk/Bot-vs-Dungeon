using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BotVsDungeon.AppFlow
{
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider ambientVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Display")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private TMP_Dropdown resolutionDropdown;

        [Header("Gameplay")]
        [SerializeField] private Slider replaySpeedSlider;
        [SerializeField] private Slider uiScaleSlider;
        [SerializeField] private TMP_Text statusText;

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private bool saveOnEachChange = true;

        private Resolution[] _resolutions;

        private const string MasterKey = "settings.audio.master";
        private const string MusicKey = "settings.audio.music";
        private const string AmbientKey = "settings.audio.ambient";
        private const string SfxKey = "settings.audio.sfx";
        private const string FullscreenKey = "settings.display.fullscreen";
        private const string VsyncKey = "settings.display.vsync";
        private const string ResolutionKey = "settings.display.resolution";
        private const string ReplaySpeedKey = "settings.gameplay.replaySpeed";
        private const string UiScaleKey = "settings.gameplay.uiScale";

        private void OnEnable()
        {
            LoadIntoUi();
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }
        }

        public void ApplyClicked()
        {
            ApplySettings();
            SaveSettings();
            SetStatus("Settings applied.", true);
        }

        public void RevertClicked()
        {
            LoadIntoUi();
            SetStatus("Settings reverted.", true);
        }

        public void OnAnyValueChanged()
        {
            if (!saveOnEachChange)
            {
                return;
            }

            ApplySettings();
            SaveSettings();
            SetStatus("Settings saved.", true);
        }

        private void LoadIntoUi()
        {
            SetSlider(masterVolumeSlider, PlayerPrefs.GetFloat(MasterKey, 1f));
            SetSlider(musicVolumeSlider, PlayerPrefs.GetFloat(MusicKey, 0.8f));
            SetSlider(ambientVolumeSlider, PlayerPrefs.GetFloat(AmbientKey, 0.8f));
            SetSlider(sfxVolumeSlider, PlayerPrefs.GetFloat(SfxKey, 0.8f));
            SetSlider(replaySpeedSlider, PlayerPrefs.GetFloat(ReplaySpeedKey, 1f));
            SetSlider(uiScaleSlider, PlayerPrefs.GetFloat(UiScaleKey, 1f));

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(FullscreenKey, 1) == 1);
            }

            if (vsyncToggle != null)
            {
                vsyncToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(VsyncKey, 1) == 1);
            }

            _resolutions = Screen.resolutions;
            if (resolutionDropdown != null)
            {
                resolutionDropdown.ClearOptions();
                int selected = PlayerPrefs.GetInt(ResolutionKey, _resolutions.Length > 0 ? _resolutions.Length - 1 : 0);
                var options = new System.Collections.Generic.List<string>();
                for (int i = 0; i < _resolutions.Length; i++)
                {
                    Resolution r = _resolutions[i];
                    options.Add($"{r.width}x{r.height} @ {r.refreshRateRatio.value:0}Hz");
                }

                if (options.Count == 0)
                {
                    options.Add("Current Resolution");
                }

                resolutionDropdown.AddOptions(options);
                resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(selected, 0, options.Count - 1));
            }

            ApplySettings();
        }

        private void ApplySettings()
        {
            AudioManager.Instance?.SetVolume(AudioLayer.Master, masterVolumeSlider != null ? masterVolumeSlider.value : 1f);
            AudioManager.Instance?.SetVolume(AudioLayer.Music, musicVolumeSlider != null ? musicVolumeSlider.value : 0.8f);
            AudioManager.Instance?.SetVolume(AudioLayer.Ambient, ambientVolumeSlider != null ? ambientVolumeSlider.value : 0.8f);
            AudioManager.Instance?.SetVolume(AudioLayer.Sfx, sfxVolumeSlider != null ? sfxVolumeSlider.value : 0.8f);

            bool fullscreen = fullscreenToggle == null || fullscreenToggle.isOn;
            bool vsync = vsyncToggle == null || vsyncToggle.isOn;
            QualitySettings.vSyncCount = vsync ? 1 : 0;

            if (_resolutions != null && _resolutions.Length > 0 && resolutionDropdown != null)
            {
                Resolution res = _resolutions[Mathf.Clamp(resolutionDropdown.value, 0, _resolutions.Length - 1)];
                Screen.SetResolution(res.width, res.height, fullscreen);
            }
            else
            {
                Screen.fullScreen = fullscreen;
            }

            float replaySpeed = replaySpeedSlider != null ? replaySpeedSlider.value : 1f;
            ReplayViewer replayViewer = FindFirstObjectByType<ReplayViewer>();
            replayViewer?.SetReplaySpeed(replaySpeed);

            CanvasScaler scaler = FindFirstObjectByType<CanvasScaler>();
            if (scaler != null)
            {
                scaler.scaleFactor = uiScaleSlider != null ? uiScaleSlider.value : 1f;
            }
        }

        private void SaveSettings()
        {
            PlayerPrefs.SetFloat(MasterKey, masterVolumeSlider != null ? masterVolumeSlider.value : 1f);
            PlayerPrefs.SetFloat(MusicKey, musicVolumeSlider != null ? musicVolumeSlider.value : 0.8f);
            PlayerPrefs.SetFloat(AmbientKey, ambientVolumeSlider != null ? ambientVolumeSlider.value : 0.8f);
            PlayerPrefs.SetFloat(SfxKey, sfxVolumeSlider != null ? sfxVolumeSlider.value : 0.8f);
            PlayerPrefs.SetFloat(ReplaySpeedKey, replaySpeedSlider != null ? replaySpeedSlider.value : 1f);
            PlayerPrefs.SetFloat(UiScaleKey, uiScaleSlider != null ? uiScaleSlider.value : 1f);
            PlayerPrefs.SetInt(FullscreenKey, fullscreenToggle != null && fullscreenToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(VsyncKey, vsyncToggle != null && vsyncToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt(ResolutionKey, resolutionDropdown != null ? resolutionDropdown.value : 0);
            PlayerPrefs.Save();
        }

        private static void SetSlider(Slider slider, float value)
        {
            if (slider != null)
            {
                slider.SetValueWithoutNotify(value);
            }
        }

        private void SetStatus(string text, bool success)
        {
            if (statusText != null)
            {
                statusText.text = text;
                statusText.color = success ? new Color(0.5f, 0.95f, 0.6f) : new Color(0.95f, 0.45f, 0.45f);
            }
        }
    }
}
