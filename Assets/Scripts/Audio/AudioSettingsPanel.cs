using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider ambientSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    private void OnEnable()
    {
        SyncFromAudioManager();
    }

    public void SetMasterVolume(float value)
    {
        AudioManager.Instance?.SetVolume(AudioLayer.Master, value);
    }

    public void SetMusicVolume(float value)
    {
        AudioManager.Instance?.SetVolume(AudioLayer.Music, value);
    }

    public void SetAmbientVolume(float value)
    {
        AudioManager.Instance?.SetVolume(AudioLayer.Ambient, value);
    }

    public void SetSfxVolume(float value)
    {
        AudioManager.Instance?.SetVolume(AudioLayer.Sfx, value);
    }

    public void SetUiVolume(float value)
    {
        AudioManager.Instance?.SetVolume(AudioLayer.Ui, value);
    }

    public void SyncFromAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            return;
        }

        SetSlider(masterSlider, AudioManager.Instance.MasterVolume);
        SetSlider(musicSlider, AudioManager.Instance.MusicVolume);
        SetSlider(ambientSlider, AudioManager.Instance.AmbientVolume);
        SetSlider(sfxSlider, AudioManager.Instance.SfxVolume);
        SetSlider(uiSlider, AudioManager.Instance.UiVolume);
    }

    private static void SetSlider(Slider slider, float value)
    {
        if (slider == null)
        {
            return;
        }

        slider.SetValueWithoutNotify(value);
    }
}
