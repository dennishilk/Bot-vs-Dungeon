using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    public class SaveLoadMenuController : MonoBehaviour
    {
        [SerializeField] private DungeonSaveManager dungeonSaveManager;
        [SerializeField] private ShareCodeManager shareCodeManager;
        [SerializeField] private TMP_InputField saveNameInput;
        [SerializeField] private TMP_Dropdown saveDropdown;
        [SerializeField] private TMP_InputField shareCodeInput;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private ErrorPopupController errorPopupController;
        [SerializeField] private GameObject panelRoot;

        private readonly List<DungeonSaveSummary> _cached = new();

        private void OnEnable()
        {
            RefreshList();
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }

            if (visible)
            {
                RefreshList();
            }
        }

        public void SaveClicked() => SaveInternal(false);
        public void OverwriteClicked() => SaveInternal(true);

        public void LoadClicked()
        {
            DungeonSaveSummary summary = Selected();
            if (summary == null)
            {
                SetStatus("Select a save first.", false);
                return;
            }

            bool ok = dungeonSaveManager != null && dungeonSaveManager.LoadLayoutByPath(summary.fullPath, summary.source, out string message);
            if (!ok)
            {
                errorPopupController?.ShowError("Save could not be loaded", message);
            }

            SetStatus(message, ok);
        }

        public void DeleteClicked()
        {
            DungeonSaveSummary summary = Selected();
            if (summary == null)
            {
                SetStatus("Select a save first.", false);
                return;
            }

            bool ok = dungeonSaveManager != null && dungeonSaveManager.DeleteLayoutByPath(summary.fullPath, out string message);
            SetStatus(message, ok);
            if (ok)
            {
                RefreshList();
            }
        }

        public void ExportShareCodeClicked()
        {
            DungeonSaveSummary summary = Selected();
            if (summary == null)
            {
                SetStatus("Select a save first.", false);
                return;
            }

            if (shareCodeManager == null)
            {
                SetStatus("Share code manager missing.", false);
                return;
            }

            bool loaded = dungeonSaveManager.LoadLayoutByPath(summary.fullPath, summary.source, out string loadMessage);
            if (!loaded)
            {
                errorPopupController?.ShowError("Could not prepare save", loadMessage);
                SetStatus(loadMessage, false);
                return;
            }

            string code = shareCodeManager.ExportCurrentDungeonCode(summary.dungeonName);
            if (shareCodeInput != null)
            {
                shareCodeInput.text = code;
            }

            SetStatus("Share code generated.", true);
        }

        public void ImportShareCodeClicked()
        {
            if (shareCodeManager == null)
            {
                SetStatus("Share code manager missing.", false);
                return;
            }

            string code = shareCodeInput != null ? shareCodeInput.text : string.Empty;
            bool ok = shareCodeManager.TryImportCode(code, out DungeonSaveData data, out string message);
            if (ok)
            {
                ok = shareCodeManager.ApplyImportedDungeon(data, out message);
            }

            if (!ok)
            {
                errorPopupController?.ShowError("Share code failed", message);
            }

            SetStatus(message, ok);
            if (ok)
            {
                RefreshList();
            }
        }

        public void RefreshList()
        {
            _cached.Clear();
            if (dungeonSaveManager != null)
            {
                _cached.AddRange(dungeonSaveManager.ListSaves());
            }

            if (saveDropdown == null)
            {
                return;
            }

            saveDropdown.ClearOptions();
            List<string> options = new();
            foreach (DungeonSaveSummary summary in _cached)
            {
                options.Add($"{summary.dungeonName} [{summary.source}] {summary.lastCertificationRating}");
            }

            if (options.Count == 0)
            {
                options.Add("No saves available");
            }

            saveDropdown.AddOptions(options);
            saveDropdown.value = 0;
        }

        private void SaveInternal(bool overwrite)
        {
            string name = saveNameInput != null ? saveNameInput.text : string.Empty;
            bool ok = dungeonSaveManager != null && dungeonSaveManager.SaveCurrentLayout(name, overwrite, out string message);
            if (!ok)
            {
                errorPopupController?.ShowError("Save failed", message);
            }

            SetStatus(message, ok);
            if (ok)
            {
                RefreshList();
            }
        }

        private DungeonSaveSummary Selected()
        {
            if (saveDropdown == null || _cached.Count == 0)
            {
                return null;
            }

            return _cached[Mathf.Clamp(saveDropdown.value, 0, _cached.Count - 1)];
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
}
