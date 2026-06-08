using UnityEngine;
using UnityEngine.UIElements;

namespace BreathTower.UI
{
    /// <summary>
    /// A container that applies device safe-area insets (notches, status bars,
    /// Dynamic Island, camera cutouts, gesture bars) as padding, converting
    /// screen-space pixels into panel-space points. Re-evaluates whenever the
    /// screen geometry changes, so it survives rotation and multi-window.
    /// </summary>
    public sealed class SafeAreaElement : VisualElement
    {
        private Rect _lastSafeArea = new Rect(-1, -1, -1, -1);
        private Vector2 _lastScreen = new Vector2(-1, -1);

        public SafeAreaElement()
        {
            style.flexGrow = 1;
            // Re-check every layout pass; cheap because we early-out when the
            // safe area / screen size has not actually changed.
            RegisterCallback<GeometryChangedEvent>(_ => Apply());
            schedule.Execute(Apply).Every(250);
        }

        public void Apply()
        {
            if (panel == null) return;

            var safe = Screen.safeArea;
            var screen = new Vector2(Screen.width, Screen.height);
            if (screen.x <= 0 || screen.y <= 0) return;
            if (safe == _lastSafeArea && screen == _lastScreen) return;
            _lastSafeArea = safe;
            _lastScreen = screen;

            // Insets in screen pixels.
            float leftPx = safe.xMin;
            float rightPx = screen.x - safe.xMax;
            // Screen.safeArea uses a bottom-left origin; UI is top-left.
            float topPx = screen.y - safe.yMax;
            float bottomPx = safe.yMin;

            // Convert screen pixels → panel points using the panel scale.
            var topLeftPanel = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(leftPx, topPx));
            var origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
            var bottomRightPanel = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(rightPx, bottomPx));

            float left = Mathf.Max(0, topLeftPanel.x - origin.x);
            float top = Mathf.Max(0, topLeftPanel.y - origin.y);
            float right = Mathf.Max(0, bottomRightPanel.x - origin.x);
            float bottom = Mathf.Max(0, bottomRightPanel.y - origin.y);

            style.paddingLeft = left;
            style.paddingTop = top;
            style.paddingRight = right;
            style.paddingBottom = bottom;
        }
    }
}
