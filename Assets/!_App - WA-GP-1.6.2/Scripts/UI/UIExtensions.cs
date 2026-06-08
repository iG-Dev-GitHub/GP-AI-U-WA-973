using UnityEngine;
using UnityEngine.UIElements;

namespace BreathTower.UI
{
    /// <summary>
    /// Fluent helpers for building UI Toolkit trees in C#. They wrap the
    /// verbose <c>element.style.*</c> API so screen code reads closer to the
    /// reference app's StyleSheet objects.
    /// </summary>
    public static class UIExtensions
    {
        // ── Generic / hierarchy ─────────────────────────────────────────────
        public static T Add<T>(this VisualElement parent, T child) where T : VisualElement
        {
            parent.Add(child);
            return child;
        }

        public static T Name<T>(this T e, string name) where T : VisualElement
        {
            e.name = name;
            return e;
        }

        public static T Class<T>(this T e, params string[] classes) where T : VisualElement
        {
            foreach (var c in classes) e.AddToClassList(c);
            return e;
        }

        // ── Layout ──────────────────────────────────────────────────────────
        public static T Flex<T>(this T e, float grow) where T : VisualElement
        {
            e.style.flexGrow = grow;
            return e;
        }

        public static T Row<T>(this T e) where T : VisualElement
        {
            e.style.flexDirection = FlexDirection.Row;
            return e;
        }

        public static T Col<T>(this T e) where T : VisualElement
        {
            e.style.flexDirection = FlexDirection.Column;
            return e;
        }

        public static T Wrap<T>(this T e) where T : VisualElement
        {
            e.style.flexWrap = UnityEngine.UIElements.Wrap.Wrap;
            return e;
        }

        public static T AlignItems<T>(this T e, Align a) where T : VisualElement
        {
            e.style.alignItems = a;
            return e;
        }

        public static T AlignSelf<T>(this T e, Align a) where T : VisualElement
        {
            e.style.alignSelf = a;
            return e;
        }

        public static T Justify<T>(this T e, UnityEngine.UIElements.Justify j) where T : VisualElement
        {
            e.style.justifyContent = j;
            return e;
        }

        public static T Center<T>(this T e) where T : VisualElement
        {
            e.style.alignItems = Align.Center;
            e.style.justifyContent = UnityEngine.UIElements.Justify.Center;
            return e;
        }

        // ── Sizing ──────────────────────────────────────────────────────────
        public static T Size<T>(this T e, float w, float h) where T : VisualElement
        {
            e.style.width = w;
            e.style.height = h;
            return e;
        }

        public static T Width<T>(this T e, float w) where T : VisualElement { e.style.width = w; return e; }
        public static T WidthPct<T>(this T e, float p) where T : VisualElement { e.style.width = Length.Percent(p); return e; }
        public static T Height<T>(this T e, float h) where T : VisualElement { e.style.height = h; return e; }
        public static T MinHeight<T>(this T e, float h) where T : VisualElement { e.style.minHeight = h; return e; }
        public static T MinWidthPct<T>(this T e, float p) where T : VisualElement { e.style.minWidth = Length.Percent(p); return e; }
        public static T MaxWidth<T>(this T e, float w) where T : VisualElement { e.style.maxWidth = w; return e; }

        // ── Spacing ─────────────────────────────────────────────────────────
        public static T Pad<T>(this T e, float all) where T : VisualElement
        {
            e.style.paddingLeft = e.style.paddingRight = e.style.paddingTop = e.style.paddingBottom = all;
            return e;
        }

        public static T Pad<T>(this T e, float v, float h) where T : VisualElement
        {
            e.style.paddingTop = e.style.paddingBottom = v;
            e.style.paddingLeft = e.style.paddingRight = h;
            return e;
        }

        public static T PadH<T>(this T e, float h) where T : VisualElement { e.style.paddingLeft = e.style.paddingRight = h; return e; }
        public static T PadV<T>(this T e, float v) where T : VisualElement { e.style.paddingTop = e.style.paddingBottom = v; return e; }
        public static T PadTop<T>(this T e, float v) where T : VisualElement { e.style.paddingTop = v; return e; }
        public static T PadBottom<T>(this T e, float v) where T : VisualElement { e.style.paddingBottom = v; return e; }

        public static T Margin<T>(this T e, float all) where T : VisualElement
        {
            e.style.marginLeft = e.style.marginRight = e.style.marginTop = e.style.marginBottom = all;
            return e;
        }

