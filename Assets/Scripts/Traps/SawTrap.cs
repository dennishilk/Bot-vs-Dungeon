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
        AudioManager.Instance?.PlaySawLoop(_loopSource);
    }

    private void OnEnable()
    {
        AudioManager.Instance?.PlaySawLoop(_loopSource);
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

        HandleBot(bot);
        _nextDamageTime = Time.time + tickRate;
    }

    public override void HandleBot(BotHealth botHealth)
    {
        botHealth.TakeDamage(damage);

        if (contactSpark != null)
        {
            contactSpark.transform.position = botHealth.transform.position + Vector3.up * 0.3f;
            contactSpark.Play();
        }
    }
}
