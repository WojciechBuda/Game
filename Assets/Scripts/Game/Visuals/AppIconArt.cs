using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// The Stackville app icon: minimal and friendly. Three big rounded blocks form a tiny skyline (one with a
    /// roof, the tallest with a smile), and one more block drops in. Drawn in code (Stackville > Generate App Icon).
    /// </summary>
    public static class AppIconArt
    {
        private static readonly Color Mint = ColorUtil.Hex("#3DDBA8");
        private static readonly Color Yellow = ColorUtil.Hex("#FFC93C");
        private static readonly Color Pink = ColorUtil.Hex("#FF7A9A");
        private static readonly Color Face = ColorUtil.Hex("#3A2E5C");

        public static PixelCanvas Draw(int size = 1024)
        {
            var c = new PixelCanvas(size, size);
            float s = size / 1024f;

            // Background: one calm diagonal gradient, violet to soft blue.
            var a = ColorUtil.Hex("#7C6CFF");
            var b = ColorUtil.Hex("#5AA8FF");
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    c.Set(x, y, Color.Lerp(b, a, Mathf.Clamp01((x * 0.35f + y) / (size * 1.35f))));

            float cell = 200 * s, gap = 16 * s, radius = 52 * s;
            float x0 = (size - (3 * cell + 2 * gap)) / 2f;
            float y0 = 210 * s;
            Vector2 At(float col, float row) => new Vector2(x0 + col * (cell + gap), y0 + row * (cell + gap));

            // Soft shadow on the ground
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, size / 2f, y0 - 18 * s, 1.75f * cell, 26 * s),
                new Color(0.1f, 0.12f, 0.35f, 0.25f), 18 * s);

            // Left: mint house with a roof
            Block(c, At(0, 0), cell, radius, s, Mint);
            Block(c, At(0, 1), cell, radius, s, Mint);
            Roof(c, At(0, 2) - new Vector2(0, 8 * s), cell, s, Color.Lerp(Mint, Color.white, 0.25f));

            // Middle: tall yellow tower with a happy face on top
            Block(c, At(1, 0), cell, radius, s, Yellow);
            Block(c, At(1, 1), cell, radius, s, Yellow);
            Block(c, At(1, 2), cell, radius, s, Yellow);
            Smile(c, At(1, 2), cell, s);

            // Right: pink block, and another one dropping onto it
            Block(c, At(2, 0), cell, radius, s, Pink);
            var falling = At(2, 1) + new Vector2(0, 70 * s);
            Block(c, falling, cell, radius, s, Pink);
            for (int i = -1; i <= 1; i += 2)
            {
                float lx = falling.x + cell / 2f + i * 46 * s, ly = falling.y + cell + 26 * s;
                float len = 70 * s;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, lx, ly, lx, ly + len, 10 * s),
                    (x, y, d) => new Color(1f, 1f, 1f, 0.7f * Mathf.Clamp01(1f - (y - ly) / len)), 1.5f * s);
            }

            return c;
        }

        /// <summary>A big rounded block with a gentle top-down light.</summary>
        private static void Block(PixelCanvas c, Vector2 pos, float cell, float radius, float s, Color color)
        {
            float cx = pos.x + cell / 2f, cy = pos.y + cell / 2f, half = cell / 2f;
            var light = Color.Lerp(color, Color.white, 0.18f);
            var dark = Color.Lerp(color, Color.black, 0.06f);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, half, half, radius),
                (x, y, d) => Color.Lerp(dark, light, Mathf.Clamp01((y - (cy - half)) / cell)), 1.5f * s);
        }

        private static void Roof(PixelCanvas c, Vector2 pos, float cell, float s, Color color)
        {
            var pts = new List<Vector2>
            {
                new Vector2(pos.x + 6 * s, pos.y - 4 * s),
                new Vector2(pos.x + cell / 2f, pos.y + cell * 0.62f),
                new Vector2(pos.x + cell - 6 * s, pos.y - 4 * s),
            };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts), color, 1.5f * s);
        }

        private static void Smile(PixelCanvas c, Vector2 pos, float cell, float s)
        {
            float cx = pos.x + cell / 2f, cy = pos.y + cell / 2f;
            float eyeY = cy + 18 * s, eyeDx = 40 * s, eyeR = 15 * s;
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Circle(x, y, cx - eyeDx, eyeY, eyeR), PixelCanvas.Circle(x, y, cx + eyeDx, eyeY, eyeR)), Face, 1.5f * s);
            // Smile: the lower half of a ring
            float r = 38 * s, t = 9 * s, my = cy - 2 * s;
            c.Paint((x, y) =>
            {
                float ring = Mathf.Abs(Mathf.Sqrt((x - cx) * (x - cx) + (y - my) * (y - my)) - r) - t;
                float below = y - (my - 6 * s);
                return Mathf.Max(ring, below);
            }, Face, 1.5f * s);
            // Rosy cheeks
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Circle(x, y, cx - 66 * s, cy - 14 * s, 14 * s), PixelCanvas.Circle(x, y, cx + 66 * s, cy - 14 * s, 14 * s)),
                new Color(1f, 0.45f, 0.5f, 0.45f), 4 * s);
        }
    }
}
