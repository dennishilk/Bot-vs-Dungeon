using UnityEngine;

public enum SoundCue
{
    SawLoop,
    BombArm,
    BombExplosion,
    ArcherFire,
    BotHurt,
    BotDeath,
    BotSuccess,
    UIButtonClick,
    SimulationStart,
    ResultFail,
    ResultVictory
}

public class AudioManager : MonoBehaviour
{
    [Header("Scene Audio Sources")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource trapSource;
    [SerializeField] private AudioSource resultSource;

    [Header("Trap Clips")]
    [SerializeField] private AudioClip sawLoopClip;
    [SerializeField] private AudioClip bombArmClip;
    [SerializeField] private AudioClip bombExplosionClip;
    [SerializeField] private AudioClip archerFireClip;

    [Header("Bot Clips")]
    [SerializeField] private AudioClip botHurtClip;
    [SerializeField] private AudioClip botDeathClip;
    [SerializeField] private AudioClip botSuccessClip;

    [Header("UI Clips")]
    [SerializeField] private AudioClip uiButtonClickClip;
    [SerializeField] private AudioClip simulationStartClip;

    [Header("Result Clips")]
    [SerializeField] private AudioClip resultFailClip;
    [SerializeField] private AudioClip resultVictoryClip;

    [Header("Recommended Volumes")]
    [Range(0f, 1f)] [SerializeField] private float trapVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float uiVolume = 0.7f;
    [Range(0f, 1f)] [SerializeField] private float resultVolume = 0.9f;

    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ConfigureSourceVolumes();
    }

    public void PlayUISound(SoundCue cue)
    {
        PlayCue(cue, uiSource);
    }

    public void PlayTrapSound(SoundCue cue)
    {
        PlayCue(cue, trapSource);
    }

    public void PlayResultSound(SoundCue cue)
    {
        PlayCue(cue, resultSource);
    }

    public void PlaySawLoop(AudioSource source)
    {
        if (source == null || sawLoopClip == null)
        {
            return;
        }

        if (source.isPlaying && source.clip == sawLoopClip)
        {
            return;
        }

        source.clip = sawLoopClip;
        source.loop = true;
        source.volume = trapVolume * 0.55f;
        source.Play();
    }

    private void ConfigureSourceVolumes()
    {
        if (uiSource != null)
        {
            uiSource.volume = uiVolume;
            uiSource.playOnAwake = false;
        }

        if (trapSource != null)
        {
            trapSource.volume = trapVolume;
            trapSource.playOnAwake = false;
        }

        if (resultSource != null)
        {
            resultSource.volume = resultVolume;
            resultSource.playOnAwake = false;
        }
    }

    private void PlayCue(SoundCue cue, AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        AudioClip clip = GetClip(cue);
        if (clip == null)
        {
            return;
        }

        source.PlayOneShot(clip);
    }

    private AudioClip GetClip(SoundCue cue)
    {
        return cue switch
        {
            SoundCue.SawLoop => sawLoopClip,
            SoundCue.BombArm => bombArmClip,
            SoundCue.BombExplosion => bombExplosionClip,
            SoundCue.ArcherFire => archerFireClip,
            SoundCue.BotHurt => botHurtClip,
            SoundCue.BotDeath => botDeathClip,
            SoundCue.BotSuccess => botSuccessClip,
            SoundCue.UIButtonClick => uiButtonClickClip,
            SoundCue.SimulationStart => simulationStartClip,
            SoundCue.ResultFail => resultFailClip,
            SoundCue.ResultVictory => resultVictoryClip,
            _ => null
        };
    }
}
