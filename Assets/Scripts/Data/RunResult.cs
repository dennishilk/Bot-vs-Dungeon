using UnityEngine;

[System.Serializable]
public class RunResult
{
    public BotPersonality personality;
    public bool survived;
    public string causeOfDeath;
    public float completionTime;
    public float remainingHP;
    public Vector2 deathPosition;
    public int pathLength;

    public string ToSummaryLine()
    {
        string outcome = survived ? "Survived" : $"Died ({causeOfDeath})";
        return $"{personality}: {outcome} | Time {completionTime:0.00}s | HP {remainingHP:0}";
    }
}
