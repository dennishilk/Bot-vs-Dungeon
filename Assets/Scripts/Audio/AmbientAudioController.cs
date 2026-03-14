using UnityEngine;

public enum AtmosphereMusicState
{
    Menu,
    Gameplay,
    Results
}

public class AmbientAudioController : MonoBehaviour
{
    [Header("Ambient Layers")]
    [SerializeField] private AudioSource windLayer;
    [SerializeField] private AudioSource dripLayer;
    [SerializeField] private AudioSource chainLayer;
    [SerializeField] private AudioSource dungeonHumLayer;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameplayMusic;
    [SerializeField] private AudioClip resultStinger;

    [Header("Tuning")]
    [Range(0f, 1f)] [SerializeField] private float ambientMaster = 0.45f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.35f;

    private void Start()
    {
        ConfigureAmbientLayer(windLayer, 0.6f);
        ConfigureAmbientLayer(dripLayer, 0.5f);
        ConfigureAmbientLayer(chainLayer, 0.35f);
        ConfigureAmbientLayer(dungeonHumLayer, 0.45f);

        SetMusicState(AtmosphereMusicState.Menu, immediate: true);
    }

    public void SetMusicState(AtmosphereMusicState state, bool immediate = false)
    {
        if (musicSource == null)
        {
            return;
        }

        AudioClip target = state switch
        {
            AtmosphereMusicState.Menu => menuMusic,
            AtmosphereMusicState.Gameplay => gameplayMusic,
            AtmosphereMusicState.Results => resultStinger,
            _ => gameplayMusic
        };

        if (target == null)
        {
            return;
        }

        if (immediate)
        {
            musicSource.clip = target;
            musicSource.loop = state != AtmosphereMusicState.Results;
            musicSource.volume = musicVolume;
            musicSource.Play();
            return;
        }

        if (musicSource.clip == target && musicSource.isPlaying)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = target;
        musicSource.loop = state != AtmosphereMusicState.Results;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    private void ConfigureAmbientLayer(AudioSource source, float layerWeight)
    {
        if (source == null)
        {
            return;
        }

        source.loop = true;
        source.volume = ambientMaster * layerWeight;

        if (!source.isPlaying && source.clip != null)
        {
            source.Play();
        }
    }
}
