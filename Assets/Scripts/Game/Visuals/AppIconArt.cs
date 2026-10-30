using System;
using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// The Stackville app icon: one big 3D block with a wide, toothy grin, a palm tree and the sun behind it.
    /// Drawn in code like every other sprite (Stackville > Generate App Icon).
    /// </summary>
    public static class AppIconArt
    {
        private static readonly Color BlockColor = ColorUtil.Hex("#FF6F91");
        private static readonly Color Ink = ColorUtil.Hex("#3A1F3D");

        public static PixelCanvas Draw(int size = 1024)
        {
            var c = new PixelCanvas(size, size);
            float s = size / 1024f;

            // Sky: deeper blue at the top, almost white near the horizon.
            var top = ColorUtil.Hex("#3FB6FF");
            var low = ColorUtil.Hex("#C9F3FF");
            for (int y = 0; y < size; y++)
            {
                var col = Color.Lerp(low, top, Mathf.Pow(y / (float)size, 0.9f));
                for (int x = 0; x < size; x++) c.Set(x, y, col);
            }

            // Sun with a warm halo (top left)
            float sunX = 250 * s, sunY = 790 * s;
            c.Paint((x, y) => PixelCanvas.Circle(x, y, sunX, sunY, 230 * s),
                (x, y, d) => new Color(1f, 0.95f, 0.7f, Mathf.Clamp01(-d / (230 * s)) * 0.6f), 2f);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, sunX, sunY, 118 * s),
                (x, y, d) => Color.Lerp(ColorUtil.Hex("#FFB938"), ColorUtil.Hex("#FFE27A"), Mathf.Clamp01((y - sunY + 118 * s) / (236 * s))), 1.5f * s);

            Palm(c, s);

            // Sand dune
            float dune = 235 * s;
            c.Paint((x, y) => y - (dune + Mathf.Sin((x / size) * Mathf.PI * 1.1f + 0.2f) * 45 * s),
                (x, y, d) => Color.Lerp(ColorUtil.Hex("#F6C77D"), ColorUtil.Hex("#FFE3A8"), Mathf.Clamp01(y / (dune + 45 * s))), 1.5f * s);

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
            float half = 230 * s, radius = 70 * s;
            var front = new Vector2(450 * s, 450 * s);
            var depth = new Vector2(70 * s, 58 * s); // extrusion towards the upper right

            // Soft shadow on the sand
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, front.x + 40 * s, front.y - half - 10 * s, half * 1.2f, 34 * s),
                new Color(0.55f, 0.35f, 0.15f, 0.3f), 20 * s);

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

            // Wide open grin: a D-shaped mouth with a row of teeth on top and a tongue at the bottom.
            float mx = front.x, my = front.y - 30 * s, mrx = 150 * s, mry = 125 * s;
            Func<float, float, float> mouth = (x, y) => Mathf.Max(PixelCanvas.Ellipse(x, y, mx, my, mrx, mry), y - my);
            c.Paint((x, y) => mouth(x, y), Ink, 1.5f * s);
            c.Paint((x, y) => Mathf.Max(mouth(x, y) + 4 * s, PixelCanvas.Circle(x, y, mx, my - mry - 10 * s, 84 * s)), ColorUtil.Hex("#FF8FB0"), 1.5f * s);
            // Teeth: a white band under the top edge, split into teeth by thin gaps
            c.Paint((x, y) =>
            {
                float band = Mathf.Max(mouth(x, y) + 8 * s, (my - 46 * s) - y);
                // Thin gaps every 50 px split the band into teeth.
                float fromGap = Mathf.Abs(Mathf.Repeat(x - mx + 25 * s, 50 * s) - 25 * s);
                return Mathf.Max(band, 3.5f * s - fromGap);
            }, Color.white, 1.5f * s);

            // Rosy cheeks
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Ellipse(x, y, front.x - 175 * s, front.y - 5 * s, 34 * s, 22 * s),
                PixelCanvas.Ellipse(x, y, front.x + 175 * s, front.y - 5 * s, 34 * s, 22 * s)), new Color(1f, 0.85f, 0.88f, 0.55f), 6 * s);
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
