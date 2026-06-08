using System;
using BreathTower.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace BreathTower.UI
{
    /// <summary>
    /// Reusable UI Toolkit widget factory. Centralizes the shared visual
    /// vocabulary of the app (cards, pill buttons, gradient buttons, scroll
    /// views without scrollbars, etc.) so screens stay declarative and we
    /// never duplicate styling.
    /// </summary>
    public static class Widgets
    {
        // ── Text ────────────────────────────────────────────────────────────
        public static Label Text(string value, float size, Color color, FontStyle weight = FontStyle.Normal)
        {
            var l = new Label(value);
            l.style.fontSize = size;
            l.style.color = color;
            l.style.unityFontStyleAndWeight = weight;
            l.style.whiteSpace = WhiteSpace.Normal;
            l.style.marginLeft = l.style.marginRight = 0;
            l.style.marginTop = l.style.marginBottom = 0;
            l.style.paddingLeft = l.style.paddingRight = 0;
            l.style.paddingTop = l.style.paddingBottom = 0;
            return l;
        }

        // ── Containers ──────────────────────────────────────────────────────
        public static VisualElement Box() => new VisualElement();

        public static VisualElement Row()
        {
            var v = new VisualElement();
            v.style.flexDirection = FlexDirection.Row;
            return v;
        }

        public static VisualElement Card()
        {
            var v = new VisualElement();
            v.style.backgroundColor = Theme.Surface;
            v.style.borderTopLeftRadius = v.style.borderTopRightRadius =
                v.style.borderBottomLeftRadius = v.style.borderBottomRightRadius = Theme.RadiusLg;
            v.style.paddingLeft = v.style.paddingRight = v.style.paddingTop = v.style.paddingBottom = Theme.SpaceMd;
            return v;
        }

        // ── Drawn glyphs (font-independent icons) ───────────────────────────
        // The bundled font lacks emoji/symbol glyphs, so prominent decorative
        // icons are drawn from primitives instead of relying on font coverage.

        /// <summary>A small stacked-tower icon (3 colored bars).</summary>
        public static VisualElement TowerGlyph(float blockW = 44f)
        {
            var v = new VisualElement();
            v.style.alignItems = Align.Center;
            var colors = new[] { Theme.TechniqueBox, Theme.BlockCalm, Theme.BlockPerfect };
            foreach (var col in colors)
            {
                var b = new VisualElement();
                b.style.width = blockW; b.style.height = 9;
                b.style.backgroundColor = col;
                b.style.borderTopLeftRadius = b.style.borderTopRightRadius =
                    b.style.borderBottomLeftRadius = b.style.borderBottomRightRadius = 2;
                b.style.marginBottom = 2;
                v.Add(b);
            }
            return v;
        }

        /// <summary>A hollow breathing-ring icon.</summary>
        public static VisualElement RingGlyph(float size = 60f)
        {
            var v = new VisualElement();
            v.style.width = size; v.style.height = size;
            v.style.borderTopLeftRadius = v.style.borderTopRightRadius =
                v.style.borderBottomLeftRadius = v.style.borderBottomRightRadius = size / 2f;
            v.style.borderTopWidth = v.style.borderBottomWidth =
                v.style.borderLeftWidth = v.style.borderRightWidth = 6;
            v.style.borderTopColor = v.style.borderBottomColor =
                v.style.borderLeftColor = v.style.borderRightColor = Theme.Cyan;
            return v;
        }

        // ── Scroll view without visible scrollbars ──────────────────────────
        /// <summary>
        /// Creates a vertical ScrollView whose scrollbars are hidden while all
        /// input methods (touch drag, mouse wheel, trackpad) keep working.
        /// </summary>
        public static ScrollView VerticalScroll()
        {
            var sv = new ScrollView(ScrollViewMode.Vertical);
            ConfigureScrollbars(sv);
            return sv;
        }

        public static ScrollView HorizontalScroll()
        {
            var sv = new ScrollView(ScrollViewMode.Horizontal);
            ConfigureScrollbars(sv);
            return sv;
        }

        /// <summary>
        /// Locks the direct children of a ScrollView's content to their natural
        /// size (flex-shrink: 0) so the content is never compressed to fit the
        /// viewport — instead it overflows and scrolls. Call after the screen's
        /// content has been added. Idempotent and safe to re-run.
        /// </summary>
        public static void LockScrollContent(ScrollView sv)
        {
            foreach (var child in sv.contentContainer.Children())
                child.style.flexShrink = 0;
        }

        /// <summary>
        /// Hides both scrollbars on any ScrollView but preserves scrolling via
        /// touch, drag, and mouse wheel. Applied app-wide to satisfy the
        /// "no visible scrollbars" requirement.
        /// </summary>
        public static void ConfigureScrollbars(ScrollView sv)
        {
            sv.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            sv.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
            sv.mouseWheelScrollSize = 80f;
            // Smooth, clamped touch dragging so content can be flung on mobile.
            sv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            sv.elasticity = 0.1f;
            sv.scrollDecelerationRate = 0.135f;
        }

        // ── Buttons ─────────────────────────────────────────────────────────
        /// <summary>A full-width pill button filled with a horizontal gradient.</summary>
        public static VisualElement GradientButton(string label, Action onClick, float fontSize = Theme.FontLg)
        {
            var btn = new VisualElement();
            btn.style.borderTopLeftRadius = btn.style.borderTopRightRadius =
                btn.style.borderBottomLeftRadius = btn.style.borderBottomRightRadius = Theme.RadiusPill;
            btn.style.overflow = Overflow.Hidden;
            btn.style.flexDirection = FlexDirection.Row;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;
            btn.style.paddingTop = btn.style.paddingBottom = Theme.SpaceMd;
            btn.style.backgroundImage = new StyleBackground(Gradients.Horizontal(Theme.Cyan, Theme.TechniqueBox));

            var l = Text(label, fontSize, Theme.TextPrimary, FontStyle.Bold);
            l.pickingMode = PickingMode.Ignore;
            btn.Add(l);

            MakeClickable(btn, onClick);
            return btn;
        }

        /// <summary>A small circular icon-ish button (text glyph) on a surface.</summary>
        public static VisualElement RoundButton(string glyph, Action onClick, float size = 40f)
        {
            var btn = new VisualElement();
            btn.style.width = size;
            btn.style.height = size;
            btn.style.borderTopLeftRadius = btn.style.borderTopRightRadius =
                btn.style.borderBottomLeftRadius = btn.style.borderBottomRightRadius = size / 2f;
            btn.style.backgroundColor = Theme.Surface;
            btn.style.alignItems = Align.Center;
            btn.style.justifyContent = Justify.Center;

            var l = Text(glyph, Theme.FontLg, Theme.TextSecondary, FontStyle.Bold);
            l.pickingMode = PickingMode.Ignore;
            btn.Add(l);

            MakeClickable(btn, onClick);
            return btn;
        }

        /// <summary>Adds press feedback + click handling to any element.</summary>
        public static void MakeClickable(VisualElement e, Action onClick)
        {
            e.pickingMode = PickingMode.Position;
            e.RegisterCallback<PointerDownEvent>(_ => e.style.opacity = 0.7f);
            e.RegisterCallback<PointerUpEvent>(_ => e.style.opacity = 1f);
            e.RegisterCallback<PointerLeaveEvent>(_ => e.style.opacity = 1f);
            var manipulator = new Clickable(() => onClick?.Invoke());
            e.AddManipulator(manipulator);
        }
    }
}