        public static T MarginH<T>(this T e, float h) where T : VisualElement { e.style.marginLeft = e.style.marginRight = h; return e; }
        public static T MarginV<T>(this T e, float v) where T : VisualElement { e.style.marginTop = e.style.marginBottom = v; return e; }
        public static T MarginTop<T>(this T e, float v) where T : VisualElement { e.style.marginTop = v; return e; }
        public static T MarginBottom<T>(this T e, float v) where T : VisualElement { e.style.marginBottom = v; return e; }
        public static T MarginRight<T>(this T e, float v) where T : VisualElement { e.style.marginRight = v; return e; }

        /// <summary>
        /// Emulates flex-gap (absent from UI Toolkit) by margining every child
        /// except the last. Call AFTER the children have been added. The gap
        /// direction follows the container's current flex-direction.
        /// </summary>
        public static T GapChildren<T>(this T e, float g) where T : VisualElement
        {
            bool row = e.resolvedStyle.flexDirection == FlexDirection.Row ||
                       e.resolvedStyle.flexDirection == FlexDirection.RowReverse ||
                       e.style.flexDirection.value == FlexDirection.Row ||
                       e.style.flexDirection.value == FlexDirection.RowReverse;
            int count = e.childCount;
            for (int i = 0; i < count; i++)
            {
                var child = e[i];
                if (i == count - 1)
                {
                    if (row) child.style.marginRight = 0; else child.style.marginBottom = 0;
                }
                else
                {
                    if (row) child.style.marginRight = g; else child.style.marginBottom = g;
                }
            }
            return e;
        }

        // ── Visuals ─────────────────────────────────────────────────────────
        public static T Bg<T>(this T e, Color c) where T : VisualElement
        {
            e.style.backgroundColor = c;
            return e;
        }

        public static T BgImage<T>(this T e, Texture2D tex) where T : VisualElement
        {
            e.style.backgroundImage = new StyleBackground(tex);
            return e;
        }

        public static T Radius<T>(this T e, float r) where T : VisualElement
        {
            e.style.borderTopLeftRadius = e.style.borderTopRightRadius =
                e.style.borderBottomLeftRadius = e.style.borderBottomRightRadius = r;
            return e;
        }

        public static T Border<T>(this T e, float width, Color color) where T : VisualElement
        {
            e.style.borderTopWidth = e.style.borderBottomWidth =
                e.style.borderLeftWidth = e.style.borderRightWidth = width;
            e.style.borderTopColor = e.style.borderBottomColor =
                e.style.borderLeftColor = e.style.borderRightColor = color;
            return e;
        }

        public static T SetBorderColor<T>(this T e, Color color) where T : VisualElement
        {
            e.style.borderTopColor = e.style.borderBottomColor =
                e.style.borderLeftColor = e.style.borderRightColor = color;
            return e;
        }

        public static T Overflow<T>(this T e, bool hidden) where T : VisualElement
        {
            e.style.overflow = hidden ? UnityEngine.UIElements.Overflow.Hidden : UnityEngine.UIElements.Overflow.Visible;
            return e;
        }

        public static T Opacity<T>(this T e, float o) where T : VisualElement
        {
            e.style.opacity = o;
            return e;
        }

        // ── Absolute positioning ────────────────────────────────────────────
        public static T Absolute<T>(this T e) where T : VisualElement
        {
            e.style.position = Position.Absolute;
            return e;
        }

        public static T Pos<T>(this T e, float? left = null, float? top = null, float? right = null, float? bottom = null) where T : VisualElement
        {
            if (left.HasValue) e.style.left = left.Value;
            if (top.HasValue) e.style.top = top.Value;
            if (right.HasValue) e.style.right = right.Value;
            if (bottom.HasValue) e.style.bottom = bottom.Value;
            return e;
        }

        public static T Inset0<T>(this T e) where T : VisualElement
        {
            e.style.position = Position.Absolute;
            e.style.left = 0; e.style.top = 0; e.style.right = 0; e.style.bottom = 0;
            return e;
        }

        public static T NoPick<T>(this T e) where T : VisualElement
        {
            e.pickingMode = PickingMode.Ignore;
            return e;
        }

        // ── Text ────────────────────────────────────────────────────────────
        public static Label FontSize(this Label e, float size) { e.style.fontSize = size; return e; }
        public static Label FontColor(this Label e, Color c) { e.style.color = c; return e; }

        public static Label Bold(this Label e)
        {
            e.style.unityFontStyleAndWeight = FontStyle.Bold;
            return e;
        }

        public static Label TextCenter(this Label e)
        {
            e.style.unityTextAlign = TextAnchor.MiddleCenter;
            return e;
        }

        public static Label Letter(this Label e, float spacing)
        {
            e.style.letterSpacing = spacing;
            return e;
        }

        public static Label WhiteSpaceNormal(this Label e)
        {
            e.style.whiteSpace = WhiteSpace.Normal;
            return e;
        }
    }
}
