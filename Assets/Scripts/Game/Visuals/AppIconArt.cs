using System;
using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// The Stackville app icon: one big 3D block with eyebrows and a wide, toothy grin on a clean gradient.
    /// Drawn in code like every other sprite (Stackville > Generate App Icon).
    /// </summary>
    public static class AppIconArt
    {
        private static readonly Color BlockColor = ColorUtil.Hex("#FF8A2A");
        private static readonly Color Ink = ColorUtil.Hex("#3A1F3D");

        public static PixelCanvas Draw(int size = 1024)
        {
            var c = new PixelCanvas(size, size);
            float s = size / 1024f;

            // Clean background: a soft diagonal gradient, so the block is the only thing in the icon.
            var top = ColorUtil.Hex("#6C7BFF");
            var low = ColorUtil.Hex("#7FD8FF");
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    c.Set(x, y, Color.Lerp(low, top, Mathf.Clamp01((y + (size - x) * 0.3f) / (size * 1.3f))));

            Block(c, s);
            return c;
        }

        // ------------------------------------------------------------------ palm tree

        private static void Palm(PixelCanvas c, float s)
        {
            // Curved trunk from the dune up to the crown, drawn as overlapping discs with ring marks.
            var p0 = new Vector2(770 * s, 200 * s);
            var p1 = new Vector2(860 * s, 470 * s);
            var p2 = new Vector2(800 * s, 760 * s);
            var trunk = new List<(Vector2 p, float r)>();
            for (int i = 0; i <= 40; i++)
            {
                float t = i / 40f;
                trunk.Add((Bezier(p0, p1, p2, t), Mathf.Lerp(34f, 22f, t) * s));
            }
            c.Paint((x, y) => MinDisc(trunk, x, y), (x, y, d) =>
            {
                // Rings along the trunk
                float band = Mathf.Repeat((y - 200 * s) / (46 * s), 1f);
                var baseCol = ColorUtil.Hex("#C98552");
                return band < 0.18f ? Color.Lerp(baseCol, Color.black, 0.18f) : Color.Lerp(baseCol, Color.white, 0.08f);
            }, 1.5f * s);

            // Fronds: curved leaves fanning out from the crown.
            var crown = p2 + new Vector2(0, 6 * s);
            var tips = new[]
            {
                new Vector2(560, 790), new Vector2(620, 900), new Vector2(790, 960),
                new Vector2(950, 890), new Vector2(985, 760), new Vector2(950, 650), new Vector2(630, 665),
            };
            var leaves = new List<List<(Vector2 p, float r)>>();
            foreach (var tip in tips)
            {
                var end = tip * s;
                var mid = (crown + end) / 2f + new Vector2(0, 70 * s); // arch upwards
                var leaf = new List<(Vector2, float)>();
                for (int i = 0; i <= 14; i++)
                {
                    float t = i / 14f;
                    leaf.Add((Bezier(crown, mid, end, t), Mathf.Sin(Mathf.PI * (0.08f + t * 0.88f)) * 40 * s + 4 * s));
                }
                leaves.Add(leaf);
            }
            c.Paint((x, y) =>
            {
                float d = float.MaxValue;
                foreach (var l in leaves) d = Mathf.Min(d, MinDisc(l, x, y));
                return d;
            }, (x, y, d) => Color.Lerp(ColorUtil.Hex("#1FA971"), ColorUtil.Hex("#4FDB8F"), Mathf.Clamp01((y - 600 * s) / (360 * s))), 1.5f * s);

            // Coconuts
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Circle(x, y, crown.x - 26 * s, crown.y - 22 * s, 22 * s),
                PixelCanvas.Circle(x, y, crown.x + 18 * s, crown.y - 30 * s, 22 * s)), ColorUtil.Hex("#8A5A34"), 1.5f * s);
        }

        // ------------------------------------------------------------------ the grinning 3D block

        private static void Block(PixelCanvas c, float s)
        {
            float half = 250 * s, radius = 76 * s;
            var front = new Vector2(474 * s, 486 * s);
            var depth = new Vector2(70 * s, 58 * s); // extrusion towards the upper right

            // Soft shadow on the sand
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, front.x + 40 * s, front.y - half - 10 * s, half * 1.2f, 34 * s),
                new Color(0.12f, 0.12f, 0.4f, 0.28f), 22 * s);

            // Extruded sides: stacked copies from back to front, darker at the back.
            int steps = Mathf.CeilToInt(depth.magnitude / (2f * s));
            for (int i = steps; i >= 1; i--)
            {
                float k = i / (float)steps;
                var o = front + depth * k;
                var side = Color.Lerp(Color.Lerp(BlockColor, Color.black, 0.28f), Color.Lerp(BlockColor, Color.black, 0.12f), 1f - k);
                c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, o.x, o.y, half, half, radius),
                    (x, y, d) => Color.Lerp(side, Color.Lerp(BlockColor, Color.white, 0.25f), Mathf.Clamp01((y - o.y - half * 0.6f) / (half * 0.5f)) * 0.6f), 1.5f * s);
            }

            // Front face, lit from the top left
            var light = Color.Lerp(BlockColor, Color.white, 0.22f);
            var dark = Color.Lerp(BlockColor, Color.black, 0.04f);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, front.x, front.y, half, half, radius),
                (x, y, d) => Color.Lerp(dark, light, Mathf.Clamp01(((y - front.y) + (front.x - x) * 0.4f) / (2.4f * half) + 0.5f)), 1.5f * s);
            // Gloss along the top edge
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, front.x - 30 * s, front.y + half * 0.72f, half * 0.7f, 24 * s, 24 * s),
                new Color(1f, 1f, 1f, 0.28f), 6 * s);

            // Eyes: white with big pupils looking slightly up, plus shine dots
            float ex = 95 * s, ey = front.y + 70 * s;
            foreach (int side in new[] { -1, 1 })
            {
                float cx = front.x + side * ex;
                c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx, ey, 44 * s, 54 * s), Color.white, 1.5f * s);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, cx + 6 * s, ey + 8 * s, 28 * s), Ink, 1.5f * s);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, cx + 16 * s, ey + 20 * s, 9 * s), Color.white, 1.5f * s);
            }

            // Eyebrows: thick rounded bars, raised a little on the outside for a cheeky look.
            foreach (int side in new[] { -1, 1 })
            {
                float cx = front.x + side * ex;
                float y1 = ey + 92 * s, y2 = ey + 106 * s;
                float ax = cx - side * 50 * s, bx = cx + side * 50 * s;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, ax, y1, bx, y2, 36 * s), Ink, 1.5f * s);
            }

            // Big toothy grin: a crescent between two upward curves, filled with two rows of teeth.
            float mx = front.x, my = front.y - 50 * s, mrx = 170 * s;
            Func<float, float> topEdge = x => { float u = (x - mx) / mrx; return my - 2 * s + 34 * s * u * u; };
            Func<float, float> bottomEdge = x => { float u = (x - mx) / mrx; return my - 112 * s + 140 * s * u * u; };
            Func<float, float, float> grin = (x, y) => Mathf.Max(Mathf.Max(bottomEdge(x) - y, y - topEdge(x)), Mathf.Abs(x - mx) - mrx);
            // Dark lips/outline, then the teeth
            c.Paint((x, y) => grin(x, y) - 9 * s, Ink, 1.5f * s);
            c.Paint((x, y) => grin(x, y), (x, y, d) => Color.Lerp(ColorUtil.Hex("#E4E4F0"), Color.white, Mathf.Clamp01(-d / (14 * s))), 1.5f * s);
            // Line between the upper and lower teeth
            c.Paint((x, y) => Mathf.Max(Mathf.Abs(y - (topEdge(x) * 0.55f + bottomEdge(x) * 0.45f)) - 3 * s, grin(x, y)),
                ColorUtil.Hex("#B9B9CC"), 1.2f * s);
            // Gaps between teeth
            c.Paint((x, y) =>
            {
                float fromGap = Mathf.Abs(Mathf.Repeat(x - mx + 32 * s, 64 * s) - 32 * s);
                return Mathf.Max(fromGap - 2.6f * s, grin(x, y) + 2 * s);
            }, ColorUtil.Hex("#D3D3E2"), 1.2f * s);
            // Smile dimples at the corners
            foreach (int side in new[] { -1, 1 })
            {
                float cx0 = mx + side * (mrx + 14 * s), cy0 = my + 34 * s;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, cx0 - side * 4 * s, cy0 - 18 * s, cx0 + side * 8 * s, cy0 + 14 * s, 6 * s), Ink, 1.5f * s);
            }

            // Rosy cheeks
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Ellipse(x, y, front.x - 175 * s, front.y + 4 * s, 34 * s, 22 * s),
                PixelCanvas.Ellipse(x, y, front.x + 175 * s, front.y + 4 * s, 34 * s, 22 * s)), new Color(1f, 0.85f, 0.88f, 0.55f), 6 * s);
        }

        // ------------------------------------------------------------------ helpers

        private static Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float u = 1f - t;
            return a * (u * u) + b * (2f * u * t) + c * (t * t);
        }

        private static float MinDisc(List<(Vector2 p, float r)> discs, float x, float y)
        {
            float d = float.MaxValue;
            foreach (var (p, r) in discs)
            {
                float dx = x - p.x, dy = y - p.y;
                d = Mathf.Min(d, Mathf.Sqrt(dx * dx + dy * dy) - r);
            }
            return d;
        }
    }
}
