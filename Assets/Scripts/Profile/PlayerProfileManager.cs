using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BotVsDungeon.AppFlow
{
    [Serializable]
    public class PlayerProfileSummary
    {
        public string profileId;
        public string displayName;
        public long createdUnixTime;
        public long lastPlayedUnixTime;
    }

    [Serializable]
    public class PlayerProfileCollection
    {
        public List<PlayerProfileSummary> profiles = new();
        public string activeProfileId;
    }

    public class PlayerProfileManager : MonoBehaviour
    {
        public static PlayerProfileManager Instance { get; private set; }

        [SerializeField] private string profileFileName = "profiles.json";
        [SerializeField] private string defaultProfileName = "Warden";

        public PlayerProfileSummary ActiveProfile { get; private set; }
        public IReadOnlyList<PlayerProfileSummary> Profiles => _profiles.profiles;

        private PlayerProfileCollection _profiles = new();

        private string ProfilesPath => Path.Combine(Application.persistentDataPath, profileFileName);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProfiles();

            if (_profiles.profiles.Count == 0)
            {
                CreateProfile(defaultProfileName, out _);
            }

            SelectProfile(string.IsNullOrWhiteSpace(_profiles.activeProfileId) ? _profiles.profiles[0].profileId : _profiles.activeProfileId);
        }

        public bool CreateProfile(string displayName, out string message)
        {
            message = string.Empty;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                message = "Enter a profile name.";
                return false;
            }

            string trimmed = displayName.Trim();
            foreach (PlayerProfileSummary profile in _profiles.profiles)
            {
                if (profile.displayName.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
                {
                    message = "A profile with this name already exists.";
                    return false;
                }
            }

            PlayerProfileSummary created = new()
            {
                profileId = Guid.NewGuid().ToString("N"),
                displayName = trimmed,
                createdUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                lastPlayedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _profiles.profiles.Add(created);
            _profiles.activeProfileId = created.profileId;
            ActiveProfile = created;
            SaveProfiles();
            message = $"Profile '{trimmed}' created.";
            return true;
        }

        public bool RenameProfile(string profileId, string newName, out string message)
        {
            message = string.Empty;
            PlayerProfileSummary profile = Find(profileId);
            if (profile == null)
            {
                message = "Profile not found.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(newName))
            {
                message = "Enter a profile name.";
                return false;
            }

            profile.displayName = newName.Trim();
            profile.lastPlayedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveProfiles();
            message = "Profile renamed.";
            return true;
        }

        public bool DeleteProfile(string profileId, out string message)
        {
            message = string.Empty;
            if (_profiles.profiles.Count <= 1)
            {
                message = "At least one profile must remain.";
                return false;
            }

            int removed = _profiles.profiles.RemoveAll(p => p.profileId == profileId);
            if (removed == 0)
            {
                message = "Profile not found.";
                return false;
            }

            if (_profiles.activeProfileId == profileId)
            {
                _profiles.activeProfileId = _profiles.profiles[0].profileId;
                ActiveProfile = _profiles.profiles[0];
            }

            SaveProfiles();
            message = "Profile deleted.";
            return true;
        }

        public bool SelectProfile(string profileId)
        {
            PlayerProfileSummary profile = Find(profileId);
            if (profile == null)
            {
                return false;
            }

            ActiveProfile = profile;
            ActiveProfile.lastPlayedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            _profiles.activeProfileId = profile.profileId;
            SaveProfiles();
            return true;
        }

        public void SaveActiveProfile()
        {
            if (ActiveProfile == null)
            {
                return;
            }

            ActiveProfile.lastPlayedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SaveProfiles();
        }

        private PlayerProfileSummary Find(string profileId)
        {
            foreach (PlayerProfileSummary profile in _profiles.profiles)
            {
                if (profile.profileId == profileId)
                {
                    return profile;
                }
            }

            return null;
        }

        private void LoadProfiles()
        {
            if (!File.Exists(ProfilesPath))
            {
                _profiles = new PlayerProfileCollection();
                return;
            }

            string json = File.ReadAllText(ProfilesPath);
            _profiles = JsonUtility.FromJson<PlayerProfileCollection>(json) ?? new PlayerProfileCollection();
            _profiles.profiles ??= new List<PlayerProfileSummary>();
        }

        private void SaveProfiles()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            File.WriteAllText(ProfilesPath, JsonUtility.ToJson(_profiles, true));
        }
    }
}
