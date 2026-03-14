using System.Collections.Generic;
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

public enum AudioLayer
{
    Master,
    Music,
    Ambient,
    Sfx,
    Ui
}

public enum TrapSoundType
{
    SawTrap,
    BombTrap,
    ArcherTrap,
    SpikeTrap,
    FlameJetTrap,
    TeleportTrap,
    PressurePlate,
    FakeFloor
}

public enum TrapSoundEvent
{
    IdleLoop,
    Activate,
    Damage,
    Arm,
    Flight,
    Impact,
    Secondary
}

public enum BotAudioEvent
{
    Spawn,
    Hurt,
    Death,
    Success,
    Fall
}

public enum UIAudioEvent
{
    ButtonClick,
    PanelOpen,
    PanelClose,
    AchievementUnlocked,
    PromotionEarned,
    ResultAppear,
    SimulationStart
}

public enum MusicTrackType
{
    Menu,
    Gameplay,
    Results,
    PromotionFanfare,
    StressTension
}

public enum AnnouncerEvent
{
    CertificationInitiated,
    BotFatalityRecorded,
    SurvivabilityThresholdAchieved,
    ComplianceRatingApproved
}

public class AudioManager : MonoBehaviour
{
    private const string MasterVolumeKey = "audio_master";
    private const string MusicVolumeKey = "audio_music";
    private const string AmbientVolumeKey = "audio_ambient";
    private const string SfxVolumeKey = "audio_sfx";
    private const string UiVolumeKey = "audio_ui";

    [Header("Core Controllers")]
    [SerializeField] private AmbientAudioController ambientAudioController;
    [SerializeField] private TrapAudioController trapAudioController;
    [SerializeField] private BotAudioController botAudioController;
    [SerializeField] private UIAudioController uiAudioController;
    [SerializeField] private AnnouncerSystem announcerSystem;

