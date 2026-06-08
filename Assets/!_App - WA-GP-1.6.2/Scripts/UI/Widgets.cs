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

        // ── Vector glyphs (Painter2D) ───────────────────────────────────────
        // The bundled font has no emoji coverage, so the reference app's emoji
        // icons (🧘 🌟 ✨ 🏗️) are reproduced as crisp vector shapes drawn with
        // Painter2D — matching the originals far better than primitive bars.

        /// <summary>Creates a decorative, non-interactive icon drawn via Painter2D.</summary>
        private static VisualElement VectorIcon(float w, float h, Action<Painter2D, Rect> draw)
        {
            var ve = new VisualElement();
            ve.style.width = w;
            ve.style.height = h;
            ve.pickingMode = PickingMode.Ignore;
            ve.generateVisualContent += mgc =>
            {
                var r = ve.contentRect;
                if (r.width <= 0f || r.height <= 0f) return;
                draw(mgc.painter2D, r);
            };
            // Repaint once layout assigns a real size.
            ve.RegisterCallback<GeometryChangedEvent>(_ => ve.MarkDirtyRepaint());
            return ve;
        }

        private static void PolyStar(Painter2D p, Vector2 c, float outer, float inner, int points, float startAngle)
        {
            p.BeginPath();
            int n = points * 2;
            for (int i = 0; i < n; i++)
            {
                float ang = startAngle + i * Mathf.PI / points;
                float rad = (i % 2 == 0) ? outer : inner;
                var pt = new Vector2(c.x + Mathf.Cos(ang) * rad, c.y + Mathf.Sin(ang) * rad);
                if (i == 0) p.MoveTo(pt); else p.LineTo(pt);
            }
            p.ClosePath();
            p.Fill();
        }

        private static void FillCircle(Painter2D p, Vector2 c, float radius)
        {
            p.BeginPath();
            const int seg = 28;
            for (int i = 0; i <= seg; i++)
            {
                float a = i / (float)seg * Mathf.PI * 2f;
                var pt = new Vector2(c.x + Mathf.Cos(a) * radius, c.y + Mathf.Sin(a) * radius);
                if (i == 0) p.MoveTo(pt); else p.LineTo(pt);
            }
            p.ClosePath();
            p.Fill();
        }

        /// <summary>A five-pointed star (reference 🌟).</summary>
        public static VisualElement StarGlyph(float size, Color color)
        {
            return VectorIcon(size, size, (p, r) =>
            {
                p.fillColor = color;
                float outer = Mathf.Min(r.width, r.height) * 0.48f;
                PolyStar(p, r.center, outer, outer * 0.42f, 5, -Mathf.PI / 2f);
            });
        }

        /// <summary>A four-point sparkle (reference ✨).</summary>
        public static VisualElement SparkleGlyph(float size, Color color)
        {
            return VectorIcon(size, size, (p, r) =>
            {
                p.fillColor = color;
                float big = Mathf.Min(r.width, r.height) * 0.5f;
                PolyStar(p, r.center, big, big * 0.2f, 4, -Mathf.PI / 2f);
                // Two small accent sparkles.
                float s = big * 0.28f;
                PolyStar(p, new Vector2(r.x + r.width * 0.82f, r.y + r.height * 0.22f), s, s * 0.2f, 4, -Mathf.PI / 2f);
                PolyStar(p, new Vector2(r.x + r.width * 0.20f, r.y + r.height * 0.80f), s * 0.8f, s * 0.16f, 4, -Mathf.PI / 2f);
            });
        }

        /// <summary>A seated meditation figure (reference 🧘).</summary>
        public static VisualElement MeditationGlyph(float size, Color color)
        {
            return VectorIcon(size, size, (p, r) =>
            {
                float w = r.width, h = r.height, cx = r.center.x;
                p.fillColor = color;

                // Head.
                FillCircle(p, new Vector2(cx, r.y + h * 0.20f), h * 0.13f);

                // Torso + crossed legs (rounded wide triangle).
                float baseY = r.y + h * 0.84f;
                float peakY = r.y + h * 0.40f;
                float half = w * 0.42f;
                p.BeginPath();
                p.MoveTo(new Vector2(cx - half, baseY));
                p.LineTo(new Vector2(cx, peakY));
                p.LineTo(new Vector2(cx + half, baseY));
                p.ClosePath();
                p.Fill();

                // Arms resting on the knees.
                p.strokeColor = color;
                p.lineWidth = h * 0.11f;
                p.lineCap = LineCap.Round;
                p.BeginPath();
                p.MoveTo(new Vector2(cx - half * 0.92f, r.y + h * 0.60f));
                p.LineTo(new Vector2(cx + half * 0.92f, r.y + h * 0.60f));
                p.Stroke();
            });
        }

        /// <summary>A tower crane lifting a block (reference 🏗️).</summary>
        public static VisualElement ConstructionGlyph(float size, Color craneColor, Color blockColor)
        {
            return VectorIcon(size, size, (p, r) =>
            {
                float w = r.width, h = r.height, x0 = r.x, y0 = r.y;
                p.strokeColor = craneColor;
                p.lineWidth = Mathf.Max(2f, h * 0.07f);
                p.lineCap = LineCap.Round;
                p.lineJoin = LineJoin.Round;

                float mastX = x0 + w * 0.66f;
                float jibY = y0 + h * 0.18f;

                // Mast.
                p.BeginPath();
                p.MoveTo(new Vector2(mastX, jibY));
                p.LineTo(new Vector2(mastX, y0 + h * 0.86f));
                p.Stroke();
                // Foot.
                p.BeginPath();
                p.MoveTo(new Vector2(x0 + w * 0.50f, y0 + h * 0.86f));
                p.LineTo(new Vector2(x0 + w * 0.82f, y0 + h * 0.86f));
                p.Stroke();
                // Jib (top arm) reaching left, plus a short counter-jib.
                p.BeginPath();
                p.MoveTo(new Vector2(x0 + w * 0.12f, jibY));
                p.LineTo(new Vector2(x0 + w * 0.84f, jibY));
                p.Stroke();
                // Brace from mast up to the jib.
                p.BeginPath();
                p.MoveTo(new Vector2(mastX, y0 + h * 0.34f));
                p.LineTo(new Vector2(x0 + w * 0.40f, jibY));
                p.Stroke();

                // Hoist cable + hanging block.
                float cableX = x0 + w * 0.26f;
                p.BeginPath();
                p.MoveTo(new Vector2(cableX, jibY));
                p.LineTo(new Vector2(cableX, y0 + h * 0.44f));
                p.Stroke();

                float bw = w * 0.22f, bh = h * 0.16f, top = y0 + h * 0.44f;
                p.fillColor = blockColor;
                p.BeginPath();
                p.MoveTo(new Vector2(cableX - bw / 2f, top));
                p.LineTo(new Vector2(cableX + bw / 2f, top));
                p.LineTo(new Vector2(cableX + bw / 2f, top + bh));
                p.LineTo(new Vector2(cableX - bw / 2f, top + bh));
                p.ClosePath();
                p.Fill();
            });
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
            // Direct (clamped) touch dragging that stops the moment the finger
            // lifts. Inertia is disabled on purpose: while a fling is still
            // decelerating, the ScrollView swallows the next PointerDown to halt
            // the glide, so the first tap on a button only stops the scroll and
            // the user has to tap again. With no lingering momentum, every tap
            // reaches the button on the first try.
            sv.touchScrollBehavior = ScrollView.TouchScrollBehavior.Clamped;
            sv.elasticity = 0f;
            sv.scrollDecelerationRate = 0f;
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
