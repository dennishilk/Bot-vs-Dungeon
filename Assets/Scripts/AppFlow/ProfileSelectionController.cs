using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BotVsDungeon.AppFlow
{
    public class ProfileSelectionController : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown profilesDropdown;
        [SerializeField] private TMP_InputField createInput;
        [SerializeField] private TMP_InputField renameInput;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private GameObject panelRoot;

        private readonly List<PlayerProfileSummary> _cached = new();

        private void OnEnable()
        {
            Refresh();
        }

        public void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }
        }

        public void Refresh()
        {
            _cached.Clear();
            if (PlayerProfileManager.Instance != null)
            {
                foreach (PlayerProfileSummary profile in PlayerProfileManager.Instance.Profiles)
                {
                    _cached.Add(profile);
                }
            }

            if (profilesDropdown == null)
            {
                return;
            }

            profilesDropdown.ClearOptions();
            List<string> options = new();
            foreach (PlayerProfileSummary profile in _cached)
            {
                string stamp = DateTimeOffset.FromUnixTimeSeconds(profile.lastPlayedUnixTime).ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                options.Add($"{profile.displayName} ({stamp})");
            }

            if (options.Count == 0)
            {
                options.Add("No profiles found");
            }

            profilesDropdown.AddOptions(options);
            profilesDropdown.value = 0;
        }

        public void CreateProfileClicked()
        {
            if (PlayerProfileManager.Instance == null)
            {
                SetStatus("Profile system is unavailable.", false);
                return;
            }

            bool success = PlayerProfileManager.Instance.CreateProfile(createInput != null ? createInput.text : string.Empty, out string message);
            SetStatus(message, success);
            if (success)
            {
                Refresh();
            }
        }

        public void SelectProfileClicked()
        {
            PlayerProfileSummary selected = GetSelected();
            if (selected == null)
            {
                SetStatus("Select a valid profile.", false);
                return;
            }

            bool success = PlayerProfileManager.Instance != null && PlayerProfileManager.Instance.SelectProfile(selected.profileId);
            SetStatus(success ? $"Selected {selected.displayName}." : "Could not select profile.", success);
            if (success)
            {
                AppStateManager.Instance?.ReturnToMenu();
            }
        }

        public void RenameProfileClicked()
        {
            PlayerProfileSummary selected = GetSelected();
            if (selected == null || PlayerProfileManager.Instance == null)
            {
                SetStatus("Select a valid profile.", false);
                return;
            }

            bool success = PlayerProfileManager.Instance.RenameProfile(selected.profileId, renameInput != null ? renameInput.text : string.Empty, out string message);
            SetStatus(message, success);
            if (success)
            {
                Refresh();
            }
        }

        public void DeleteProfileClicked()
        {
            PlayerProfileSummary selected = GetSelected();
            if (selected == null || PlayerProfileManager.Instance == null)
            {
                SetStatus("Select a valid profile.", false);
                return;
            }

            bool success = PlayerProfileManager.Instance.DeleteProfile(selected.profileId, out string message);
            SetStatus(message, success);
            if (success)
            {
                Refresh();
            }
        }

        private PlayerProfileSummary GetSelected()
        {
            if (_cached.Count == 0 || profilesDropdown == null)
            {
                return null;
            }

            int index = Mathf.Clamp(profilesDropdown.value, 0, _cached.Count - 1);
            return _cached[index];
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
