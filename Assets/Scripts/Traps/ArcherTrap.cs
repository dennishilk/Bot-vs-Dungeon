using System.Collections;
using UnityEngine;

public class ArcherTrap : TrapBase
{
    [SerializeField] private float shootInterval = 1.25f;
    [SerializeField] private float range = 6f;
    [SerializeField] private ArrowProjectile arrowProjectilePrefab;
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private Transform archerVisual;
    [SerializeField] private float recoilDistance = 0.08f;
    [SerializeField] private float recoilDuration = 0.06f;

    private float _nextShotTime;
    private Vector3 _visualLocalStart;

    private void Awake()
    {
        if (archerVisual != null)
        {
            _visualLocalStart = archerVisual.localPosition;
        }
    }

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
            EventLogger.Instance?.Log("Trap activated: archer");
            ReplayEventStream.Emit(ReplayEventType.TrapActivated, transform.position, "ArcherTrap", damage, "Archer fired");
            HandleBot(bot);
        }
    }

    public override void HandleBot(BotHealth botHealth)
    {
        AudioManager.Instance?.PlayTrap(TrapSoundType.ArcherTrap, TrapSoundEvent.Activate, transform.position, 0.9f);
        AudioManager.Instance?.PlayTrapSound(SoundCue.ArcherFire);

        if (muzzlePoint != null && arrowProjectilePrefab != null)
        {
            ArrowProjectile projectile = Instantiate(arrowProjectilePrefab, muzzlePoint.position, muzzlePoint.rotation);
            projectile.Initialize(damage);
            AudioManager.Instance?.PlayTrap(TrapSoundType.ArcherTrap, TrapSoundEvent.Flight, muzzlePoint.position, 0.6f);
        }
        else
        {
            botHealth.TakeDamage(damage, DamageSource.ArcherTrap);
            EventLogger.Instance?.Log($"Bot took {damage:0} damage");
        }

        if (archerVisual != null)
        {
            StopAllCoroutines();
            StartCoroutine(RecoilRoutine());
        }
    }

    private IEnumerator RecoilRoutine()
    {
        Vector3 recoilPos = _visualLocalStart - Vector3.forward * recoilDistance;
        archerVisual.localPosition = recoilPos;
        yield return new WaitForSeconds(recoilDuration);
        archerVisual.localPosition = _visualLocalStart;
    }
}
