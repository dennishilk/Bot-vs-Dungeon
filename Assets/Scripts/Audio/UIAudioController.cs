using System;
using UnityEngine;

public class UIAudioController : MonoBehaviour
{
    [Serializable]
    private class UIClips
    {
        public AudioClip buttonClick;
        public AudioClip panelOpen;
        public AudioClip panelClose;
        public AudioClip achievementUnlocked;
        public AudioClip promotionEarned;
        public AudioClip resultAppear;
        public AudioClip simulationStart;
    }

    [SerializeField] private AudioSource uiSource;
    [SerializeField] private UIClips clips;

    private float _uiVolume = 1f;

    private void Awake()
    {
        if (uiSource == null)
        {
            return;
        }

        uiSource.playOnAwake = false;
        uiSource.spatialBlend = 0f;
    }

    public void SetUiVolume(float value)
    {
        _uiVolume = Mathf.Clamp01(value);
        if (uiSource != null)
        {
            uiSource.volume = _uiVolume;
        }
    }

    public void PlayEvent(UIAudioEvent uiEvent, float intensity = 1f)
    {
        if (uiSource == null)
        {
            return;
        }

        AudioClip clip = uiEvent switch
        {
            UIAudioEvent.ButtonClick => clips.buttonClick,
            UIAudioEvent.PanelOpen => clips.panelOpen,
            UIAudioEvent.PanelClose => clips.panelClose,
            UIAudioEvent.AchievementUnlocked => clips.achievementUnlocked,
            UIAudioEvent.PromotionEarned => clips.promotionEarned,
            UIAudioEvent.ResultAppear => clips.resultAppear,
            UIAudioEvent.SimulationStart => clips.simulationStart,
            _ => null
        };

        if (clip == null)
        {
            return;
        }

        uiSource.PlayOneShot(clip, Mathf.Clamp01(_uiVolume * intensity));
    }
}
