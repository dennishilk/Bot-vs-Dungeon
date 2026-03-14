using System.Collections;
using UnityEngine;

public class AmbientAudioController : MonoBehaviour
{
    [Header("Ambient Dungeon Layer")]
    [SerializeField] private AudioSource windLayer;
    [SerializeField] private AudioSource cavernRumbleLayer;
    [SerializeField] private AudioSource mechanicalHumLayer;
    [SerializeField] private AudioSource stoneResonanceLayer;

    [Header("Environment Detail Layer")]
    [SerializeField] private AudioSource environmentOneShotSource;
    [SerializeField] private AudioClip[] environmentalDetailClips;
    [SerializeField] private Transform[] environmentEmitters;
    [SerializeField] private Vector2 environmentSoundIntervalRange = new(5f, 11f);
    [SerializeField] private Vector2 environmentVolumeRange = new(0.2f, 0.45f);

    [Header("Music Layer")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip dungeonMusic;
    [SerializeField] private AudioClip resultMusic;
    [SerializeField] private AudioClip promotionFanfare;
    [SerializeField] private AudioClip stressTensionMusic;
    [SerializeField] private float musicFadeDuration = 1.2f;

    [Header("Dynamic Layer")]
    [SerializeField] private AudioSource stressTensionLayer;

    [Header("Volume Tuning")]
    [Range(0f, 1f)] [SerializeField] private float ambientLayerScale = 0.45f;
    [Range(0f, 1f)] [SerializeField] private float musicLayerScale = 0.4f;

    private float _ambientVolume = 0.45f;
    private float _musicVolume = 0.4f;
    private Coroutine _environmentCoroutine;
    private Coroutine _musicFadeRoutine;

    private void Start()
    {
        ConfigureLoop(windLayer, 0.55f);
        ConfigureLoop(cavernRumbleLayer, 0.4f);
        ConfigureLoop(mechanicalHumLayer, 0.45f);
        ConfigureLoop(stoneResonanceLayer, 0.35f);
        ConfigureOneShotSource();

        _environmentCoroutine = StartCoroutine(EnvironmentDetailLoopRoutine());
        SetMusicTrack(MusicTrackType.Menu, _musicVolume);
    }

    private void OnDisable()
    {
        if (_environmentCoroutine != null)
        {
            StopCoroutine(_environmentCoroutine);
            _environmentCoroutine = null;
        }
    }

    public void SetAmbientVolume(float volume)
    {
        _ambientVolume = Mathf.Clamp01(volume);
        ConfigureLoop(windLayer, 0.55f);
        ConfigureLoop(cavernRumbleLayer, 0.4f);
        ConfigureLoop(mechanicalHumLayer, 0.45f);
        ConfigureLoop(stoneResonanceLayer, 0.35f);

        if (stressTensionLayer != null)
        {
            stressTensionLayer.volume = _ambientVolume * 0.25f;
        }
    }

    public void SetMusicVolume(float volume)
    {
        _musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = _musicVolume * musicLayerScale;
        }
    }

    public void SetMusicTrack(MusicTrackType track, float effectiveVolume)
    {
        if (musicSource == null)
        {
            return;
        }

        _musicVolume = Mathf.Clamp01(effectiveVolume);
        AudioClip target = track switch
        {
            MusicTrackType.Menu => menuMusic,
            MusicTrackType.Gameplay => dungeonMusic,
            MusicTrackType.Results => resultMusic,
            MusicTrackType.PromotionFanfare => promotionFanfare,
            MusicTrackType.StressTension => stressTensionMusic,
            _ => dungeonMusic
        };

        if (target == null || (musicSource.clip == target && musicSource.isPlaying))
        {
            return;
        }

        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
        }

        _musicFadeRoutine = StartCoroutine(FadeMusicRoutine(target, track == MusicTrackType.PromotionFanfare));
    }

    public void SetStressTensionLayer(bool enabled, float effectiveAmbientVolume)
    {
        if (stressTensionLayer == null)
        {
            return;
        }

        _ambientVolume = Mathf.Clamp01(effectiveAmbientVolume);
        stressTensionLayer.volume = _ambientVolume * 0.25f;

        if (enabled)
        {
            if (!stressTensionLayer.isPlaying)
            {
                stressTensionLayer.Play();
            }
            return;
        }

        stressTensionLayer.Stop();
    }

    private void ConfigureLoop(AudioSource source, float weight)
    {
        if (source == null)
        {
            return;
        }

        source.loop = true;
        source.playOnAwake = false;
        source.volume = _ambientVolume * ambientLayerScale * weight;

        if (source.clip != null && !source.isPlaying)
        {
            source.Play();
        }
    }

    private void ConfigureOneShotSource()
    {
        if (environmentOneShotSource == null)
        {
            return;
        }

        environmentOneShotSource.playOnAwake = false;
        environmentOneShotSource.loop = false;
        environmentOneShotSource.spatialBlend = 1f;
        environmentOneShotSource.rolloffMode = AudioRolloffMode.Linear;
        environmentOneShotSource.maxDistance = 14f;
    }

    private IEnumerator EnvironmentDetailLoopRoutine()
    {
        while (true)
        {
            float wait = Random.Range(environmentSoundIntervalRange.x, environmentSoundIntervalRange.y);
            yield return new WaitForSeconds(wait);

            if (environmentalDetailClips == null || environmentalDetailClips.Length == 0 || environmentOneShotSource == null)
            {
                continue;
            }

            AudioClip clip = environmentalDetailClips[Random.Range(0, environmentalDetailClips.Length)];
            if (clip == null)
            {
                continue;
            }

            environmentOneShotSource.transform.position = GetEnvironmentEmitterPosition();
            float oneShotVolume = Random.Range(environmentVolumeRange.x, environmentVolumeRange.y) * _ambientVolume;
            environmentOneShotSource.PlayOneShot(clip, oneShotVolume);
        }
    }

    private Vector3 GetEnvironmentEmitterPosition()
    {
        if (environmentEmitters == null || environmentEmitters.Length == 0)
        {
            return transform.position;
        }

        Transform emitter = environmentEmitters[Random.Range(0, environmentEmitters.Length)];
        return emitter != null ? emitter.position : transform.position;
    }

    private IEnumerator FadeMusicRoutine(AudioClip targetClip, bool isStinger)
    {
        float startVolume = musicSource.volume;
        float fadeOutDuration = Mathf.Max(0.01f, musicFadeDuration * 0.5f);

        for (float elapsed = 0f; elapsed < fadeOutDuration; elapsed += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = targetClip;
        musicSource.loop = !isStinger;
        musicSource.Play();

        float targetVolume = _musicVolume * musicLayerScale;
        float fadeInDuration = Mathf.Max(0.01f, musicFadeDuration);
        for (float elapsed = 0f; elapsed < fadeInDuration; elapsed += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeInDuration);
            yield return null;
        }

        musicSource.volume = targetVolume;
        _musicFadeRoutine = null;
    }
}
