using System;
using System.Collections;
using UnityEngine;

public class BotHealth : MonoBehaviour
{
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float deathSinkDistance = 0.5f;
    [SerializeField] private float deathSinkDuration = 0.45f;
    [SerializeField] private HitFlashEffect hitFlashEffect;

    public event Action<BotHealth> OnBotDied;
    public event Action<float, float> OnBotDamaged;

    public float CurrentHp { get; private set; }

    private bool _isDying;

    private void Awake()
    {
        CurrentHp = maxHp;
    }

    public void TakeDamage(float amount)
    {
        if (CurrentHp <= 0f)
        {
            return;
        }

        CurrentHp -= amount;
        CurrentHp = Mathf.Max(0f, CurrentHp);

        hitFlashEffect?.PlayHitFlash(transform.position + Vector3.up * 0.4f);
        OnBotDamaged?.Invoke(amount, CurrentHp);
        EventLogger.Instance?.Log($"Bot took {amount:0} damage (HP: {CurrentHp:0})");
        AudioManager.Instance?.PlayTrapSound(SoundCue.BotHurt);

        if (CurrentHp <= 0f)
        {
            EventLogger.Instance?.Log("Bot died");
            AudioManager.Instance?.PlayResultSound(SoundCue.BotDeath);
            OnBotDied?.Invoke(this);
            if (!_isDying)
            {
                StartCoroutine(DeathRoutine());
            }
        }
    }

    private IEnumerator DeathRoutine()
    {
        _isDying = true;
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * deathSinkDistance;

        float elapsed = 0f;
        while (elapsed < deathSinkDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, elapsed / deathSinkDuration);
            yield return null;
        }
    }
}
