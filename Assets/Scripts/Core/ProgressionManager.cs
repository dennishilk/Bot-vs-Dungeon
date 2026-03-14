using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    [SerializeField] private string saveFileName = "progression_save.json";
    [SerializeField] private bool verboseLogging;

    public event Action OnProgressionChanged;

    public ProgressionSaveData SaveData { get; private set; } = new();
    public string SaveFilePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private int _totalLevels;

    private void Awake()
    {
        Load();
    }

    public void Initialize(int totalLevels)
    {
        _totalLevels = Mathf.Max(1, totalLevels);

        if (SaveData.unlockedLevels.Count == 0)
        {
            SaveData.unlockedLevels.Add(0);
        }

        SaveData.unlockedLevels = SaveData.unlockedLevels.Distinct().OrderBy(i => i).ToList();
        SaveData.completedLevels = SaveData.completedLevels.Distinct().OrderBy(i => i).ToList();
        Save();
    }

    public bool IsLevelUnlocked(int index)
    {
        return SaveData.unlockedLevels.Contains(index);
    }

    public bool IsLevelCompleted(int index)
    {
        return SaveData.completedLevels.Contains(index);
    }

    public bool MarkLevelCompleted(int index)
    {
        bool changed = false;
        if (!SaveData.completedLevels.Contains(index))
        {
            SaveData.completedLevels.Add(index);
            changed = true;
        }

        int next = index + 1;
        if (next < _totalLevels && !SaveData.unlockedLevels.Contains(next))
        {
            SaveData.unlockedLevels.Add(next);
            changed = true;
        }

        if (changed)
        {
            SaveData.completedLevels.Sort();
            SaveData.unlockedLevels.Sort();
            Save();
            OnProgressionChanged?.Invoke();
        }

        return changed;
    }

    public bool IsAchievementUnlocked(string id)
    {
        return SaveData.achievements.Any(a => a.achievementID == id && a.unlocked);
    }

    public bool UnlockAchievement(AchievementData achievementData)
    {
        if (achievementData == null || string.IsNullOrWhiteSpace(achievementData.achievementID))
        {
            return false;
        }

        AchievementData existing = SaveData.achievements.FirstOrDefault(a => a.achievementID == achievementData.achievementID);
        if (existing != null && existing.unlocked)
        {
            return false;
        }

        string now = DateTime.UtcNow.ToString("u");

        if (existing != null)
        {
            existing.title = achievementData.title;
            existing.description = achievementData.description;
            existing.unlocked = true;
            existing.unlockDate = now;
        }
        else
        {
            achievementData.unlocked = true;
            achievementData.unlockDate = now;
            SaveData.achievements.Add(achievementData);
        }

        Save();
        OnProgressionChanged?.Invoke();
        return true;
    }

    [ContextMenu("Reset Progression Save")]
    public void ResetSave()
    {
        SaveData = new ProgressionSaveData();
        SaveData.unlockedLevels.Add(0);
        Save();
        OnProgressionChanged?.Invoke();
    }

    private void Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            SaveData = new ProgressionSaveData();
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData = JsonUtility.FromJson<ProgressionSaveData>(json) ?? new ProgressionSaveData();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to load progression data. Starting fresh. {ex.Message}");
            SaveData = new ProgressionSaveData();
        }
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(SaveData, true);
            File.WriteAllText(SaveFilePath, json);
            if (verboseLogging)
            {
                Debug.Log($"Progression saved to {SaveFilePath}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to save progression data: {ex.Message}");
        }
    }
}
