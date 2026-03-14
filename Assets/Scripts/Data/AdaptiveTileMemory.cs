using UnityEngine;

[System.Serializable]
public class AdaptiveTileMemory
{
    public SerializableVector2Int gridPosition;
    public int deathCount;
    public int damageCount;
    public float totalDamageTaken;
    public int successfulPassCount;
    public float learnedDangerModifier;
    public int avoidedCount;

    public AdaptiveTileMemory(Vector2Int position)
    {
        gridPosition = SerializableVector2Int.From(position);
    }

    public Vector2Int Position => gridPosition.ToVector2Int();
    public float AverageDamage => damageCount > 0 ? totalDamageTaken / damageCount : 0f;
}
