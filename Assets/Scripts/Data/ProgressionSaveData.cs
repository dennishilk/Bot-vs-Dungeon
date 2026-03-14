using System;
using System.Collections.Generic;

[Serializable]
public class ProgressionSaveData
{
    public List<int> unlockedLevels = new();
    public List<int> completedLevels = new();
    public List<AchievementData> achievements = new();
}
