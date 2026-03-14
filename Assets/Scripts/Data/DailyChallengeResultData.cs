using System;

[Serializable]
public class DailyChallengeResultData
{
    public string challengeId;
    public bool completed;
    public bool bestSurvived;
    public float bestCompletionTime;
    public int attempts;
}
