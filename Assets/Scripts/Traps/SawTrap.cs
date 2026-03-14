using UnityEngine;

public class SawTrap : TrapBase
{
    [SerializeField] private float tickRate = 0.5f;
    private float _nextDamageTime;

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
    }
}
