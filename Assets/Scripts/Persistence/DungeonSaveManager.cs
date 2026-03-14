using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DungeonSaveManager : MonoBehaviour
{
    [SerializeField] private DungeonSerializer dungeonSerializer;
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private string saveFolderName = "DungeonSaves";
    [SerializeField] private string fileExtension = ".json";

    public string SaveDirectoryPath => Path.Combine(Application.persistentDataPath, saveFolderName);

    private void Awake()
    {
        Directory.CreateDirectory(SaveDirectoryPath);
    }

    public bool SaveCurrentLayout(string saveName, bool overwrite, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(saveName))
        {
            message = "Enter a save name first.";
            return false;
        }

        string sanitizedName = SanitizeFileName(saveName.Trim());
        string path = BuildSavePath(sanitizedName);

        if (File.Exists(path) && !overwrite)
        {
            message = "Save exists. Use overwrite to replace it.";
            return false;
        }

        DungeonSaveData saveData = dungeonSerializer.CaptureLayout(sanitizedName);
        string json = dungeonSerializer.ToJson(saveData);
        File.WriteAllText(path, json);
        message = $"Saved dungeon '{sanitizedName}'.";
        return true;
    }

    public bool LoadLayout(string saveName, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(saveName))
        {
            message = "Select a save to load.";
            return false;
        }

        string path = BuildSavePath(saveName);
        if (!File.Exists(path))
        {
            message = "Save file is missing.";
            return false;
        }

        if (simulationManager != null && simulationManager.IsSimulationRunning)
        {
            simulationManager.ResetRun();
        }

        string json = File.ReadAllText(path);
        if (!dungeonSerializer.TryFromJson(json, out DungeonSaveData data))
        {
            message = "Save file is corrupted or incompatible.";
            return false;
        }

        if (!dungeonSerializer.ApplyLayout(data, out string applyError))
        {
            message = $"Failed to load save: {applyError}";
            return false;
        }

        message = $"Loaded dungeon '{saveName}'.";
        return true;
    }

    public bool DeleteLayout(string saveName, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(saveName))
        {
            message = "Select a save to delete.";
            return false;
        }

        string path = BuildSavePath(saveName);
        if (!File.Exists(path))
        {
            message = "Save file was not found.";
            return false;
        }

        File.Delete(path);
        message = $"Deleted '{saveName}'.";
        return true;
    }

    public List<DungeonSaveSummary> ListSaves()
    {
        Directory.CreateDirectory(SaveDirectoryPath);

        return Directory
            .GetFiles(SaveDirectoryPath, $"*{fileExtension}")
            .Select(BuildSummary)
            .Where(summary => summary != null)
            .OrderByDescending(summary => summary.lastWriteUnixTime)
            .ToList();
    }

    public bool TryLoadFromRawJson(string json, out DungeonSaveData data, out string message)
    {
        data = null;
        if (!dungeonSerializer.TryFromJson(json, out data))
        {
            message = "Dungeon data could not be parsed.";
            return false;
        }

        message = string.Empty;
        return true;
    }

    public bool ApplySaveData(DungeonSaveData data, out string message)
    {
        if (simulationManager != null && simulationManager.IsSimulationRunning)
        {
            simulationManager.ResetRun();
        }

        bool success = dungeonSerializer.ApplyLayout(data, out string error);
        message = success ? "Dungeon imported successfully." : error;
        return success;
    }

    private DungeonSaveSummary BuildSummary(string path)
    {
        try
        {
            FileInfo info = new(path);
            string json = File.ReadAllText(path);
            dungeonSerializer.TryFromJson(json, out DungeonSaveData data);

            return new DungeonSaveSummary
            {
                saveName = Path.GetFileNameWithoutExtension(path),
                fullPath = path,
                trapBudgetUsed = data != null ? data.trapBudgetUsed : -1,
                createdUnixTime = data != null ? data.createdUnixTime : 0,
                lastWriteUnixTime = ((DateTimeOffset)info.LastWriteTimeUtc).ToUnixTimeSeconds()
            };
        }
        catch
        {
            return null;
        }
    }

    private string BuildSavePath(string saveName)
    {
        return Path.Combine(SaveDirectoryPath, $"{saveName}{fileExtension}");
    }

    private static string SanitizeFileName(string value)
    {
        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '_');
        }

        return value;
    }
}

[Serializable]
public class DungeonSaveSummary
{
    public string saveName;
    public string fullPath;
    public int trapBudgetUsed;
    public long createdUnixTime;
    public long lastWriteUnixTime;
}
