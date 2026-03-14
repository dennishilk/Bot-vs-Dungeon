using System;
using UnityEngine;

[Serializable]
public class StressTestResultData
{
    public int botsSpawned;
    public int botsSurvived;
    public int botsDied;
    public float averageSurvivalTime;
    public float averagePathLength;
    public Vector2Int mostLethalTile;
    public string mostCommonCauseOfDeath;
    public bool adaptiveModeUsed;
    public string adaptiveLearningPool;
    public AdaptiveLearningSummaryData learningSummary;
}
