using UnityEngine;

public class ArcherTrap : TrapBase
{
    [SerializeField] private float shootInterval = 1.25f;
    [SerializeField] private float range = 6f;
    private float _nextShotTime;

    private void Update()
    {
        if (Time.time < _nextShotTime)
        {
            return;
        }

        _nextShotTime = Time.time + shootInterval;

        if (!Physics.Raycast(transform.position + Vector3.up * 0.5f, transform.forward, out RaycastHit hit, range))
        {
            return;
        }

        BotHealth bot = hit.collider.GetComponent<BotHealth>();
        if (bot != null)
        {
            HandleBot(bot);
        }
    }

    public override void HandleBot(BotHealth botHealth)
    {
        botHealth.TakeDamage(damage);
    }
}
