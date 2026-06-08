using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BreathTower.Data
{
    /// <summary>
    /// Offline persistence layer. Replaces the web app's AsyncStorage
    /// (src/store/storage.ts) with a JSON file in Application.persistentDataPath.
    /// All app state lives behind this single in-memory <see cref="AppData"/>
    /// instance, which is loaded once and saved on mutation.
    /// </summary>
    public static class SaveSystem
    {
        private const string FileName = "breathTowerData_v1.json";

        private static AppData _cache;
        private static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        /// <summary>The current in-memory app data (loads from disk on first access).</summary>
        public static AppData Data
        {
            get
            {
                if (_cache == null) Load();
                return _cache;
            }
        }

        public static void Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var raw = File.ReadAllText(FilePath);
                    _cache = JsonUtility.FromJson<AppData>(raw) ?? new AppData();
                    // Guard against partially-written / legacy payloads.
                    _cache.sessions ??= new List<SessionRecord>();
                    _cache.settings ??= new AppSettings();
                    _cache.settings.customRhythms ??= new List<CustomRhythm>();
                }
                else
                {
                    _cache = new AppData();
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BreathTower] Failed to load save data: {e.Message}");
                _cache = new AppData();
            }
        }

        public static void Save()
        {
            try
            {
                var raw = JsonUtility.ToJson(Data);
                File.WriteAllText(FilePath, raw);
            }
            catch (Exception e)
            {
                Debug.LogError($"[BreathTower] Failed to save data: {e.Message}");
            }
        }

        // ── High-level operations (mirror storage.ts) ───────────────────────

        public static void MarkTutorialSeen()
        {
            Data.hasSeenTutorial = true;
            Save();
        }

        public static void SaveSession(SessionRecord session)
        {
            Data.sessions.Insert(0, session);
            Data.totalFloors += session.totalFloors;
            Save();
        }

        public static SessionRecord GetSessionById(string id)
        {
            foreach (var s in Data.sessions)
                if (s.id == id) return s;
            return null;
        }

        public static void UpdateSettings(AppSettings settings)
        {
            Data.settings.soundEnabled = settings.soundEnabled;
            Data.settings.vibrationEnabled = settings.vibrationEnabled;
            Save();
        }

        public static void ResetAllData()
        {
            _cache = new AppData();
            Save();
        }

        public static void SaveCustomRhythm(CustomRhythm rhythm)
        {
            Data.settings.customRhythms.Add(rhythm);
            Save();
        }

        public static void DeleteCustomRhythm(string id)
        {
            Data.settings.customRhythms.RemoveAll(r => r.id == id);
            Save();
        }

        public static string GenerateSessionId()
        {
            return $"session_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid().ToString("N").Substring(0, 9)}";
        }

        /// <summary>Computes earned badges. Mirrors computeBadges() in storage.ts.</summary>
        public static List<string> ComputeBadges(SessionRecord s)
        {
            var badges = new List<string>();
            if (s.cyclesCompleted >= 10) badges.Add("Peak Breath");
            if (s.focusFloors >= 1) badges.Add("Focus Master");
            if (s.calmBlocks >= 3) badges.Add("Calm Spirit");
            if (s.cyclesCompleted >= 20) badges.Add("Tower Builder");
            if (s.cyclesCompleted >= 30) badges.Add("Sky Raiser");
            return badges;
        }
    }
}
