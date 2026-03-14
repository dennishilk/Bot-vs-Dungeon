using System;
using UnityEngine;

public class BotAudioController : MonoBehaviour
{
    [Serializable]
    private class BotEventClips
    {
        public AudioClip spawn;
        public AudioClip hurt;
        public AudioClip death;
        public AudioClip success;
        public AudioClip fall;
    }

    [Header("Routing")]
    [SerializeField] private AudioSource oneShotSource;

    [Header("Bot Audio")]
    [SerializeField] private BotEventClips clips;

    [Header("Personality Multipliers")]
    [SerializeField] private float carefulVolumeMultiplier = 0.7f;
    [SerializeField] private float balancedVolumeMultiplier = 1f;
    [SerializeField] private float recklessVolumeMultiplier = 1.2f;
    [SerializeField] private float panicVolumeMultiplier = 1.05f;

    private float _sfxVolume = 1f;

    private void Awake()
    {
        if (oneShotSource == null)
        {
            return;
        }

        oneShotSource.playOnAwake = false;
        oneShotSource.spatialBlend = 1f;
        oneShotSource.rolloffMode = AudioRolloffMode.Linear;
        oneShotSource.maxDistance = 16f;
    }

    public void SetSfxVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);
    }

    public void PlayEvent(BotAudioEvent audioEvent, BotPersonality personality, Vector3? worldPosition, float intensity)
    {
        if (oneShotSource == null)
        {
            return;
        }

        AudioClip clip = audioEvent switch
        {
            BotAudioEvent.Spawn => clips.spawn,
            BotAudioEvent.Hurt => clips.hurt,
            BotAudioEvent.Death => clips.death,
            BotAudioEvent.Success => clips.success,
            BotAudioEvent.Fall => clips.fall,
            _ => null
        };

        if (clip == null)
        {
            return;
        }

        if (worldPosition.HasValue)
        {
            oneShotSource.transform.position = worldPosition.Value;
        }

        float personalityFactor = personality switch
        {
            BotPersonality.Careful => carefulVolumeMultiplier,
            BotPersonality.Reckless => recklessVolumeMultiplier,
            BotPersonality.Panic => panicVolumeMultiplier,
            _ => balancedVolumeMultiplier
        };

        float volume = Mathf.Clamp01(intensity) * _sfxVolume * personalityFactor;
        oneShotSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }
}
