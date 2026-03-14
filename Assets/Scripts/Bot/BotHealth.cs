using System;
using UnityEngine;

public class BotHealth : MonoBehaviour
{
    [SerializeField] private float maxHp = 100f;

    public event Action<BotHealth> OnBotDied;

    public float CurrentHp { get; private set; }

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
        if (CurrentHp <= 0f)
        {
            CurrentHp = 0f;
            OnBotDied?.Invoke(this);
        }
    }
}
