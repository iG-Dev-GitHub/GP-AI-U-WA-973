using System.Collections.Generic;
using System.Text;

namespace BreathTower.Core
{
    /// <summary>A single phase inside a breathing cycle.</summary>
    public struct Phase
    {
        public string Name;      // inhale | hold | exhale | hold2
        public int Duration;     // seconds
        public string Label;     // Inhale | Hold | Exhale
        public string ColorKey;  // inhale | hold | exhale

        public Phase(string name, int duration, string label, string colorKey)
        {
            Name = name;
            Duration = duration;
            Label = label;
            ColorKey = colorKey;
        }
    }

    /// <summary>A full breathing technique definition.</summary>
    public class Technique
    {
        public string Id;
        public string Name;
        public string Description;
        public string Rhythm;
        public List<Phase> Phases;
        public string ColorId;
        public int TotalCycleDuration;
    }

    /// <summary>
    /// Breathing technique catalog + cycle/block logic, ported 1:1 from
    /// the web reference (src/utils/breathing.ts).
    /// </summary>
    public static class Breathing
    {
        public static readonly List<Technique> Techniques = new List<Technique>
        {
            new Technique
            {
                Id = "4-7-8",
                Name = "4-7-8 Breathing",
                Description = "Promotes deep relaxation and helps reduce anxiety. Inhale for 4s, hold for 7s, exhale for 8s.",
                Rhythm = "4 — 7 — 8",
                ColorId = "4-7-8",
                TotalCycleDuration = 19,
                Phases = new List<Phase>
                {
                    new Phase("inhale", 4, "Inhale", "inhale"),
                    new Phase("hold",   7, "Hold",   "hold"),
                    new Phase("exhale", 8, "Exhale", "exhale"),
                },
            },
            new Technique
            {
                Id = "box",
                Name = "Box Breathing",
                Description = "Used by Navy SEALs to manage stress. Equal duration for all 4 phases.",
                Rhythm = "4 — 4 — 4 — 4",
                ColorId = "box",
                TotalCycleDuration = 16,
                Phases = new List<Phase>
                {
                    new Phase("inhale", 4, "Inhale", "inhale"),
                    new Phase("hold",   4, "Hold",   "hold"),
                    new Phase("exhale", 4, "Exhale", "exhale"),
                    new Phase("hold2",  4, "Hold",   "hold"),
                },
            },
            new Technique
            {
                Id = "even",
                Name = "Equal Breathing",
                Description = "Simple and balanced. Equal inhale and exhale creates calm focus.",
                Rhythm = "4 — 4",
                ColorId = "even",
                TotalCycleDuration = 8,
                Phases = new List<Phase>
                {
                    new Phase("inhale", 4, "Inhale", "inhale"),
                    new Phase("exhale", 4, "Exhale", "exhale"),
                },
            },
            new Technique
            {
                Id = "custom",
                Name = "Custom Rhythm",
                Description = "Define your own breathing pattern for a personalized practice.",
                Rhythm = "Custom",
                ColorId = "custom",
                TotalCycleDuration = 10,
                Phases = new List<Phase>
                {
                    new Phase("inhale", 4, "Inhale", "inhale"),
                    new Phase("hold",   2, "Hold",   "hold"),
                    new Phase("exhale", 4, "Exhale", "exhale"),
                },
            },
        };

        public static Technique FindTechnique(string id)
        {
            foreach (var t in Techniques)
                if (t.Id == id) return t;
            return Techniques[0];
        }

        public static Technique BuildCustom(int inhale, int hold, int exhale, int hold2)
        {
            var phases = new List<Phase> { new Phase("inhale", inhale, "Inhale", "inhale") };
            if (hold > 0) phases.Add(new Phase("hold", hold, "Hold", "hold"));
            phases.Add(new Phase("exhale", exhale, "Exhale", "exhale"));
            if (hold2 > 0) phases.Add(new Phase("hold2", hold2, "Hold", "hold"));

            var desc = new StringBuilder();
            desc.Append($"Inhale {inhale}s");
            if (hold > 0) desc.Append($" · Hold {hold}s");
            desc.Append($" · Exhale {exhale}s");
            if (hold2 > 0) desc.Append($" · Hold {hold2}s");

            var parts = new List<string> { inhale.ToString() };
            if (hold > 0) parts.Add(hold.ToString());
            parts.Add(exhale.ToString());
            if (hold2 > 0) parts.Add(hold2.ToString());

            return new Technique
            {
                Id = "custom",
                Name = "Custom Rhythm",
                Description = desc.ToString(),
                Rhythm = string.Join(" — ", parts),
                ColorId = "custom",
                TotalCycleDuration = inhale + hold + exhale + hold2,
                Phases = phases,
            };
        }

        /// <summary>Block type earned for completing cycle number <paramref name="cycleNum"/>.</summary>
        public static string DetermineBlockType(int cycleNum)
        {
            if (cycleNum > 0 && cycleNum % 10 == 0) return "focus";
            if (cycleNum > 0 && cycleNum % 5 == 0) return "calm";
            return "perfect";
        }
    }
}
