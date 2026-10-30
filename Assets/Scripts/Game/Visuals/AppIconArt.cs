using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// The Stackville app icon: a little town whose buildings are stacked falling-block pieces, with one more
    /// piece dropping in from the sky. Drawn in code like every other sprite (see Stackville > Generate App Icon).
    /// </summary>
    public static class AppIconArt
    {
        private static readonly Color Orange = ColorUtil.Hex("#FF9F43");
        private static readonly Color Blue = ColorUtil.Hex("#3FB6FF");
        private static readonly Color Pink = ColorUtil.Hex("#FF6F91");
        private static readonly Color Purple = ColorUtil.Hex("#8E7CFF");
        private static readonly Color Mint = ColorUtil.Hex("#2ED3A0");
        private static readonly Color Yellow = ColorUtil.Hex("#FFC93C");
        private static readonly Color Window = ColorUtil.Hex("#FFF4C7");

        public static PixelCanvas Draw(int size = 1024)
        {
            var c = new PixelCanvas(size, size);
            float s = size / 1024f;

            // Sky: violet at the top fading to a warm pink near the horizon.
            var top = ColorUtil.Hex("#5B5BF0");
            var mid = ColorUtil.Hex("#A98BFF");
            var low = ColorUtil.Hex("#FFC2D9");
            for (int y = 0; y < size; y++)
            {
                float t = y / (float)size;
                var col = t > 0.45f ? Color.Lerp(mid, top, (t - 0.45f) / 0.55f) : Color.Lerp(low, mid, t / 0.45f);
                for (int x = 0; x < size; x++) c.Set(x, y, col);
            }

            // Sun with a soft glow behind the town.
            float sx = 735 * s, sy = 560 * s;
            c.Paint((x, y) => PixelCanvas.Circle(x, y, sx, sy, 250 * s),
                (x, y, d) => new Color(1f, 0.93f, 0.6f, Mathf.Clamp01(-d / (250 * s)) * 0.55f), 2f);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, sx, sy, 125 * s), ColorUtil.Hex("#FFE27A"), 1.5f * s);

            // Soft sparkles in the sky.
            Sparkle(c, 170 * s, 860 * s, 26 * s);
            Sparkle(c, 880 * s, 880 * s, 18 * s);
            Sparkle(c, 300 * s, 700 * s, 12 * s);

            // Rolling green hill as the ground.
            float ground = 190 * s;
            c.Paint((x, y) => y - (ground + Mathf.Sin(x / size * Mathf.PI) * 40 * s),
                (x, y, d) => Color.Lerp(ColorUtil.Hex("#3CC98B"), ColorUtil.Hex("#7FE8A8"), Mathf.Clamp01(y / (ground + 40 * s))), 1.5f * s);

            // Town grid: buildings are falling-block pieces made of glossy cells.
            float cell = 104 * s;
            float gx = (size - 7 * cell) / 2f;
            float gy = ground + 14 * s;
            Vector2 Cell(float col, float row) => new Vector2(gx + col * cell, gy + row * cell);

            // Shadows under the buildings
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, size / 2f, gy - 6 * s, 3.9f * cell, 22 * s),
                new Color(0.1f, 0.3f, 0.2f, 0.22f), 12 * s);

            // L building (orange) with two roofs
            Building(c, Cell, cell, s, Orange, new[] { (0, 0), (1, 0), (0, 1), (0, 2) });
            Roof(c, Cell(0, 3), cell, s, Orange);
            Roof(c, Cell(1, 1), cell, s, Orange);

            // Little mint house
            Building(c, Cell, cell, s, Mint, new[] { (2, 0) });
            Roof(c, Cell(2, 1), cell, s, Mint);

            // Tall blue I tower with an antenna
            Building(c, Cell, cell, s, Blue, new[] { (3, 0), (3, 1), (3, 2), (3, 3) });
            var ant = Cell(3.5f, 4);
            c.Paint((x, y) => PixelCanvas.Segment(x, y, ant.x, ant.y, ant.x, ant.y + 70 * s, 7 * s), ColorUtil.Hex("#2A7FC2"), 1.5f * s);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, ant.x, ant.y + 76 * s, 14 * s), Pink, 1.5f * s);

            // Pink O house with one wide roof, and a purple J resting on it
            Building(c, Cell, cell, s, Pink, new[] { (4, 0), (5, 0), (4, 1), (5, 1) });
            Building(c, Cell, cell, s, Purple, new[] { (6, 0), (6, 1), (6, 2), (5, 2) });
            Roof(c, Cell(4, 2), cell, s, Pink, 1);
            Roof(c, Cell(5, 3), cell, s, Purple, 2);

            // The falling T piece with speed streaks above it
            float fall = 4.75f;
            var tCells = new[] { (1.5f, fall + 1), (2.5f, fall + 1), (3.5f, fall + 1), (2.5f, fall) };
            for (int i = 0; i < 3; i++)
            {
                var p = Cell(1.5f + i + 0.5f, fall + 2.05f);
                float len = (i == 1 ? 120 : 80) * s;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, p.x, p.y, p.x, p.y + len, 9 * s),
                    (x, y, d) => new Color(1f, 1f, 1f, 0.55f * Mathf.Clamp01(1f - (y - p.y) / len)), 1.5f * s);
            }
            foreach (var (col, row) in tCells) GlossyCell(c, Cell(col, row), cell, s, Yellow, false);

            return c;
        }

        private static void Building(PixelCanvas c, System.Func<float, float, Vector2> cellPos, float cell, float s, Color color, (int col, int row)[] cells)
        {
            foreach (var (col, row) in cells) GlossyCell(c, cellPos(col, row), cell, s, color, true);
        }

        /// <summary>One rounded, lit-from-above block. Buildings get two lit windows.</summary>
        private static void GlossyCell(PixelCanvas c, Vector2 pos, float cell, float s, Color color, bool windows)
        {
            float pad = 4 * s;
            float cx = pos.x + cell / 2f, cy = pos.y + cell / 2f;
            float half = cell / 2f - pad, r = 22 * s;
            var light = Color.Lerp(color, Color.white, 0.32f);
            var dark = Color.Lerp(color, Color.black, 0.12f);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, half, half, r),
                (x, y, d) => Color.Lerp(dark, light, Mathf.Clamp01((y - (cy - half)) / (2 * half))), 1.5f * s);
            // Top gloss
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy + half * 0.55f, half * 0.78f, half * 0.28f, 14 * s),
                new Color(1f, 1f, 1f, 0.28f), 3 * s);
            if (!windows) return;
            float w = 16 * s, h = 20 * s;
            c.Paint((x, y) => Mathf.Min(
                    PixelCanvas.RoundedBox(x, y, cx - 22 * s, cy - 6 * s, w, h, 6 * s),
                    PixelCanvas.RoundedBox(x, y, cx + 22 * s, cy - 6 * s, w, h, 6 * s)),
                Window, 1.5f * s);
        }

        /// <summary>Triangular roof sitting on top of the cell at <paramref name="pos"/> (spanning <paramref name="width"/> cells).</summary>
        private static void Roof(PixelCanvas c, Vector2 pos, float cell, float s, Color color, int width = 1)
        {
            float w = cell * width;
            var pts = new List<Vector2>
            {
                new Vector2(pos.x - 8 * s, pos.y - 6 * s),
                new Vector2(pos.x + w / 2f, pos.y + 66 * s),
                new Vector2(pos.x + w + 8 * s, pos.y - 6 * s),
            };
            var roof = Color.Lerp(color, Color.black, 0.22f);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts) - 4 * s, roof, 1.5f * s);
        }

        private static void Sparkle(PixelCanvas c, float x0, float y0, float r)
        {
            var pts = PixelCanvas.StarPoints(x0, y0, r, r * 0.28f, 4);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts), new Color(1f, 1f, 1f, 0.85f), 1.5f);
        }
    }
}
