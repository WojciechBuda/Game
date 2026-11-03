using UnityEngine;
using JellyDrop.Visuals;

namespace JellyDrop.City3D
{
    /// <summary>
    /// Colour grading and lighting for the 3D town and the islands. Every mesh colour goes through
    /// <see cref="Grade"/> when the mesh is built, so the hand-picked palette stays readable in code while
    /// the worlds render deeper and with more contrast: pale, large surfaces (grass, sand, roofs) are muted and
    /// darkened, mid tones sink, and strong accents (mushrooms, flowers, coins) keep most of their punch.
    /// </summary>
    public static class WorldPalette
    {
        /// <summary>Overall brightness of graded colours (1 = unchanged).</summary>
        public const float Gain = 0.86f;
        /// <summary>Above 1 darkens mid tones more than highlights (more contrast).</summary>
        public const float Gamma = 1.65f;
        /// <summary>Keeps the darkest colours from going pitch black.</summary>
        public const float Lift = 0.035f;

        public static readonly Color Ambient = ColorUtil.Hex("#5E6390");
        public static readonly Color SunColor = ColorUtil.Hex("#FFE4BE");
        public const float SunIntensity = 1.22f;
        public const float ShadowStrength = 0.72f;

        public static readonly Color CitySky = ColorUtil.Hex("#2C3768");
        public static readonly Color ForestSky = ColorUtil.Hex("#22415F");
        public static readonly Color GardenSky = ColorUtil.Hex("#2F5684");

        public static Color Grade(Color c)
        {
            ToHsv(c, out float h, out float s, out float v);
            // Mute soft colours more than vivid ones, so accents still stand out against the ground.
            s *= 0.84f + 0.16f * s;
            v = Lift + Gain * Mathf.Pow(v, Gamma);
            // Neon-bright colours (pond water, pastel flowers) come down a little too.
            v *= 1f - 0.12f * s;
            var o = FromHsv(h, Mathf.Clamp01(s), Mathf.Clamp01(v));
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
