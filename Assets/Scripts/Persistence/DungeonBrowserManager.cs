using System;
using System.Collections.Generic;
using UnityEngine;

public class DungeonBrowserManager : MonoBehaviour
{
    [SerializeField] private DungeonSaveManager saveManager;
    [SerializeField] private SimulationManager simulationManager;

    public DungeonBrowserSortMode ActiveSort { get; private set; } = DungeonBrowserSortMode.Date;
    public IReadOnlyList<DungeonSaveSummary> CachedEntries => _cachedEntries;

    private readonly List<DungeonSaveSummary> _cachedEntries = new();

    public event Action<IReadOnlyList<DungeonSaveSummary>> OnEntriesUpdated;

    public void RefreshEntries()
    {
        _cachedEntries.Clear();
        if (saveManager != null)
        {
            _cachedEntries.AddRange(saveManager.ListSaves(ActiveSort));
        }

        OnEntriesUpdated?.Invoke(_cachedEntries);
    }

    public void SetSortMode(DungeonBrowserSortMode sortMode)
    {
        ActiveSort = sortMode;
        RefreshEntries();
    }

    public bool LoadEntry(DungeonSaveSummary entry, out string message)
    {
        if (entry == null || saveManager == null)
        {
            message = "No dungeon selected.";
            return false;
        }

        return saveManager.LoadLayoutByPath(entry.fullPath, entry.source, out message);
    }

    public bool TestEntry(DungeonSaveSummary entry, out string message)
    {
        if (!LoadEntry(entry, out message))
        {
            return false;
        }

        if (simulationManager == null)
        {
            message = "Loaded, but SimulationManager is missing.";
            return false;
        }

        simulationManager.StartSimulation();
        message = $"Loaded and started test for '{entry.dungeonName}'.";
        return true;
    }

    public bool DeleteEntry(DungeonSaveSummary entry, out string message)
    {
        if (entry == null || saveManager == null)
        {
            message = "No dungeon selected.";
            return false;
        }

        if (entry.source != DungeonEntrySource.Local)
        {
            message = "Only local dungeons can be deleted from the browser.";
            return false;
        }

        bool success = saveManager.DeleteLayoutByPath(entry.fullPath, out message);
        if (success)
        {
            RefreshEntries();
        }

        return success;
    }
}
