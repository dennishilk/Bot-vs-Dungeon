using System;
using System.Collections.Generic;
using UnityEngine;

public class TrapAudioController : MonoBehaviour
{
    [Serializable]
    private class TrapEventClipSet
    {
        public TrapSoundType trapType;
        public AudioClip idleLoop;
        public AudioClip activate;
        public AudioClip damage;
        public AudioClip arm;
        public AudioClip flight;
        public AudioClip impact;
        public AudioClip secondary;
    }

    [Header("Routing")]
    [SerializeField] private AudioSource worldOneShotSource;
    [SerializeField] private AudioSource loopSourcePrefab;
    [SerializeField] private Transform loopParent;

    [Header("Trap Mapping")]
    [SerializeField] private TrapEventClipSet[] trapAudioMap;

    [Header("Spatial Tuning")]
    [SerializeField] private float oneShotMinDistance = 1.5f;
    [SerializeField] private float oneShotMaxDistance = 18f;

    private readonly Dictionary<TrapSoundType, TrapEventClipSet> _cache = new();
    private readonly Dictionary<Transform, AudioSource> _activeLoops = new();
    private float _sfxVolume = 1f;

    private void Awake()
    {
        if (worldOneShotSource != null)
        {
            worldOneShotSource.playOnAwake = false;
            worldOneShotSource.spatialBlend = 1f;
            worldOneShotSource.rolloffMode = AudioRolloffMode.Linear;
            worldOneShotSource.minDistance = oneShotMinDistance;
            worldOneShotSource.maxDistance = oneShotMaxDistance;
        }

        _cache.Clear();
        if (trapAudioMap == null)
        {
            return;
        }

        foreach (TrapEventClipSet entry in trapAudioMap)
        {
            if (entry == null)
            {
                continue;
            }

            _cache[entry.trapType] = entry;
        }
    }

    public void SetSfxVolume(float value)
    {
        _sfxVolume = Mathf.Clamp01(value);
        foreach (KeyValuePair<Transform, AudioSource> entry in _activeLoops)
        {
            AudioSource source = entry.Value;
            if (source != null)
            {
                source.volume = _sfxVolume * 0.35f;
            }
        }
    }

    public void PlayTrap(TrapSoundType type, TrapSoundEvent trapEvent, Vector3? worldPosition, float intensity)
    {
        AudioClip clip = ResolveClip(type, trapEvent);
        if (clip == null)
        {
            return;
        }

        if (worldOneShotSource == null)
        {
            return;
        }

        if (worldPosition.HasValue)
        {
            worldOneShotSource.transform.position = worldPosition.Value;
        }

        float volume = Mathf.Clamp01(intensity) * _sfxVolume;
        worldOneShotSource.PlayOneShot(clip, volume);
    }

    public void StartLoop(TrapSoundType type, Transform anchor, float intensity)
    {
        if (anchor == null)
        {
            return;
        }

        AudioClip loopClip = ResolveClip(type, TrapSoundEvent.IdleLoop);
        if (loopClip == null || loopSourcePrefab == null)
        {
            return;
        }

        if (_activeLoops.TryGetValue(anchor, out AudioSource existingSource) && existingSource != null)
        {
            existingSource.clip = loopClip;
            existingSource.volume = Mathf.Clamp01(intensity) * _sfxVolume * 0.35f;
            if (!existingSource.isPlaying)
            {
                existingSource.Play();
            }
            return;
        }

        AudioSource source = Instantiate(loopSourcePrefab, anchor.position, Quaternion.identity, loopParent != null ? loopParent : transform);
        source.transform.SetParent(anchor);
        source.transform.localPosition = Vector3.zero;
        source.clip = loopClip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = oneShotMinDistance;
        source.maxDistance = oneShotMaxDistance;
        source.volume = Mathf.Clamp01(intensity) * _sfxVolume * 0.35f;
        source.Play();

        _activeLoops[anchor] = source;
    }

    public void StopLoop(TrapSoundType type, Transform anchor)
    {
        if (anchor == null || !_activeLoops.TryGetValue(anchor, out AudioSource source))
        {
            return;
        }

        if (source != null)
        {
            source.Stop();
            Destroy(source.gameObject);
        }

        _activeLoops.Remove(anchor);
    }

    private AudioClip ResolveClip(TrapSoundType type, TrapSoundEvent trapEvent)
    {
        if (!_cache.TryGetValue(type, out TrapEventClipSet clipSet))
        {
            return null;
        }

        return trapEvent switch
        {
            TrapSoundEvent.IdleLoop => clipSet.idleLoop,
            TrapSoundEvent.Activate => clipSet.activate,
            TrapSoundEvent.Damage => clipSet.damage,
            TrapSoundEvent.Arm => clipSet.arm,
            TrapSoundEvent.Flight => clipSet.flight,
            TrapSoundEvent.Impact => clipSet.impact,
            TrapSoundEvent.Secondary => clipSet.secondary,
            _ => null
        };
    }
}
