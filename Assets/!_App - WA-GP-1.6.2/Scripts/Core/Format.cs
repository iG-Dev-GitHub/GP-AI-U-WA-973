using System;
using System.Globalization;

namespace BreathTower.Core
{
    /// <summary>Display formatting helpers shared across screens.</summary>
    public static class Format
    {
        /// <summary>m:ss (e.g. 1:05). Mirrors home.tsx formatDuration.</summary>
        public static string DurationClock(int seconds)
        {
            int m = seconds / 60;
            int s = seconds % 60;
            return $"{m}:{s:00}";
        }

        /// <summary>"12s" or "1m 5s". Mirrors summary.tsx formatDuration.</summary>
        public static string DurationWords(int seconds)
        {
            int m = seconds / 60;
            int s = seconds % 60;
            return m == 0 ? $"{s}s" : $"{m}m {s}s";
        }

        /// <summary>"Jun 8, 2026". Mirrors toLocaleDateString('en-US', ...).</summary>
        public static string Date(string iso)
        {
            if (DateTime.TryParse(iso, null, DateTimeStyles.RoundtripKind, out var d))
                return d.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
            return iso;
        }
    }
}
