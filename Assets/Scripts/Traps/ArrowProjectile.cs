using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private ParticleSystem impactParticle;

    private float _damage;

    public void Initialize(float damage)
    {
        _damage = damage;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        BotHealth bot = other.GetComponent<BotHealth>();
        if (bot != null)
        {
            bot.TakeDamage(_damage, DamageSource.ArcherTrap);
        }

        AudioManager.Instance?.PlayTrap(TrapSoundType.ArcherTrap, TrapSoundEvent.Impact, transform.position, 0.8f);

        if (impactParticle != null)
        {
            impactParticle.transform.parent = null;
            impactParticle.Play();
            Destroy(impactParticle.gameObject, 1f);
        }

        Destroy(gameObject);
    }
}