    [Header("Legacy Scene Audio Sources")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioSource trapSource;
    [SerializeField] private AudioSource resultSource;

    [Header("Legacy Clips")]
    [SerializeField] private AudioClip sawLoopClip;
    [SerializeField] private AudioClip bombArmClip;
    [SerializeField] private AudioClip bombExplosionClip;
    [SerializeField] private AudioClip archerFireClip;
    [SerializeField] private AudioClip botHurtClip;
    [SerializeField] private AudioClip botDeathClip;
    [SerializeField] private AudioClip botSuccessClip;
    [SerializeField] private AudioClip uiButtonClickClip;
    [SerializeField] private AudioClip simulationStartClip;
    [SerializeField] private AudioClip resultFailClip;
    [SerializeField] private AudioClip resultVictoryClip;

    [Header("Volume Defaults")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float ambientVolume = 0.45f;
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float uiVolume = 0.7f;

    [Header("Dynamic Events")]
    [SerializeField] private AudioClip highFatalitySting;
    [SerializeField] private AudioClip objectiveReachedResonance;
    [SerializeField] private AudioClip stressTestRisingTensionSting;
    [SerializeField] private float fatalityWindowSeconds = 4f;
    [SerializeField] private int fatalityThreshold = 3;

    public static AudioManager Instance { get; private set; }

    private readonly List<float> _recentDeathTimes = new();

    public float MasterVolume => masterVolume;
    public float MusicVolume => musicVolume;
    public float AmbientVolume => ambientVolume;
    public float SfxVolume => sfxVolume;
    public float UiVolume => uiVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadSavedVolumes();
        ApplyAllVolumes();
    }

    public void PlayUISound(SoundCue cue)
    {
        switch (cue)
        {
            case SoundCue.UIButtonClick:
                PlayUI(UIAudioEvent.ButtonClick);
                break;
            case SoundCue.SimulationStart:
                PlayUI(UIAudioEvent.SimulationStart);
                break;
            default:
                PlayLegacyCue(cue, uiSource);
                break;
        }
    }

    public void PlayTrapSound(SoundCue cue)
    {
        switch (cue)
        {
            case SoundCue.BombArm:
                PlayTrap(TrapSoundType.BombTrap, TrapSoundEvent.Arm);
                break;
            case SoundCue.BombExplosion:
                PlayTrap(TrapSoundType.BombTrap, TrapSoundEvent.Impact);
                break;
            case SoundCue.ArcherFire:
                PlayTrap(TrapSoundType.ArcherTrap, TrapSoundEvent.Activate);
                break;
            case SoundCue.BotHurt:
                PlayBotEvent(BotAudioEvent.Hurt, BotPersonality.Balanced);
                break;
            default:
                PlayLegacyCue(cue, trapSource);
                break;
        }
    }

    public void PlayResultSound(SoundCue cue)
    {
        switch (cue)
        {
            case SoundCue.ResultVictory:
                PlayUI(UIAudioEvent.ResultAppear);
                PlayMusicTrack(MusicTrackType.Results);
                break;
            case SoundCue.ResultFail:
                PlayUI(UIAudioEvent.ResultAppear);
                PlayMusicTrack(MusicTrackType.Results);
                break;
            case SoundCue.BotDeath:
                PlayBotEvent(BotAudioEvent.Death, BotPersonality.Balanced);
                RegisterBotFatality();
                break;
            case SoundCue.BotSuccess:
                PlayBotEvent(BotAudioEvent.Success, BotPersonality.Balanced);
                TriggerGoalReachedResonance();
                break;
            default:
                PlayLegacyCue(cue, resultSource);
                break;
        }
    }

    public void PlaySawLoop(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.spatialBlend = 1f;
        source.loop = true;
        source.volume = EffectiveSfxVolume * 0.45f;

        if (source.isPlaying)
        {
            return;
        }

        if (sawLoopClip != null)
        {
            source.clip = sawLoopClip;
        }

        if (source.clip != null)
        {
            source.Play();
        }
    }

    public void PlayTrap(TrapSoundType type, TrapSoundEvent trapEvent, Vector3? worldPosition = null, float intensity = 1f)
    {
        trapAudioController?.PlayTrap(type, trapEvent, worldPosition, intensity * EffectiveSfxVolume);
    }

    public void StartTrapLoop(TrapSoundType type, Transform loopAnchor, float intensity = 1f)
    {
        trapAudioController?.StartLoop(type, loopAnchor, intensity * EffectiveSfxVolume);
    }

    public void StopTrapLoop(TrapSoundType type, Transform loopAnchor)
    {
        trapAudioController?.StopLoop(type, loopAnchor);
    }

    public void PlayBotEvent(BotAudioEvent botAudioEvent, BotPersonality personality, Vector3? worldPosition = null)
    {
        botAudioController?.PlayEvent(botAudioEvent, personality, worldPosition, EffectiveSfxVolume);
    }

    public void PlayUI(UIAudioEvent uiAudioEvent)
    {
        uiAudioController?.PlayEvent(uiAudioEvent, EffectiveUiVolume);
    }

    public void PlayMusicTrack(MusicTrackType track)
    {
        ambientAudioController?.SetMusicTrack(track, EffectiveMusicVolume);
    }

    public void SetStressTestTension(bool enabled)
    {
        ambientAudioController?.SetStressTensionLayer(enabled, EffectiveAmbientVolume);

        if (enabled && stressTestRisingTensionSting != null && resultSource != null)
        {
            resultSource.PlayOneShot(stressTestRisingTensionSting, EffectiveMusicVolume * 0.6f);
        }
    }

    public void TriggerGoalReachedResonance()
    {
        if (objectiveReachedResonance != null && resultSource != null)
        {
            resultSource.PlayOneShot(objectiveReachedResonance, EffectiveMusicVolume * 0.7f);
        }
    }

    public void QueueAnnouncement(AnnouncerEvent announcerEvent)
    {
        announcerSystem?.Announce(announcerEvent, EffectiveUiVolume);
    }

    public void RegisterBotFatality()
    {
        _recentDeathTimes.Add(Time.time);
        _recentDeathTimes.RemoveAll(time => Time.time - time > fatalityWindowSeconds);

        if (_recentDeathTimes.Count < fatalityThreshold)
        {
            return;
        }

        if (highFatalitySting != null && resultSource != null)
        {
            resultSource.PlayOneShot(highFatalitySting, EffectiveMusicVolume * 0.75f);
        }

        _recentDeathTimes.Clear();
    }

    public void SetVolume(AudioLayer layer, float value, bool save = true)
    {
        float clamped = Mathf.Clamp01(value);
        switch (layer)
        {
            case AudioLayer.Master:
                masterVolume = clamped;
                break;
            case AudioLayer.Music:
                musicVolume = clamped;
                break;
            case AudioLayer.Ambient:
                ambientVolume = clamped;
                break;
            case AudioLayer.Sfx:
                sfxVolume = clamped;
                break;
            case AudioLayer.Ui:
                uiVolume = clamped;
                break;
        }

        ApplyAllVolumes();

        if (save)
        {
            SaveVolumes();
        }
    }

    public void SaveVolumes()
    {
        PlayerPrefs.SetFloat(MasterVolumeKey, masterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
        PlayerPrefs.SetFloat(AmbientVolumeKey, ambientVolume);
        PlayerPrefs.SetFloat(SfxVolumeKey, sfxVolume);
        PlayerPrefs.SetFloat(UiVolumeKey, uiVolume);
        PlayerPrefs.Save();
    }

    private float EffectiveMusicVolume => masterVolume * musicVolume;
    private float EffectiveAmbientVolume => masterVolume * ambientVolume;
    private float EffectiveSfxVolume => masterVolume * sfxVolume;
    private float EffectiveUiVolume => masterVolume * uiVolume;

    private void LoadSavedVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
        ambientVolume = PlayerPrefs.GetFloat(AmbientVolumeKey, ambientVolume);
        sfxVolume = PlayerPrefs.GetFloat(SfxVolumeKey, sfxVolume);
        uiVolume = PlayerPrefs.GetFloat(UiVolumeKey, uiVolume);
    }

    private void ApplyAllVolumes()
    {
        ambientAudioController?.SetAmbientVolume(EffectiveAmbientVolume);
        ambientAudioController?.SetMusicVolume(EffectiveMusicVolume);
        trapAudioController?.SetSfxVolume(EffectiveSfxVolume);
        botAudioController?.SetSfxVolume(EffectiveSfxVolume);
        uiAudioController?.SetUiVolume(EffectiveUiVolume);

        if (uiSource != null)
        {
            uiSource.volume = EffectiveUiVolume;
            uiSource.playOnAwake = false;
        }

        if (trapSource != null)
        {
            trapSource.volume = EffectiveSfxVolume;
            trapSource.playOnAwake = false;
        }

        if (resultSource != null)
        {
            resultSource.volume = Mathf.Max(EffectiveMusicVolume, EffectiveSfxVolume);
            resultSource.playOnAwake = false;
        }
    }

    private void PlayLegacyCue(SoundCue cue, AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        AudioClip clip = GetLegacyClip(cue);
        if (clip == null)
        {
            return;
        }

        source.PlayOneShot(clip);
    }

    private AudioClip GetLegacyClip(SoundCue cue)
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
