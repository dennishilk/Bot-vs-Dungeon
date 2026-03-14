using System;
using System.Collections.Generic;

[Serializable]
public class PlayerCareerData
{
    public string currentRank = "Junior Architect";
    public int bureauScore;
    public int totalCertifications;
    public int totalBotFatalities;
    public int totalSurvivals;
    public float averageSurvivalRate;
    public string bestDungeonRating = "Unsafe";

    public List<string> completedAssignments = new();
    public List<string> unlockedTiers = new();
    public List<string> unlockedFeatures = new();
}
