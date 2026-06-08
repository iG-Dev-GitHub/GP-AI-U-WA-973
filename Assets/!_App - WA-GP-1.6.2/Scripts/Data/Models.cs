using System;
using System.Collections.Generic;

namespace BreathTower.Data
{
    // All models mirror src/store/storage.ts. They are plain [Serializable]
    // classes with public fields so Unity's JsonUtility can round-trip them.

    [Serializable]
    public class BlockRecord
    {
        public string id;
        public string type;        // perfect | standard | calm | focus | crooked
        public int cycleNum;
        public string techniqueId;
    }

    [Serializable]
    public class SessionRecord
    {
        public string id;
        public string date;        // ISO-8601 string
        public string techniqueId;
        public string techniqueName;
        public int cyclesCompleted;
        public int perfectBlocks;
        public int calmBlocks;
        public int focusFloors;
        public int totalFloors;
        public List<string> badges = new List<string>();
        public int durationSeconds;
        public List<BlockRecord> blocks = new List<BlockRecord>();
    }

    [Serializable]
    public class CustomRhythm
    {
        public string id;
        public string name;
        public int inhale;
        public int hold;
        public int exhale;
        public int hold2;
    }

    [Serializable]
    public class AppSettings
    {
        public bool soundEnabled = true;
        public bool vibrationEnabled = true;
        public List<CustomRhythm> customRhythms = new List<CustomRhythm>();
    }

    [Serializable]
    public class AppData
    {
        public int totalFloors;
        public List<SessionRecord> sessions = new List<SessionRecord>();
        public bool hasSeenTutorial;
        public AppSettings settings = new AppSettings();
    }
}
