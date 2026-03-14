using System.Collections;
using UnityEngine;

public class BombTrap : TrapBase
{
    [SerializeField] private float triggerRadius = 1.5f;
    [SerializeField] private bool explodeOnce = true;
    [SerializeField] private float armDuration = 0.25f;
    [SerializeField] private Renderer blinkRenderer;
    [SerializeField] private Color blinkColor = new(1f, 0.35f, 0.2f, 1f);
    [SerializeField] private ExplosionEffect explosionEffect;
    [SerializeField] private LightCameraShake cameraShake;
    [SerializeField] private float shakeDuration = 0.18f;
    [SerializeField] private float shakeMagnitude = 0.1f;

    private bool _hasExploded;
    private bool _isArming;
    private Material _blinkMaterial;
    private Color _baseColor;

    private void Awake()
    {
        if (blinkRenderer != null)
        {
            _blinkMaterial = blinkRenderer.material;
            _baseColor = _blinkMaterial.color;
        }
    }

    private void Update()
    {
        if ((_hasExploded && explodeOnce) || _isArming)
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

            EventLogger.Instance?.Log("Bot entered bomb trap zone");
            _isArming = true;
            StartCoroutine(ArmAndExplode(bot));
            _hasExploded = true;
            break;
        }
    }

    public override void HandleBot(BotHealth botHealth)
    {
        botHealth.TakeDamage(damage * 2f, DamageSource.BombTrap);
    }

    private IEnumerator ArmAndExplode(BotHealth bot)
    {
        EventLogger.Instance?.Log("Trap activated: bomb arming");
        ReplayEventStream.Emit(ReplayEventType.TrapActivated, transform.position, "BombTrap", damage * 2f, "Bomb armed");
        AudioManager.Instance?.PlayTrap(TrapSoundType.BombTrap, TrapSoundEvent.Arm, transform.position, 0.8f);
        AudioManager.Instance?.PlayTrapSound(SoundCue.BombArm);

        if (_blinkMaterial != null)
        {
            _blinkMaterial.color = blinkColor;
        }

        yield return new WaitForSeconds(armDuration);

        if (_blinkMaterial != null)
        {
            _blinkMaterial.color = _baseColor;
        }

        explosionEffect?.PlayAt(transform.position + Vector3.up * 0.2f);
        cameraShake?.Shake(shakeDuration, shakeMagnitude);
        AudioManager.Instance?.PlayTrap(TrapSoundType.BombTrap, TrapSoundEvent.Impact, transform.position, 1f);
        AudioManager.Instance?.PlayTrap(TrapSoundType.BombTrap, TrapSoundEvent.Secondary, transform.position, 0.7f);
        AudioManager.Instance?.PlayTrapSound(SoundCue.BombExplosion);
        HandleBot(bot);
        EventLogger.Instance?.Log($"Bot took {damage * 2f:0} damage");

        _isArming = false;

        if (explodeOnce)
        {
            gameObject.SetActive(false);
        }
    }
}
