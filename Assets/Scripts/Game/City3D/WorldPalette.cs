using UnityEngine;
using JellyDrop.Visuals;

namespace JellyDrop.City3D
{
    /// <summary>
    /// Colour grading and lighting for the 3D town and the islands. Every mesh colour goes through
    /// <see cref="Grade"/> when the mesh is built, so the hand-picked palette stays readable in code while the
    /// worlds render like the Arcade board: rich, saturated colours with deep shadows and warm light. Mid tones
    /// sink (contrast), greens get deeper and lusher, and everything that isn't water or sky leans warm.
    /// </summary>
    public static class WorldPalette
    {
        /// <summary>Overall brightness of graded colours (1 = unchanged).</summary>
        public const float Gain = 0.92f;
        /// <summary>Above 1 darkens mid tones more than highlights (more contrast).</summary>
        public const float Gamma = 1.45f;
        /// <summary>Keeps the darkest colours from going pitch black.</summary>
        public const float Lift = 0.025f;
        /// <summary>Saturation boost for coloured surfaces (greys stay grey).</summary>
        public const float Saturation = 1.2f;

        public static readonly Color Ambient = ColorUtil.Hex("#4A4C78");
        public static readonly Color SunColor = ColorUtil.Hex("#FFD49A");
        public const float SunIntensity = 1.38f;
        public const float ShadowStrength = 0.82f;

        public static readonly Color CitySky = ColorUtil.Hex("#1E2554");
        public static readonly Color ForestSky = ColorUtil.Hex("#172F45");
        public static readonly Color GardenSky = ColorUtil.Hex("#22406A");

        public static Color Grade(Color c)
        {
            ToHsv(c, out float h, out float s, out float v);
            // Vivid: boost colour, leave near-greys alone (stone, paths, metal).
            bool blue = h > 0.47f && h < 0.75f;        // water, sky-ish, glass: keep them cool and calm
            bool green = h > 0.2f && h <= 0.47f;
            if (s > 0.08f && !blue) s = Mathf.Min(0.9f, s * Saturation + 0.04f);
            v = Lift + Gain * Mathf.Pow(v, Gamma);
            if (blue) v *= 0.88f;
            if (green)
            {
                // Lush greens: a touch towards yellow-green, a little more saturated, a little deeper.
                // Big green areas (grass, trees) go deep, so buildings and paths pop on top of them.
                h = Mathf.Lerp(h, 0.3f, 0.25f);
                s = Mathf.Min(0.85f, s * 1.05f);
                v *= 0.8f;
            }
            var o = FromHsv(h, s, Mathf.Clamp01(v));
            if (!blue)
            {
                // Warm light on everything else (wood, roofs, flowers, sand).
                o.r = Mathf.Clamp01(o.r * 1.06f);
                o.b = Mathf.Clamp01(o.b * 0.88f);
            }
            o.a = c.a;
            return o;
        }

        private static void ToHsv(Color c, out float h, out float s, out float v)
        {
            float max = Mathf.Max(c.r, Mathf.Max(c.g, c.b)), min = Mathf.Min(c.r, Mathf.Min(c.g, c.b));
            float d = max - min;
            v = max;
            s = max <= 0f ? 0f : d / max;
            if (d <= 1e-5f) { h = 0f; return; }
            if (max == c.r) h = (c.g - c.b) / d;
            else if (max == c.g) h = 2f + (c.b - c.r) / d;
            else h = 4f + (c.r - c.g) / d;
            h /= 6f;
            if (h < 0f) h += 1f;
        }

        private static Color FromHsv(float h, float s, float v)
        {
            float h6 = h * 6f;
            int i = (int)Mathf.Floor(h6) % 6;
            float f = h6 - Mathf.Floor(h6);
            float p = v * (1f - s), q = v * (1f - s * f), t = v * (1f - s * (1f - f));
            switch (i)
            {
                case 0: return new Color(v, t, p, 1f);
                case 1: return new Color(q, v, p, 1f);
                case 2: return new Color(p, v, t, 1f);
                case 3: return new Color(p, q, v, 1f);
                case 4: return new Color(t, p, v, 1f);
                default: return new Color(v, p, q, 1f);
            }
        }

        /// <summary>Applies the shared lighting to a world's sun (call when building the world).</summary>
        public static void SetupSun(Light sun)
        {
            sun.color = SunColor;
            sun.intensity = SunIntensity;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = ShadowStrength;
        }
    }
}
