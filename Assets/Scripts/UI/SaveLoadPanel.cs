using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveLoadPanel : MonoBehaviour
{
    [SerializeField] private DungeonSaveManager saveManager;
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private TMP_Dropdown savesDropdown;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private GameObject panelRoot;

    private readonly List<DungeonSaveSummary> _cachedSaves = new();

    private void OnEnable()
    {
        RefreshSaveList();
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
        }
    }

    public void SaveClicked()
    {
        string saveName = saveNameInput != null ? saveNameInput.text : string.Empty;
        bool success = saveManager.SaveCurrentLayout(saveName, false, out string message);
        SetStatus(message, success);
        if (success)
        {
            RefreshSaveList();
        }
    }

    public void OverwriteClicked()
    {
        string saveName = saveNameInput != null ? saveNameInput.text : string.Empty;
        bool success = saveManager.SaveCurrentLayout(saveName, true, out string message);
        SetStatus(message, success);
        if (success)
        {
            RefreshSaveList();
        }
    }

    public void LoadClicked()
    {
        string selected = GetSelectedSaveName();
        bool success = saveManager.LoadLayout(selected, out string message);
        SetStatus(message, success);
    }

    public void DeleteClicked()
    {
        string selected = GetSelectedSaveName();
        bool success = saveManager.DeleteLayout(selected, out string message);
        SetStatus(message, success);
        if (success)
        {
            RefreshSaveList();
        }
    }

    public void RefreshSaveList()
    {
        _cachedSaves.Clear();
        _cachedSaves.AddRange(saveManager.ListSaves());

        if (savesDropdown == null)
        {
            return;
        }

        savesDropdown.ClearOptions();
        List<string> options = new();
        foreach (DungeonSaveSummary summary in _cachedSaves)
        {
            string date = summary.createdUnixTime > 0
                ? DateTimeOffset.FromUnixTimeSeconds(summary.createdUnixTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm")
                : "Unknown date";

            string budget = summary.trapBudgetUsed >= 0 ? $"Budget {summary.trapBudgetUsed}" : "Budget --";
            options.Add($"{summary.saveName} | {date} | {budget}");
        }

        if (options.Count == 0)
        {
            options.Add("No saves found");
        }

        savesDropdown.AddOptions(options);
        savesDropdown.value = 0;
    }

    private string GetSelectedSaveName()
    {
        if (_cachedSaves.Count == 0 || savesDropdown == null)
        {
            return string.Empty;
        }

        int index = Mathf.Clamp(savesDropdown.value, 0, _cachedSaves.Count - 1);
        return _cachedSaves[index].saveName;
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
