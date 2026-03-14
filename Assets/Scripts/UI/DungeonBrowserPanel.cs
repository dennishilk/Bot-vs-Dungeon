using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DungeonBrowserPanel : MonoBehaviour
{
    [SerializeField] private DungeonBrowserManager browserManager;
    [SerializeField] private TMP_Dropdown sortDropdown;
    [SerializeField] private TMP_Dropdown dungeonDropdown;
    [SerializeField] private TMP_Text detailsText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject panelRoot;

    private readonly List<DungeonSaveSummary> _entries = new();

    private void OnEnable()
    {
        if (browserManager != null)
        {
            browserManager.OnEntriesUpdated += HandleEntriesUpdated;
            browserManager.RefreshEntries();
        }

        if (sortDropdown != null)
        {
            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string> { "Sort: Name", "Sort: Date", "Sort: Rating" });
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
            sortDropdown.value = 1;
        }

        if (dungeonDropdown != null)
        {
            dungeonDropdown.onValueChanged.AddListener(_ => RefreshDetails());
        }
    }

    private void OnDisable()
    {
        if (browserManager != null)
        {
            browserManager.OnEntriesUpdated -= HandleEntriesUpdated;
        }

        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.RemoveListener(OnSortChanged);
        }
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void LoadClicked()
    {
        DungeonSaveSummary selected = GetSelected();
        bool ok = browserManager != null && browserManager.LoadEntry(selected, out string message);
        SetStatus(message, ok);
    }

    public void TestClicked()
    {
        DungeonSaveSummary selected = GetSelected();
        bool ok = browserManager != null && browserManager.TestEntry(selected, out string message);
        SetStatus(message, ok);
    }

    public void DeleteClicked()
    {
        DungeonSaveSummary selected = GetSelected();
        bool ok = browserManager != null && browserManager.DeleteEntry(selected, out string message);
        SetStatus(message, ok);
    }

    private void OnSortChanged(int value)
    {
        DungeonBrowserSortMode mode = value switch
        {
            0 => DungeonBrowserSortMode.Name,
            2 => DungeonBrowserSortMode.Rating,
            _ => DungeonBrowserSortMode.Date
        };

        browserManager?.SetSortMode(mode);
    }

    private void HandleEntriesUpdated(IReadOnlyList<DungeonSaveSummary> entries)
    {
        _entries.Clear();
        _entries.AddRange(entries);

        if (dungeonDropdown == null)
        {
            return;
        }

        dungeonDropdown.ClearOptions();
        List<string> options = new();
        foreach (DungeonSaveSummary entry in _entries)
        {
            options.Add($"{entry.dungeonName} [{entry.source}]");
        }

        if (options.Count == 0)
        {
            options.Add("No dungeons found");
        }

        dungeonDropdown.AddOptions(options);
        dungeonDropdown.value = 0;
        RefreshDetails();
    }

    private void RefreshDetails()
    {
        if (detailsText == null)
        {
            return;
        }

        DungeonSaveSummary selected = GetSelected();
        if (selected == null)
        {
            detailsText.text = "No dungeon selected.";
            return;
        }

        string date = selected.createdUnixTime > 0
            ? DateTimeOffset.FromUnixTimeSeconds(selected.createdUnixTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm")
            : "Unknown";
        string rating = string.IsNullOrWhiteSpace(selected.lastCertificationRating) ? "--" : selected.lastCertificationRating;
        detailsText.text = $"Name: {selected.dungeonName}\nBudget: {selected.trapBudgetUsed}\nRating: {rating}\nDate: {date}\nTimes Tested: {selected.timesTested}";
    }

    private DungeonSaveSummary GetSelected()
    {
        if (_entries.Count == 0 || dungeonDropdown == null)
        {
            return null;
        }

        int index = Mathf.Clamp(dungeonDropdown.value, 0, _entries.Count - 1);
        return _entries[index];
    }

    private void SetStatus(string message, bool success)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = success ? new Color(0.5f, 0.95f, 0.6f) : new Color(0.95f, 0.45f, 0.45f);
        }
    }
}
