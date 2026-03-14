using UnityEngine;

public abstract class TrapBase : MonoBehaviour
{
    [SerializeField] protected float damage = 10f;

    public virtual float PathCostPenalty => 5f;

    public abstract void HandleBot(BotHealth botHealth);
}
