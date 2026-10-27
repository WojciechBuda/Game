using System;
using JellyDrop.Core.Economy;
using UnityEngine;

namespace JellyDrop.Services
{
    /// <summary>
    /// Saves player progress as JSON in PlayerPrefs, with a backup copy.
    /// Swap in Unity Cloud Save / Firebase later by implementing the same two methods.
    /// </summary>
    public sealed class SaveService
    {
        private const string Key = "jellydrop.save.v1";
        private const string BackupKey = "jellydrop.save.v1.bak";

        private bool _dirty;
        private float _lastSaveTime;

        public PlayerData Load()
        {
            var data = TryParse(PlayerPrefs.GetString(Key, string.Empty))
                       ?? TryParse(PlayerPrefs.GetString(BackupKey, string.Empty));
            return data ?? new PlayerData();
        }

        public void MarkDirty() => _dirty = true;

        /// <summary>Saves at most every couple of seconds while playing, immediately when forced.</summary>
        public void SaveIfDirty(PlayerData data, bool force = false)
        {
            if (!_dirty && !force) return;
            if (!force && Time.realtimeSinceStartup - _lastSaveTime < 2f) return;
            Save(data);
        }

        public void Save(PlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                string previous = PlayerPrefs.GetString(Key, string.Empty);
                if (!string.IsNullOrEmpty(previous)) PlayerPrefs.SetString(BackupKey, previous);
                PlayerPrefs.SetString(Key, json);
                PlayerPrefs.Save();
                _dirty = false;
                _lastSaveTime = Time.realtimeSinceStartup;
            }
            catch (Exception e)
            {
                Debug.LogError("Save failed: " + e);
            }
        }

        /// <summary>Wipes progress (used by the debug menu).</summary>
        public void DeleteAll()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.DeleteKey(BackupKey);
            PlayerPrefs.Save();
        }

        private static PlayerData TryParse(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonUtility.FromJson<PlayerData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Corrupt save, ignoring: " + e.Message);
                return null;
            }
        }
    }
}
