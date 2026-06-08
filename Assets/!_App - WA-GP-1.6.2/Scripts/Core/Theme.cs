using UnityEngine;

namespace BreathTower.Core
{
    /// <summary>
    /// Centralized design tokens ported from the reference web app
    /// (src/constants/theme.ts and design_guidelines.json).
    /// Pure data — contains no asset references, so it survives obfuscation.
    /// </summary>
    public static class Theme
    {
        // ── Surfaces / background ────────────────────────────────────────────
        public static readonly Color Bg               = Hex("#0D1B2A");
        public static readonly Color Surface          = Hex("#1B263B");
        public static readonly Color SurfaceAlt        = Hex("#162032");
        public static readonly Color SurfaceHighlight  = Hex("#415A77");
        public static readonly Color CityBuilding      = Hex("#0A1520");
        public static readonly Color HeroBuilding      = Hex("#0D1B2A");

        // ── Text ─────────────────────────────────────────────────────────────
        public static readonly Color TextPrimary   = Hex("#FFFFFF");
        public static readonly Color TextSecondary = Rgba(255, 255, 255, 0.7f);
        public static readonly Color TextMuted     = Rgba(255, 255, 255, 0.4f);

        // ── Blocks ───────────────────────────────────────────────────────────
        public static readonly Color BlockStandard = Hex("#87CEEB");
        public static readonly Color BlockPerfect  = Hex("#FFD700");
        public static readonly Color BlockCalm     = Hex("#00CED1");
        public static readonly Color BlockFocus    = Hex("#FFD700");
        public static readonly Color BlockCrooked  = Hex("#FF6B6B");

        // ── Technique accent colors ─────────────────────────────────────────
        public static readonly Color Technique478    = Hex("#9D4CDD");
        public static readonly Color TechniqueBox     = Hex("#4A9EFF");
        public static readonly Color TechniqueEven    = Hex("#87CEEB");
        public static readonly Color TechniqueCustom  = Hex("#FFFFFF");

        // ── Breathing phase colors ──────────────────────────────────────────
        public static readonly Color Inhale = Hex("#00CED1");
        public static readonly Color Hold   = Hex("#FFFFFF");
        public static readonly Color Exhale = Hex("#4A9EFF");

        // ── Accents ──────────────────────────────────────────────────────────
        public static readonly Color Crane  = Hex("#8FA3B1");
        public static readonly Color Gold   = Hex("#FFD700");
        public static readonly Color Cyan   = Hex("#00CED1");
        public static readonly Color Purple = Hex("#9D4CDD");

        public static readonly Color Success = Hex("#4CAF50");
        public static readonly Color Danger  = Hex("#FF6B6B");
        public static readonly Color Warning = Hex("#FFA726");

        public static readonly Color GradientTop = Hex("#0D1B2A");
        public static readonly Color GradientMid = Hex("#1B263B");

        // Window light color used in the city skylines.
        public static readonly Color WindowLight = Hex("#FFF8C0");

        // ── Spacing (px) ─────────────────────────────────────────────────────
        public const float SpaceXs  = 4f;
        public const float SpaceSm  = 8f;
        public const float SpaceMd  = 16f;
        public const float SpaceLg  = 24f;
        public const float SpaceXl  = 32f;
        public const float SpaceXxl = 48f;

        // ── Radius (px) ──────────────────────────────────────────────────────
        public const float RadiusSm   = 8f;
        public const float RadiusMd   = 12f;
        public const float RadiusLg   = 20f;
        public const float RadiusXl   = 28f;
        public const float RadiusPill = 9999f;

        // ── Font sizes (px) ──────────────────────────────────────────────────
        public const float FontXs   = 11f;
        public const float FontSm   = 13f;
        public const float FontMd   = 15f;
        public const float FontLg   = 18f;
        public const float FontXl   = 22f;
        public const float FontXxl  = 28f;
        public const float FontXxxl = 36f;
        public const float FontHuge = 48f;

        /// <summary>Accent color for a breathing technique id.</summary>
        public static Color TechniqueColor(string id)
        {
            switch (id)
            {
                case "4-7-8": return Technique478;
                case "box":   return TechniqueBox;
                case "even":  return TechniqueEven;
                case "custom": return TechniqueCustom;
                default:       return BlockStandard;
            }
        }

        /// <summary>Color used for a given breathing phase.</summary>
        public static Color PhaseColor(string phaseName)
        {
            if (phaseName == "inhale") return Inhale;
            if (phaseName == "hold" || phaseName == "hold2") return Hold;
            return Exhale;
        }

        /// <summary>Returns a copy of the color with a new alpha (0..1).</summary>
        public static Color WithAlpha(this Color c, float alpha)
        {
            c.a = alpha;
            return c;
        }

        // ── Hex / rgba helpers ──────────────────────────────────────────────
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.magenta;
        }

        public static Color Rgba(int r, int g, int b, float a)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a);
        }
    }
}
