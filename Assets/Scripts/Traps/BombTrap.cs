using UnityEngine;

public class BombTrap : TrapBase
{
    [SerializeField] private float triggerRadius = 1.5f;
    [SerializeField] private bool explodeOnce = true;
    private bool _hasExploded;

    private void Update()
    {
        if (_hasExploded && explodeOnce)
        {
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius);
        foreach (Collider hit in hits)
        {
            BotHealth bot = hit.GetComponent<BotHealth>();
            if (bot == null)
            {
                continue;
            }

            HandleBot(bot);
            _hasExploded = true;
            if (explodeOnce)
            {
                gameObject.SetActive(false);
            }

            break;
        }
    }

    public override void HandleBot(BotHealth botHealth)
    {
        botHealth.TakeDamage(damage * 2f);
    }
}
