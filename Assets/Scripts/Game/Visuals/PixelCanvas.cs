using System;
using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// Tiny software rasteriser used to draw every sprite in the game at startup,
    /// so the project needs no image assets. Shapes are drawn with signed distance
    /// functions, which gives smooth anti-aliased edges.
    /// Coordinates: pixels, origin bottom-left, y up.
    /// </summary>
    public sealed class PixelCanvas
    {
        /// <summary>Editor tools set this to keep textures CPU-readable (e.g. to export the app icon).</summary>
        public static bool KeepReadable;

        public readonly int Width;
        public readonly int Height;
        private readonly Color[] _px;

        public PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _px = new Color[width * height];
        }

        public Color Get(int x, int y) => _px[y * Width + x];

        public void Set(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            _px[y * Width + x] = c;
        }

        /// <summary>Alpha-blends <paramref name="c"/> over the pixel with extra coverage factor.</summary>
        public void Blend(int x, int y, Color c, float coverage = 1f)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            float a = c.a * Mathf.Clamp01(coverage);
            if (a <= 0f) return;
            int i = y * Width + x;
            var dst = _px[i];
            float outA = a + dst.a * (1f - a);
            if (outA <= 0f) { _px[i] = new Color(0, 0, 0, 0); return; }
            float r = (c.r * a + dst.r * dst.a * (1f - a)) / outA;
            float g = (c.g * a + dst.g * dst.a * (1f - a)) / outA;
            float b = (c.b * a + dst.b * dst.a * (1f - a)) / outA;
            _px[i] = new Color(r, g, b, outA);
        }

        /// <summary>
        /// Paints a shape given by a signed distance function (negative inside).
        /// <paramref name="shade"/> chooses the colour per pixel; it receives (x, y, distance).
        /// </summary>
        public void Paint(Func<float, float, float> sdf, Func<float, float, float, Color> shade, float softness = 1f)
        {
            for (int y = 0; y < Height; y++)
            {
                float py = y + 0.5f;
                for (int x = 0; x < Width; x++)
                {
                    float px = x + 0.5f;
                    float d = sdf(px, py);
                    float coverage = Mathf.Clamp01(0.5f - d / softness);
                    if (coverage <= 0f) continue;
                    Blend(x, y, shade(px, py, d), coverage);
                }
            }
        }

        public void Paint(Func<float, float, float> sdf, Color color, float softness = 1f) =>
            Paint(sdf, (x, y, d) => color, softness);

        // ------------------------------------------------------------------ SDF helpers

        public static float Circle(float px, float py, float cx, float cy, float r)
        {
            float dx = px - cx, dy = py - cy;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        public static float Ellipse(float px, float py, float cx, float cy, float rx, float ry)
        {
            // Approximation good enough for highlights and eyes.
            float dx = (px - cx) / rx, dy = (py - cy) / ry;
            float k = Mathf.Sqrt(dx * dx + dy * dy);
            return (k - 1f) * Mathf.Min(rx, ry);
        }

        public static float RoundedBox(float px, float py, float cx, float cy, float halfW, float halfH, float radius)
        {
            float qx = Mathf.Abs(px - cx) - (halfW - radius);
            float qy = Mathf.Abs(py - cy) - (halfH - radius);
            float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
        }

        /// <summary>Distance to a line segment of the given thickness (a capsule).</summary>
        public static float Segment(float px, float py, float ax, float ay, float bx, float by, float thickness)
        {
            float pax = px - ax, pay = py - ay, bax = bx - ax, bay = by - ay;
            float h = Mathf.Clamp01((pax * bax + pay * bay) / (bax * bax + bay * bay + 1e-6f));
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - thickness * 0.5f;
        }

        /// <summary>Signed distance to a polygon (even-odd rule), vertices in order.</summary>
        public static float Polygon(float px, float py, IList<Vector2> v)
        {
            float d = float.MaxValue;
            bool inside = false;
            for (int i = 0, j = v.Count - 1; i < v.Count; j = i++)
            {
                var a = v[j];
                var b = v[i];
                float ex = b.x - a.x, ey = b.y - a.y;
                float wx = px - a.x, wy = py - a.y;
                float h = Mathf.Clamp01((wx * ex + wy * ey) / (ex * ex + ey * ey + 1e-6f));
                float dx = wx - ex * h, dy = wy - ey * h;
                d = Mathf.Min(d, dx * dx + dy * dy);
                bool c1 = py >= a.y, c2 = py < b.y, c3 = ex * wy > ey * wx;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) inside = !inside;
            }
            float dist = Mathf.Sqrt(d);
            return inside ? -dist : dist;
        }

        public static List<Vector2> StarPoints(float cx, float cy, float outer, float inner, int points, float rotationDeg = 90f)
        {
            var list = new List<Vector2>(points * 2);
            for (int i = 0; i < points * 2; i++)
            {
                float r = i % 2 == 0 ? outer : inner;
                float a = (rotationDeg + i * 180f / points) * Mathf.Deg2Rad;
                list.Add(new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r));
            }
            return list;
        }

        // ------------------------------------------------------------------ output

        public Texture2D ToTexture(FilterMode filter = FilterMode.Bilinear)
        {
            var tex = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = filter;
            tex.SetPixels(_px);
            tex.Apply(false, !KeepReadable);
            return tex;
        }

        /// <summary>Creates a sprite. A non-zero border makes it 9-sliceable for UI panels.</summary>
        public Sprite ToSprite(float pixelsPerUnit, Vector4 border = default, FilterMode filter = FilterMode.Bilinear)
        {
            var tex = ToTexture(filter);
            return Sprite.Create(tex, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit, 0, SpriteMeshType.FullRect, border);
        }

        // ------------------------------------------------------------------ colour helpers

        public static Color Lighten(Color c, float amount) => Color.Lerp(c, Color.white, amount);
        public static Color Darken(Color c, float amount) => Color.Lerp(c, Color.black, amount);
        public static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }

    public static class ColorUtil
    {
        public static Color Hex(string hex, float alpha = 1f)
        {
            if (string.IsNullOrEmpty(hex)) return Color.magenta;
            if (hex[0] == '#') hex = hex.Substring(1);
            if (hex.Length < 6) return Color.magenta;
            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);
            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }

        // ---- Soft arcade palette: bright candy accents on airy, light surfaces.
        public static readonly Color Pink = Hex("#FF6F91");
        public static readonly Color Purple = Hex("#8E7CFF");
        public static readonly Color Blue = Hex("#3FB6FF");
        public static readonly Color Mint = Hex("#2ED3A0");
        public static readonly Color Lime = Hex("#9BDB4D");
        public static readonly Color Yellow = Hex("#FFC93C");
        public static readonly Color Orange = Hex("#FF9F43");
        public static readonly Color Red = Hex("#FF5A6E");
        public static readonly Color Gold = Hex("#FFC93C");

        /// <summary>Deep ink navy: dimmed backdrops and dark text on bright chips.</summary>
        public static readonly Color Night = Hex("#2B2D5C");
        /// <summary>Card / popup background.</summary>
        public static readonly Color Panel = Hex("#FFFFFF");
        /// <summary>Tinted card used inside panels (shop items, rows).</summary>
        public static readonly Color PanelLight = Hex("#F3F5FF");
        /// <summary>Body text.</summary>
        public static readonly Color Ink = Hex("#2B2D5C");
        /// <summary>Secondary / hint text.</summary>
        public static readonly Color InkDim = Hex("#7C82B3");
        /// <summary>Dark colour for faces and details drawn inside sprites (eyes, clock hands).</summary>
        public static readonly Color FaceInk = Hex("#2A1440");
        /// <summary>Junk rows from harder difficulties.</summary>
        public static readonly Color Junk = Hex("#B8BDD6");
        /// <summary>Grass for the city.</summary>
        public static readonly Color Grass = Hex("#9BE29A");

        /// <summary>Kept for older code: popups and cards use the panel colour.</summary>
        public static readonly Color Cream = Panel;

        public static readonly Color[] Rainbow =
        {
            Hex("#FF5A6E"), Hex("#FF9F43"), Hex("#FFC93C"), Hex("#2ED3A0"), Hex("#3FB6FF"), Hex("#8E7CFF"), Hex("#FF6F91")
        };
    }
}
