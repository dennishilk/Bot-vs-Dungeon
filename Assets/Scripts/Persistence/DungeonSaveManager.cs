using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public enum DungeonEntrySource
{
    Local,
    ImportedShareCode,
    DailyChallenge
}

public enum DungeonBrowserSortMode
{
    Name,
    Date,
    Rating
}

public class DungeonSaveManager : MonoBehaviour
{
    [SerializeField] private DungeonSerializer dungeonSerializer;
    [SerializeField] private SimulationManager simulationManager;
    [SerializeField] private string saveFolderName = "DungeonSaves";
    [SerializeField] private string importedFolderName = "ImportedDungeons";
    [SerializeField] private string dailyFolderName = "DailyChallenge";
    [SerializeField] private string fileExtension = ".json";

    public string SaveDirectoryPath => Path.Combine(Application.persistentDataPath, saveFolderName);
    public string ImportedDirectoryPath => Path.Combine(Application.persistentDataPath, importedFolderName);
    public string DailyDirectoryPath => Path.Combine(Application.persistentDataPath, dailyFolderName);

    private void Awake()
    {
        Directory.CreateDirectory(SaveDirectoryPath);
        Directory.CreateDirectory(ImportedDirectoryPath);
        Directory.CreateDirectory(DailyDirectoryPath);
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
        string path = BuildSavePath(sanitizedName, DungeonEntrySource.Local);

        if (File.Exists(path) && !overwrite)
        {
            message = "Save exists. Use overwrite to replace it.";
            return false;
        }

        DungeonSaveData saveData = dungeonSerializer.CaptureLayout(sanitizedName);
        ApplyMetadataDefaults(saveData, sanitizedName);
        string json = dungeonSerializer.ToJson(saveData);
        File.WriteAllText(path, json);
        message = $"Saved dungeon '{sanitizedName}'.";
        return true;
    }

    public bool SaveImportedLayout(string saveName, DungeonSaveData data, bool overwrite, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(saveName) || data == null)
        {
            message = "Imported dungeon is invalid.";
            return false;
        }

        string sanitizedName = SanitizeFileName(saveName.Trim());
        string path = BuildSavePath(sanitizedName, DungeonEntrySource.ImportedShareCode);
        if (File.Exists(path) && !overwrite)
        {
            message = "Imported save already exists.";
            return false;
        }

