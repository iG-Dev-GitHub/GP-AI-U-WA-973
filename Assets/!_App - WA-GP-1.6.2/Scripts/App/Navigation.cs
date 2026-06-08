using System.Collections.Generic;
using UnityEngine.UIElements;

namespace BreathTower.App
{
    /// <summary>Identifiers for every routable screen (mirrors expo-router routes).</summary>
    public enum ScreenId
    {
        Splash,
        Welcome,
        Home,
        Technique,
        Session,
        Summary,
        Tower,
        Settings,
    }

    /// <summary>
    /// Lightweight, typed navigation parameters — the Unity equivalent of
    /// expo-router's query params. Avoids passing loose strings around.
    /// </summary>
    public sealed class NavArgs
    {
        private readonly Dictionary<string, string> _values = new Dictionary<string, string>();

        public NavArgs Set(string key, string value)
        {
            _values[key] = value;
            return this;
        }

        public NavArgs Set(string key, int value) => Set(key, value.ToString());

        public string Get(string key, string fallback = "")
            => _values.TryGetValue(key, out var v) ? v : fallback;

        public int GetInt(string key, int fallback)
            => _values.TryGetValue(key, out var v) && int.TryParse(v, out var n) ? n : fallback;

        public static NavArgs Empty => new NavArgs();
    }

    /// <summary>
    /// Base class for screens. A screen builds its own VisualElement tree and
    /// optionally receives per-frame ticks for animation/timers.
    /// </summary>
    public abstract class ScreenBase
    {
        protected BreathTowerApp App { get; private set; }
        protected NavArgs Args { get; private set; }

        public ScreenId Id { get; private set; }
        public VisualElement Root { get; set; }

        public void Init(BreathTowerApp app, ScreenId id, NavArgs args)
        {
            App = app;
            Id = id;
            Args = args ?? NavArgs.Empty;
        }

        /// <summary>Build and return the screen's root element.</summary>
        public abstract VisualElement Build();

        /// <summary>Per-frame update (only the active screen receives this).</summary>
        public virtual void Tick(float deltaTime) { }

        /// <summary>Called right before the screen is removed; release timers here.</summary>
        public virtual void Dispose() { }
    }
}
