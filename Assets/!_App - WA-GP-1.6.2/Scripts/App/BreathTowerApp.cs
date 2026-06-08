using System;
using System.Collections.Generic;
using BreathTower.Core;
using BreathTower.Data;
using BreathTower.Screens;
using BreathTower.UI;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

namespace BreathTower.App
{
    /// <summary>
    /// Application root. This is the single MonoBehaviour placed in the scene.
    /// It builds the UI Toolkit panel entirely in code (UIDocument + a runtime
    /// PanelSettings), owns navigation, and drives the active screen's ticks.
    ///
    /// No Resources.Load is used anywhere: every asset comes from the
    /// serialized <see cref="AppAssets"/> reference, making the build robust
    /// to obfuscation and asset renaming.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BreathTowerApp : MonoBehaviour
    {
        [SerializeField] private AppAssets assets;

        private UIDocument _document;
        private SafeAreaElement _safeArea;
        private VisualElement _screenHost;

        private ScreenBase _current;
        private readonly Stack<(ScreenId id, NavArgs args)> _history = new Stack<(ScreenId, NavArgs)>();

        public AppAssets Assets => assets;

        // ── Lifecycle ───────────────────────────────────────────────────────
        private void Awake()
        {
            if (assets == null)
            {
                Debug.LogError("[BreathTower] AppAssets reference is missing on BreathTowerApp.");
                enabled = false;
                return;
            }

            SaveSystem.Load();
            BuildPanel();
        }

        private void Start()
        {
            // Entry route mirrors app/index.tsx: tutorial gate.
            var first = SaveSystem.Data.hasSeenTutorial ? ScreenId.Home : ScreenId.Welcome;
            Navigate(first, NavArgs.Empty, clearHistory: true, animate: false);
        }

        private void Update()
        {
            _current?.Tick(Time.deltaTime);

            // Hardware/Android back button.
            if (Input.GetKeyDown(KeyCode.Escape))
                Back();
        }

        // ── Panel construction (all code, no scene wiring) ──────────────────
        private void BuildPanel()
        {
            _document = gameObject.GetComponent<UIDocument>();
            if (_document == null) _document = gameObject.AddComponent<UIDocument>();

            _document.panelSettings = CreatePanelSettings();

            var root = _document.rootVisualElement;
            root.style.flexGrow = 1;

            // App-wide font (TrueType, rendered dynamically — no atlas baking).
            // Set both the legacy `-unity-font` and the modern
            // `-unity-font-definition` so text renders regardless of which path
            // the (intentionally empty) runtime theme resolves.
            if (assets.primaryFont != null)
            {
                root.style.unityFont = assets.primaryFont;
                root.style.unityFontDefinition = FontDefinition.FromFont(assets.primaryFont);
            }

            // Shared USS.
            if (assets.appStyleSheet != null)
                root.styleSheets.Add(assets.appStyleSheet);

            // Night-sky background behind everything.
            root.style.backgroundColor = Theme.Bg;

            _safeArea = new SafeAreaElement();
            root.Add(_safeArea);

            _screenHost = new VisualElement { name = "screen-host" };
            _screenHost.style.flexGrow = 1;
            _screenHost.style.overflow = Overflow.Hidden;
            _safeArea.Add(_screenHost);
        }

        private PanelSettings CreatePanelSettings()
        {
            var ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.name = "BreathTowerPanelSettings";

            // An empty runtime theme avoids a hand-authored .tss asset while
            // satisfying PanelSettings' theme requirement. All visuals come
            // from our USS + inline styles + the app font.
            ps.themeStyleSheet = ScriptableObject.CreateInstance<ThemeStyleSheet>();

            // Responsive scaling: design at a 390x844 reference (modern phone)
            // and scale to fit any aspect ratio.
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            ps.referenceResolution = new Vector2Int(390, 844);
            ps.match = 0.5f;

            ps.clearColor = true;
            ps.colorClearValue = Theme.Bg;
            return ps;
        }

        // ── Navigation API ──────────────────────────────────────────────────
        public void Navigate(ScreenId id, NavArgs args = null, bool clearHistory = false, bool animate = true)
        {
            if (clearHistory) _history.Clear();
            if (_current != null)
                _history.Push((_current.Id, _currentArgs));

            ShowScreen(id, args, animate, fromRight: true);
        }

        /// <summary>Replaces the current screen without growing history (router.replace).</summary>
        public void Replace(ScreenId id, NavArgs args = null, bool animate = true)
        {
            ShowScreen(id, args, animate, fromRight: true);
        }

        public void Back()
        {
            if (_history.Count == 0) return;
            var (id, args) = _history.Pop();
            ShowScreen(id, args, animate: true, fromRight: false);
        }

        private NavArgs _currentArgs = NavArgs.Empty;

        private void ShowScreen(ScreenId id, NavArgs args, bool animate, bool fromRight)
        {
            args ??= NavArgs.Empty;

            var outgoing = _current;
            var outgoingRoot = outgoing?.Root;

            _current?.Dispose();

            var screen = CreateScreen(id);
            screen.Init(this, id, args);
            _currentArgs = args;
            _current = screen;

            var newRoot = screen.Build();
            screen.Root = newRoot;
            newRoot.style.position = Position.Absolute;
            newRoot.style.left = 0;
            newRoot.style.top = 0;
            newRoot.style.right = 0;
            newRoot.style.bottom = 0;
            _screenHost.Add(newRoot);

            if (animate)
            {
                float w = _screenHost.resolvedStyle.width;
                if (w <= 0) w = Screen.width;
                float fromX = fromRight ? w : -w * 0.4f;
                AnimateTranslateX(newRoot, fromX, 0f, 240);

                if (outgoingRoot != null)
                {
                    float toX = fromRight ? -w * 0.4f : w;
                    AnimateTranslateX(outgoingRoot, 0f, toX, 240, () => outgoingRoot.RemoveFromHierarchy());
                }
            }
            else
            {
                outgoingRoot?.RemoveFromHierarchy();
            }
        }

        private void AnimateTranslateX(VisualElement e, float from, float to, int durationMs, Action onComplete = null)
        {
            e.style.translate = new Translate(from, 0, 0);
            e.experimental.animation
                .Start(from, to, durationMs, (el, v) => el.style.translate = new Translate(v, 0, 0))
                .Ease(Easing.OutCubic)
                .OnCompleted(() => onComplete?.Invoke());
        }

        private ScreenBase CreateScreen(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Welcome:   return new WelcomeScreen();
                case ScreenId.Home:      return new HomeScreen();
                case ScreenId.Technique: return new TechniqueScreen();
                case ScreenId.Session:   return new SessionScreen();
                case ScreenId.Summary:   return new SummaryScreen();
                case ScreenId.Tower:     return new TowerScreen();
                case ScreenId.Settings:  return new SettingsScreen();
                default:                 return new HomeScreen();
            }
        }
    }
}
