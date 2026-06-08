using System.Collections.Generic;
using UnityEngine;

namespace BreathTower.UI
{
    /// <summary>
    /// Builds and caches gradient textures at runtime so we can reproduce the
    /// reference app's LinearGradient backgrounds/buttons without shipping any
    /// image assets or touching the Resources folder.
    /// </summary>
    public static class Gradients
    {
        private static readonly Dictionary<int, Texture2D> Cache = new Dictionary<int, Texture2D>();

        /// <summary>Vertical gradient (top → bottom).</summary>
        public static Texture2D Vertical(Color top, Color bottom)
        {
            int key = Hash(top, bottom, 0);
            if (Cache.TryGetValue(key, out var cached)) return cached;

            const int h = 64;
            var tex = new Texture2D(1, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            for (int y = 0; y < h; y++)
            {
                float t = y / (float)(h - 1);
                // Texture row 0 is the bottom in Unity, so lerp accordingly.
                tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
            }
            tex.Apply();
            Cache[key] = tex;
            return tex;
        }

        /// <summary>Horizontal gradient (left → right).</summary>
        public static Texture2D Horizontal(Color left, Color right)
        {
            int key = Hash(left, right, 1);
            if (Cache.TryGetValue(key, out var cached)) return cached;

            const int w = 64;
            var tex = new Texture2D(w, 1, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            for (int x = 0; x < w; x++)
            {
                float t = x / (float)(w - 1);
                tex.SetPixel(x, 0, Color.Lerp(left, right, t));
            }
            tex.Apply();
            Cache[key] = tex;
            return tex;
        }

        private static int Hash(Color a, Color b, int dir)
        {
            unchecked
            {
                int h = dir;
                h = h * 397 ^ a.GetHashCode();
                h = h * 397 ^ b.GetHashCode();
                return h;
            }
        }
    }
}