        data.saveName = sanitizedName;
        ApplyMetadataDefaults(data, sanitizedName);
        File.WriteAllText(path, dungeonSerializer.ToJson(data));
        message = $"Imported dungeon '{sanitizedName}' saved.";
        return true;
    }

    public bool SaveDailyChallengeLayout(string fileName, DungeonSaveData data, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(fileName) || data == null)
        {
            message = "Daily challenge data is invalid.";
            return false;
        }

        string path = BuildSavePath(fileName, DungeonEntrySource.DailyChallenge);
        ApplyMetadataDefaults(data, data.saveName);
        File.WriteAllText(path, dungeonSerializer.ToJson(data));
        message = "Daily challenge saved.";
        return true;
    }

    public bool LoadLayout(string saveName, out string message)
    {
        if (TryFindSavePath(saveName, out string path, out DungeonEntrySource source))
        {
            return LoadLayoutByPath(path, source, out message);
        }

        message = "Save file is missing.";
        return false;
    }

    public bool LoadLayoutByPath(string path, DungeonEntrySource source, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
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

        data.metadata.timesTested++;
        File.WriteAllText(path, dungeonSerializer.ToJson(data));

        message = $"Loaded dungeon '{Path.GetFileNameWithoutExtension(path)}' ({source}).";
        return true;
    }

    public bool DeleteLayout(string saveName, out string message)
    {
        return DeleteLayoutByPath(BuildSavePath(saveName, DungeonEntrySource.Local), out message);
    }

    public bool DeleteLayoutByPath(string path, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            message = "Save file was not found.";
            return false;
        }

        File.Delete(path);
        message = $"Deleted '{Path.GetFileNameWithoutExtension(path)}'.";
        return true;
    }

    public List<DungeonSaveSummary> ListSaves(DungeonBrowserSortMode sortMode = DungeonBrowserSortMode.Date)
    {
        Directory.CreateDirectory(SaveDirectoryPath);
        Directory.CreateDirectory(ImportedDirectoryPath);
        Directory.CreateDirectory(DailyDirectoryPath);

        IEnumerable<DungeonSaveSummary> all =
            GetFilesForSource(SaveDirectoryPath, DungeonEntrySource.Local)
            .Concat(GetFilesForSource(ImportedDirectoryPath, DungeonEntrySource.ImportedShareCode))
            .Concat(GetFilesForSource(DailyDirectoryPath, DungeonEntrySource.DailyChallenge))
            .Where(summary => summary != null);

        return sortMode switch
        {
            DungeonBrowserSortMode.Name => all.OrderBy(s => s.dungeonName).ThenByDescending(s => s.createdUnixTime).ToList(),
            DungeonBrowserSortMode.Rating => all.OrderByDescending(s => ParseRatingWeight(s.lastCertificationRating)).ThenBy(s => s.dungeonName).ToList(),
            _ => all.OrderByDescending(s => s.createdUnixTime > 0 ? s.createdUnixTime : s.lastWriteUnixTime).ToList()
        };
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

    public bool UpdateRating(string saveName, string rating)
    {
        if (!TryFindSavePath(saveName, out string path, out _))
        {
            return false;
        }

        string json = File.ReadAllText(path);
        if (!dungeonSerializer.TryFromJson(json, out DungeonSaveData data) || data == null)
        {
            return false;
        }

        ApplyMetadataDefaults(data, saveName);
        data.metadata.lastCertificationRating = rating;
        File.WriteAllText(path, dungeonSerializer.ToJson(data));
        return true;
    }

    private IEnumerable<DungeonSaveSummary> GetFilesForSource(string folder, DungeonEntrySource source)
    {
        foreach (string path in Directory.GetFiles(folder, $"*{fileExtension}"))
        {
            DungeonSaveSummary summary = BuildSummary(path, source);
            if (summary != null)
            {
                yield return summary;
            }
        }
    }

    private DungeonSaveSummary BuildSummary(string path, DungeonEntrySource source)
    {
        try
        {
            FileInfo info = new(path);
            string json = File.ReadAllText(path);
            dungeonSerializer.TryFromJson(json, out DungeonSaveData data);
            string fallbackName = Path.GetFileNameWithoutExtension(path);
            ApplyMetadataDefaults(data, fallbackName);

            return new DungeonSaveSummary
            {
                saveName = fallbackName,
                dungeonName = data != null && !string.IsNullOrWhiteSpace(data.metadata.dungeonName) ? data.metadata.dungeonName : fallbackName,
                fullPath = path,
                source = source,
                trapBudgetUsed = data != null ? data.trapBudgetUsed : -1,
                createdUnixTime = data != null && data.metadata.creationDate > 0 ? data.metadata.creationDate : (data != null ? data.createdUnixTime : 0),
                lastWriteUnixTime = ((DateTimeOffset)info.LastWriteTimeUtc).ToUnixTimeSeconds(),
                lastCertificationRating = data != null ? data.metadata.lastCertificationRating : string.Empty,
                timesTested = data != null ? data.metadata.timesTested : 0
            };
        }
        catch
        {
            return null;
        }
    }

    private bool TryFindSavePath(string saveName, out string path, out DungeonEntrySource source)
    {
        source = DungeonEntrySource.Local;
        path = BuildSavePath(saveName, DungeonEntrySource.Local);
        if (File.Exists(path))
        {
            return true;
        }

        path = BuildSavePath(saveName, DungeonEntrySource.ImportedShareCode);
        if (File.Exists(path))
        {
            source = DungeonEntrySource.ImportedShareCode;
            return true;
        }

        path = BuildSavePath(saveName, DungeonEntrySource.DailyChallenge);
        if (File.Exists(path))
        {
            source = DungeonEntrySource.DailyChallenge;
            return true;
        }

        return false;
    }

    private string BuildSavePath(string saveName, DungeonEntrySource source)
    {
        string folder = source switch
        {
            DungeonEntrySource.ImportedShareCode => ImportedDirectoryPath,
            DungeonEntrySource.DailyChallenge => DailyDirectoryPath,
            _ => SaveDirectoryPath
        };

        return Path.Combine(folder, $"{saveName}{fileExtension}");
    }

    private static int ParseRatingWeight(string rating)
    {
        if (string.IsNullOrWhiteSpace(rating)) return 0;
        if (rating.Equals("Safe", StringComparison.OrdinalIgnoreCase)) return 1;
        if (rating.Equals("Fair", StringComparison.OrdinalIgnoreCase)) return 2;
        if (rating.Equals("Dangerous", StringComparison.OrdinalIgnoreCase)) return 3;
        if (rating.Equals("Deadly", StringComparison.OrdinalIgnoreCase)) return 4;
        return 0;
    }

    private static void ApplyMetadataDefaults(DungeonSaveData data, string fallbackName)
    {
        if (data == null)
        {
            return;
        }

        data.metadata ??= new DungeonMetadata();
        if (string.IsNullOrWhiteSpace(data.metadata.dungeonName))
        {
            data.metadata.dungeonName = !string.IsNullOrWhiteSpace(data.saveName) ? data.saveName : fallbackName;
        }

        if (data.metadata.creationDate <= 0)
        {
            data.metadata.creationDate = data.createdUnixTime > 0 ? data.createdUnixTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        if (data.metadata.trapBudget <= 0)
        {
            data.metadata.trapBudget = data.trapBudgetUsed;
        }
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
    public string dungeonName;
    public string fullPath;
    public DungeonEntrySource source;
    public int trapBudgetUsed;
    public long createdUnixTime;
    public long lastWriteUnixTime;
    public string lastCertificationRating;
    public int timesTested;
}
