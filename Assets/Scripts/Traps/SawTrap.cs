using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SawTrap : TrapBase
{
    [SerializeField] private float tickRate = 0.5f;
    [SerializeField] private ParticleSystem contactSpark;

    private float _nextDamageTime;
    private AudioSource _loopSource;

    private void Awake()
    {
        _loopSource = GetComponent<AudioSource>();
        _loopSource.spatialBlend = 1f;
        AudioManager.Instance?.StartTrapLoop(TrapSoundType.SawTrap, transform);
        AudioManager.Instance?.PlaySawLoop(_loopSource);
    }

    private void OnEnable()
    {
        AudioManager.Instance?.StartTrapLoop(TrapSoundType.SawTrap, transform);
        AudioManager.Instance?.PlaySawLoop(_loopSource);
    }


    private void OnDisable()
    {
        AudioManager.Instance?.StopTrapLoop(TrapSoundType.SawTrap, transform);
    }

    private void OnTriggerStay(Collider other)
    {
        if (Time.time < _nextDamageTime)
        {
            return;
        }

        BotHealth bot = other.GetComponent<BotHealth>();
        if (bot == null)
        {
            return;
        }

        EventLogger.Instance?.Log("Bot entered saw trap");
        HandleBot(bot);
        _nextDamageTime = Time.time + tickRate;
    }

    public override void HandleBot(BotHealth botHealth)
    {
        EventLogger.Instance?.Log($"Trap activated: saw ({damage:0})");
        botHealth.TakeDamage(damage, DamageSource.SawTrap);
        AudioManager.Instance?.PlayTrap(TrapSoundType.SawTrap, TrapSoundEvent.Damage, botHealth.transform.position, 0.8f);
        EventLogger.Instance?.Log($"Bot took {damage:0} damage");

        if (contactSpark != null)
        {
            contactSpark.transform.position = botHealth.transform.position + Vector3.up * 0.3f;
            contactSpark.Play();
        }
    }
}
