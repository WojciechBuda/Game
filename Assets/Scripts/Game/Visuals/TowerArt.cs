using System;
using System.Collections.Generic;
using JellyDrop.Core.Tower;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// Painted art for the tower climb, one set per scenery: a tall backdrop that tiles vertically (trees,
    /// waterfalls, vines, icicles, planets, lava falls...), a wall texture, platforms and little animated
    /// critters (birds, leaves, snow, fireflies, embers). Everything is drawn in code and cached.
    /// </summary>
    public static class TowerArt
    {
        public const int BackW = 384, BackH = 768;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private static Sprite Cached(string key, Func<Sprite> make)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            s = make();
            s.name = "tower_" + key;
            Cache[key] = s;
            return s;
        }

        private static Color C(string hex, float a = 1f) => ColorUtil.Hex(hex, a);

        // ------------------------------------------------------------------ drawing helpers (wrap vertically)

        private sealed class Painter
        {
            public readonly PixelCanvas Canvas;
            public readonly int W, H;
            public readonly System.Random Rng;
            public bool Wrap = true;

            public Painter(int w, int h, int seed)
            {
                W = w; H = h;
                Canvas = new PixelCanvas(w, h);
                Rng = new System.Random(seed);
            }

            public float R(float a, float b) => a + (float)Rng.NextDouble() * (b - a);

            /// <summary>Paints a shape inside a box; if the box crosses the top or bottom it is painted again on the other side.</summary>
            public void Shape(float x0, float y0, float x1, float y1, Func<float, float, float> sdf, Func<float, float, float, Color> shade, float soft = 1f)
            {
                Canvas.PaintIn(x0, y0, x1, y1, sdf, shade, soft);
                if (!Wrap) return;
                if (y0 < 0) Canvas.PaintIn(x0, y0 + H, x1, y1 + H, (x, y) => sdf(x, y - H), (x, y, d) => shade(x, y - H, d), soft);
                if (y1 > H) Canvas.PaintIn(x0, y0 - H, x1, y1 - H, (x, y) => sdf(x, y + H), (x, y, d) => shade(x, y + H, d), soft);
            }

            public void Shape(float x0, float y0, float x1, float y1, Func<float, float, float> sdf, Color col, float soft = 1f) =>
                Shape(x0, y0, x1, y1, sdf, (x, y, d) => col, soft);

            public void Circle(float cx, float cy, float r, Color col, float soft = 1f) =>
                Shape(cx - r, cy - r, cx + r, cy + r, (x, y) => PixelCanvas.Circle(x, y, cx, cy, r), col, soft);

            public void Ellipse(float cx, float cy, float rx, float ry, Color col, float soft = 1f) =>
                Shape(cx - rx, cy - ry, cx + rx, cy + ry, (x, y) => PixelCanvas.Ellipse(x, y, cx, cy, rx, ry), col, soft);

            public void Box(float cx, float cy, float hw, float hh, float radius, Color col, float soft = 1f) =>
                Shape(cx - hw, cy - hh, cx + hw, cy + hh, (x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, hw, hh, radius), col, soft);

            public void Line(float ax, float ay, float bx, float by, float t, Color col, float soft = 1f) =>
                Shape(Math.Min(ax, bx) - t, Math.Min(ay, by) - t, Math.Max(ax, bx) + t, Math.Max(ay, by) + t,
                    (x, y) => PixelCanvas.Segment(x, y, ax, ay, bx, by, t), col, soft);

            public void Poly(List<Vector2> pts, Color col, float soft = 1f)
            {
                float x0 = float.MaxValue, y0 = float.MaxValue, x1 = float.MinValue, y1 = float.MinValue;
                foreach (var p in pts) { x0 = Math.Min(x0, p.x); y0 = Math.Min(y0, p.y); x1 = Math.Max(x1, p.x); y1 = Math.Max(y1, p.y); }
                Shape(x0, y0, x1, y1, (x, y) => PixelCanvas.Polygon(x, y, pts), col, soft);
            }

            /// <summary>A full-height vertical band (tiles by itself).</summary>
            public void Column(float x0, float x1, Func<float, float, float, Color> shade) =>
                Canvas.PaintIn(x0, 0, x1, H, (x, y) => Math.Max(x0 - x, x - x1), shade);

            public Sprite Sprite(float ppu = 100f) => Canvas.ToSprite(ppu);
        }

        // ------------------------------------------------------------------ backdrop

        /// <summary>Tall transparent backdrop that tiles vertically; shown behind the platforms with parallax.</summary>
        public static Sprite Backdrop(TowerScenery s) => Cached("back_" + s, () =>
        {
            var p = new Painter(BackW, BackH, 100 + (int)s);
            switch (s)
            {
                case TowerScenery.Jungle: Jungle(p); break;
                case TowerScenery.Forest: Forest(p); break;
                case TowerScenery.Candy: Candy(p); break;
                case TowerScenery.Space: Space(p); break;
                case TowerScenery.Lava: Lava(p); break;
                default: Ice(p); break;
            }
            return p.Sprite();
        });

        private static void Leaf(Painter p, float x, float y, float len, float angle, Color col)
        {
            float ca = Mathf.Cos(angle), sa = Mathf.Sin(angle);
            float ex = x + ca * len, ey = y + sa * len;
            float mx = (x + ex) / 2f, my = (y + ey) / 2f;
            float w = len * 0.32f;
            p.Shape(Math.Min(x, ex) - w, Math.Min(y, ey) - w, Math.Max(x, ex) + w, Math.Max(y, ey) + w,
                (px, py) =>
                {
                    float dx = px - mx, dy = py - my;
                    float u = (dx * ca + dy * sa) / (len / 2f), v = (-dx * sa + dy * ca) / w;
                    return (Mathf.Sqrt(u * u + v * v) - 1f) * w;
                }, col);
            p.Line(x, y, ex, ey, 0.8f, PixelCanvas.Darken(col, 0.25f));
        }

        private static void Jungle(Painter p)
        {
            // Far trunks
            foreach (var x in new[] { 34f, 300f, 360f })
            {
                float w = p.R(14, 22);
                p.Column(x - w, x + w, (px, py, d) => Color.Lerp(C("#123726", 0.9f), C("#1C4A30", 0.9f), (px - x + w) / (2 * w)));
            }
            // Waterfall with streaks and mist
            float wx0 = 170, wx1 = 240;
            p.Column(wx0, wx1, (px, py, d) =>
            {
                float streak = Mathf.Repeat(px * 0.37f + Mathf.Sin(px * 0.21f) * 3f, 1f);
                float flow = 0.5f + 0.5f * Mathf.Sin(py * 0.05f * Mathf.PI * 2f / 3.84f + px * 0.7f);
                return Color.Lerp(C("#3FA9D6", 0.75f), C("#D8F6FF", 0.9f), streak * 0.5f + flow * 0.35f);
            });
            for (int i = 0; i < 10; i++) p.Circle(p.R(wx0 - 10, wx1 + 10), p.R(0, BackH), p.R(10, 20), C("#E8FBFF", 0.18f), 6f);
            // Rocks beside the falls
            for (int i = 0; i < 4; i++) p.Ellipse(i % 2 == 0 ? wx0 - 8 : wx1 + 8, p.R(0, BackH), p.R(16, 24), p.R(12, 18), C("#2A4A3A", 0.95f));
            // Vines with leaves and flowers
            for (int v = 0; v < 5; v++)
            {
                float vx = p.R(20, BackW - 20), ph = p.R(0, 6.28f), amp = p.R(6, 14);
                for (float y = 0; y < BackH; y += 6)
                {
                    float x0 = vx + Mathf.Sin(y * 2f * Mathf.PI / BackH * 3f + ph) * amp;
                    float x1 = vx + Mathf.Sin((y + 6) * 2f * Mathf.PI / BackH * 3f + ph) * amp;
                    p.Line(x0, y, x1, y + 6, 2.2f, C("#2E7A3E", 0.95f));
                    if ((int)y % 36 == 0) Leaf(p, x0, y, p.R(14, 22), p.R(0, 1) < 0.5f ? p.R(-0.6f, 0.4f) : p.R(2.7f, 3.7f), C("#4FB85A"));
                    if ((int)y % 150 == 0)
                    {
                        var petal = p.Rng.Next(2) == 0 ? C("#FF6F91") : C("#FFB347");
                        for (int k = 0; k < 5; k++) { float a = k * 1.2566f; p.Circle(x0 + Mathf.Cos(a) * 5, y + Mathf.Sin(a) * 5, 4.5f, petal); }
                        p.Circle(x0, y, 3f, C("#FFE27A"));
                    }
                }
            }
            // Big leaf clusters
            for (int i = 0; i < 16; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH);
                var col = Color.Lerp(C("#2F8F4A"), C("#5CCB60"), p.R(0, 1));
                for (int k = 0; k < 5; k++) Leaf(p, x, y, p.R(22, 38), p.R(0, 6.28f), col);
            }
        }

        private static void Forest(Painter p)
        {
            // Birch and pine trunks
            for (int t = 0; t < 4; t++)
            {
                float x = 30 + t * 105 + p.R(-12, 12), w = p.R(9, 14);
                bool birch = t % 2 == 0;
                p.Column(x - w, x + w, (px, py, d) =>
                {
                    if (!birch) return Color.Lerp(C("#3B2A1E", 0.95f), C("#5A3F2A", 0.95f), (px - x + w) / (2 * w));
                    float mark = Mathf.Repeat(py * 0.031f + Mathf.Sin(px) * 0.2f, 1f) < 0.12f ? 1f : 0f;
                    return Color.Lerp(C("#E9E4D8", 0.95f), C("#2B2620", 0.95f), mark * (Mathf.Abs(px - x) < w * 0.7f ? 1f : 0f));
                });
                // Branches with leaf clumps
                for (float y = p.R(0, 80); y < BackH; y += p.R(90, 140))
                {
                    float side = p.Rng.Next(2) == 0 ? -1 : 1;
                    float bx = x + side * p.R(30, 55), by = y + p.R(10, 25);
                    p.Line(x, y, bx, by, 3f, birch ? C("#6B5A48") : C("#4A3324"));
                    var leaf = birch ? C("#7BC66A") : C("#2F6B3A");
                    for (int k = 0; k < 6; k++) p.Circle(bx + p.R(-16, 16), by + p.R(-10, 12), p.R(9, 15), PixelCanvas.WithAlpha(Color.Lerp(leaf, C("#E8C547"), p.R(0, 0.35f)), 0.95f));
                }
            }
            // Pine silhouettes in the distance
            for (int i = 0; i < 7; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH), h = p.R(60, 110);
                for (int k = 0; k < 3; k++)
                {
                    float yy = y + k * h * 0.28f, w = h * (0.42f - k * 0.1f);
                    p.Poly(new List<Vector2> { new Vector2(x - w, yy), new Vector2(x + w, yy), new Vector2(x, yy + h * 0.45f) }, C("#1D3A2A", 0.85f));
                }
            }
            // Mushrooms and ferns on ledges
            for (int i = 0; i < 8; i++)
            {
                float x = p.R(10, BackW - 10), y = p.R(0, BackH);
                p.Box(x, y, 3, 8, 2, C("#F2E6D0"));
                p.Shape(x - 12, y + 6, x + 12, y + 18, (px, py) => Math.Max(PixelCanvas.Ellipse(px, py, x, y + 8, 12, 10), y + 8 - py), C("#E0483E"));
                p.Circle(x - 4, y + 13, 2f, Color.white);
                p.Circle(x + 5, y + 11, 1.6f, Color.white);
            }
            for (int i = 0; i < 10; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH);
                for (int k = -2; k <= 2; k++) Leaf(p, x, y, p.R(14, 22), Mathf.PI / 2f + k * 0.35f, C("#3E8A44"));
            }
            // An owl in a tree hollow
            float ox = 135, oy = 420;
            p.Ellipse(ox, oy, 18, 24, C("#24170F"));
            p.Ellipse(ox, oy - 2, 12, 15, C("#8A6A4A"));
            p.Circle(ox - 5, oy + 3, 4.5f, Color.white); p.Circle(ox + 5, oy + 3, 4.5f, Color.white);
            p.Circle(ox - 5, oy + 3, 2f, C("#1D1A10")); p.Circle(ox + 5, oy + 3, 2f, C("#1D1A10"));
            p.Poly(new List<Vector2> { new Vector2(ox - 2, oy - 1), new Vector2(ox + 2, oy - 1), new Vector2(ox, oy - 5) }, C("#FFB347"));
        }

        private static void Ice(Painter p)
        {
            // Aurora ribbons (tile because they repeat every BackH)
            foreach (var band in new[] { (x: 90f, col: C("#5CFFC8", 0.22f)), (x: 260f, col: C("#7AA8FF", 0.2f)) })
            {
                float bx = band.x;
                var bc = band.col;
                p.Canvas.PaintIn(0, 0, BackW, BackH, (x, y) => Mathf.Abs(x - (bx + Mathf.Sin(y / BackH * Mathf.PI * 4f) * 40f)) - 22f,
                    (x, y, d) => PixelCanvas.WithAlpha(bc, bc.a * Mathf.Clamp01(-d / 22f)), 8f);
            }
            // Frozen waterfall
            p.Column(300, 344, (px, py, d) => Color.Lerp(C("#9FD8FF", 0.7f), C("#EAF8FF", 0.85f), Mathf.Repeat(px * 0.13f + Mathf.Sin(py * 0.02f) * 0.3f, 1f)));
            // Ice ledges with icicles
            for (int i = 0; i < 6; i++)
            {
                float x = p.R(0, BackW - 80), y = p.R(0, BackH), w = p.R(50, 90);
                p.Box(x + w / 2, y, w / 2, 6, 5, C("#DDF3FF", 0.95f));
                for (float ix = x + 4; ix < x + w - 4; ix += p.R(7, 12))
                {
                    float h = p.R(10, 28);
                    p.Poly(new List<Vector2> { new Vector2(ix - 3.5f, y - 3), new Vector2(ix + 3.5f, y - 3), new Vector2(ix, y - 3 - h) }, C("#BFE8FF", 0.95f));
                }
            }
            // Snowy pines
            for (int i = 0; i < 6; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH), h = p.R(50, 90);
                for (int k = 0; k < 3; k++)
                {
                    float yy = y + k * h * 0.25f, w = h * (0.4f - k * 0.1f);
                    p.Poly(new List<Vector2> { new Vector2(x - w, yy), new Vector2(x + w, yy), new Vector2(x, yy + h * 0.42f) }, C("#2E5A7A", 0.9f));
                    p.Poly(new List<Vector2> { new Vector2(x - w * 0.6f, yy + h * 0.17f), new Vector2(x + w * 0.6f, yy + h * 0.17f), new Vector2(x, yy + h * 0.42f) }, C("#F2FAFF", 0.9f));
                }
            }
            // Crystals
            for (int i = 0; i < 10; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH), h = p.R(12, 24);
                p.Poly(new List<Vector2> { new Vector2(x, y - h), new Vector2(x + h * 0.35f, y), new Vector2(x, y + h), new Vector2(x - h * 0.35f, y) }, C("#B7F0FF", 0.85f));
            }
        }

        private static void Candy(Painter p)
        {
            // Candy-cane pillars: stripe period divides the height so they tile
            foreach (var x in new[] { 40f, 344f })
                p.Column(x - 18, x + 18, (px, py, d) => Mathf.Repeat((py + px * 0.8f) / 48f, 1f) < 0.5f ? C("#FF5A7E", 0.95f) : C("#FFF2F6", 0.95f));
            // Chocolate drips from ledges
            for (int i = 0; i < 4; i++)
            {
                float x = p.R(70, 280), y = p.R(0, BackH), w = p.R(50, 80);
                p.Box(x, y, w / 2, 7, 6, C("#6B3A24"));
                for (float dx = -w / 2 + 6; dx < w / 2 - 4; dx += p.R(9, 15))
                {
                    float h = p.R(8, 26);
                    p.Box(x + dx, y - h / 2, 3.5f, h / 2, 3.5f, C("#6B3A24"));
                }
            }
            // Lollipops, donuts, gumdrops
            for (int i = 0; i < 7; i++)
            {
                float x = p.R(70, 310), y = p.R(0, BackH), r = p.R(14, 22);
                p.Line(x, y, x, y - r * 2.4f, 2.5f, C("#F5F0E8"));
                var c1 = i % 2 == 0 ? C("#FF6F91") : C("#7A5CFF");
                p.Circle(x, y, r, c1);
                p.Shape(x - r, y - r, x + r, y + r, (px, py) => Mathf.Abs(PixelCanvas.Circle(px, py, x, y, r * 0.55f)) - r * 0.12f, C("#FFFFFF", 0.8f));
            }
            for (int i = 0; i < 5; i++)
            {
                float x = p.R(20, 364), y = p.R(0, BackH);
                p.Shape(x - 18, y - 18, x + 18, y + 18, (px, py) => Mathf.Abs(PixelCanvas.Circle(px, py, x, y, 12f)) - 6f, C("#E8A15A"));
                p.Shape(x - 18, y - 18, x + 18, y + 18, (px, py) => Math.Max(Mathf.Abs(PixelCanvas.Circle(px, py, x, y, 12f)) - 5f, y - 2 - py), C("#FF8FC0"));
            }
            for (int i = 0; i < 9; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH);
                Color[] gum = { C("#5CE1A8"), C("#FFC93C"), C("#5AB8FF"), C("#FF7A5C") };
                p.Shape(x - 10, y - 2, x + 10, y + 16, (px, py) => Math.Max(PixelCanvas.Ellipse(px, py, x, y, 10, 14), y - py), gum[i % 4]);
            }
            // Sprinkles everywhere
            Color[] spr = { C("#FF5A7E"), C("#FFE27A"), C("#7AE0FF"), C("#B48CFF"), Color.white };
            for (int i = 0; i < 140; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH), a = p.R(0, 3.14f);
                p.Line(x, y, x + Mathf.Cos(a) * 5, y + Mathf.Sin(a) * 5, 1.4f, PixelCanvas.WithAlpha(spr[i % spr.Length], 0.8f));
            }
        }

        private static void Space(Painter p)
        {
            // Nebula clouds
            Color[] neb = { C("#7A3CFF", 0.10f), C("#FF4FA3", 0.08f), C("#3DE0FF", 0.07f) };
            for (int i = 0; i < 14; i++) p.Circle(p.R(0, BackW), p.R(0, BackH), p.R(40, 90), neb[i % 3], 50f);
            // Stars
            for (int i = 0; i < 260; i++) p.Circle(p.R(0, BackW), p.R(0, BackH), p.R(0.6f, 1.8f), C("#FFFFFF", p.R(0.4f, 1f)));
            for (int i = 0; i < 12; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH);
                p.Line(x - 6, y, x + 6, y, 0.9f, C("#FFFFFF", 0.8f)); p.Line(x, y - 6, x, y + 6, 0.9f, C("#FFFFFF", 0.8f));
            }
            // Planets
            float px0 = 290, py0 = 560;
            p.Shape(px0 - 70, py0 - 70, px0 + 70, py0 + 70, (x, y) => PixelCanvas.Circle(x, y, px0, py0, 44),
                (x, y, d) => Color.Lerp(C("#FF9F6B"), C("#C8508A"), Mathf.Repeat((y - py0) / 18f, 1f) * 0.4f + (x - px0 + 44) / 88f * 0.6f));
            p.Shape(px0 - 80, py0 - 22, px0 + 80, py0 + 22, (x, y) => Mathf.Abs(PixelCanvas.Ellipse(x, y, px0, py0, 74, 14)) - 3f, C("#FFE2B8", 0.8f));
            float qx = 80, qy = 180;
            p.Shape(qx - 30, qy - 30, qx + 30, qy + 30, (x, y) => PixelCanvas.Circle(x, y, qx, qy, 26), (x, y, d) => Color.Lerp(C("#3D7BFF"), C("#5CFFC8"), (y - qy + 26) / 52f));
            p.Circle(qx - 8, qy + 6, 7, C("#2BB673", 0.8f)); p.Circle(qx + 9, qy - 7, 5, C("#2BB673", 0.8f));
            // Asteroids
            for (int i = 0; i < 8; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH), r = p.R(6, 14);
                p.Circle(x, y, r, C("#6B6480"));
                p.Circle(x - r * 0.3f, y + r * 0.2f, r * 0.3f, C("#4E4862"));
            }
            // A little satellite
            float sx = 180, sy = 330;
            p.Box(sx, sy, 8, 8, 2, C("#D8DCF0"));
            p.Box(sx - 22, sy, 12, 5, 1, C("#3DA0FF")); p.Box(sx + 22, sy, 12, 5, 1, C("#3DA0FF"));
            p.Line(sx, sy + 8, sx + 6, sy + 18, 1.2f, C("#D8DCF0"));
        }

        private static void Lava(Painter p)
        {
            // Basalt pillars
            foreach (var x in new[] { 30f, 120f, 350f })
            {
                float w = p.R(16, 26);
                p.Column(x - w, x + w, (px, py, d) => Color.Lerp(C("#2A1414", 0.95f), C("#3E2020", 0.95f), Mathf.Repeat((px - x) / 12f, 1f)));
            }
            // Lava falls
            foreach (var x in new[] { 230f, 290f })
                p.Column(x - 12, x + 12, (px, py, d) =>
                {
                    float f = 0.5f + 0.5f * Mathf.Sin(py * 2f * Mathf.PI / BackH * 9f + px * 0.4f);
                    return Color.Lerp(C("#FF4A1C", 0.9f), C("#FFD04A", 0.95f), f * (1f - Mathf.Abs(px - x) / 12f));
                });
            // Glowing cracks
            for (int i = 0; i < 9; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH);
                for (int k = 0; k < 4; k++)
                {
                    float nx = x + p.R(-22, 22), ny = y + p.R(10, 26);
                    p.Line(x, y, nx, ny, 3.5f, C("#FF6A2A", 0.25f), 3f);
                    p.Line(x, y, nx, ny, 1.2f, C("#FFC34A", 0.9f));
                    x = nx; y = ny;
                }
            }
            // Rocks and crystals
            for (int i = 0; i < 10; i++) p.Ellipse(p.R(0, BackW), p.R(0, BackH), p.R(10, 20), p.R(8, 14), C("#4A2828", 0.95f));
            for (int i = 0; i < 6; i++)
            {
                float x = p.R(0, BackW), y = p.R(0, BackH), h = p.R(14, 26);
                p.Poly(new List<Vector2> { new Vector2(x - h * 0.3f, y), new Vector2(x + h * 0.3f, y), new Vector2(x, y + h) }, C("#FF8A3C", 0.9f));
            }
            // Smoke puffs
            for (int i = 0; i < 10; i++) p.Circle(p.R(0, BackW), p.R(0, BackH), p.R(18, 36), C("#120808", 0.18f), 20f);
        }

        // ------------------------------------------------------------------ walls

        /// <summary>Wall texture (tiles vertically), 96 x 192 px.</summary>
        public static Sprite Wall(TowerScenery s) => Cached("wall_" + s, () =>
        {
            var p = new Painter(96, 192, 300 + (int)s);
            switch (s)
            {
                case TowerScenery.Jungle:
                case TowerScenery.Forest:
                    p.Column(0, 96, (x, y, d) => Color.Lerp(C(s == TowerScenery.Jungle ? "#4A3220" : "#5A3E28"), C("#6E4C30"), 0.5f + 0.5f * Mathf.Sin(x * 0.35f + Mathf.Sin(y * 2f * Mathf.PI / 192f * 2f) * 2f)));
                    for (int i = 0; i < 7; i++) p.Ellipse(p.R(10, 86), p.R(0, 192), p.R(10, 20), p.R(6, 12), C(s == TowerScenery.Jungle ? "#3E9A4A" : "#5E8A3A", 0.9f));
                    for (int i = 0; i < 3; i++) Leaf(p, p.R(20, 76), p.R(0, 192), p.R(14, 22), p.R(0, 6.28f), C("#58C060"));
                    break;
                case TowerScenery.Candy:
                    p.Column(0, 96, (x, y, d) => Mathf.Repeat((y + x) / 48f, 1f) < 0.5f ? C("#FF8FB8") : C("#FFE4EE"));
                    for (int i = 0; i < 4; i++) p.Circle(p.R(10, 86), p.R(0, 192), p.R(5, 9), C("#FFFFFF", 0.6f));
                    break;
                case TowerScenery.Space:
                    p.Column(0, 96, (x, y, d) => C("#2E3266"));
                    for (int r = 0; r < 4; r++)
                    {
                        float y = r * 48 + 24;
                        p.Box(48, y, 42, 21, 5, C("#3D4285"));
                        p.Circle(12, y + 15, 2.5f, C("#8A90D6")); p.Circle(84, y + 15, 2.5f, C("#8A90D6"));
                        p.Circle(12, y - 15, 2.5f, C("#8A90D6")); p.Circle(84, y - 15, 2.5f, C("#8A90D6"));
                    }
                    p.Column(44, 52, (x, y, d) => Color.Lerp(C("#3DE0FF", 0.5f), C("#B48CFF", 0.7f), 0.5f + 0.5f * Mathf.Sin(y * 2f * Mathf.PI / 192f * 4f)));
                    break;
                case TowerScenery.Lava:
                    p.Column(0, 96, (x, y, d) => C("#FF6A2A"));
                    for (int r = 0; r < 4; r++)
                        for (int c = -1; c < 3; c++)
                        {
                            float off = r % 2 == 0 ? 0 : 24;
                            p.Box(c * 48 + 24 + off, r * 48 + 24, 22, 22, 4, Color.Lerp(C("#3A1E1E"), C("#4E2828"), p.R(0, 1)));
                        }
                    break;
                default:
                    p.Column(0, 96, (x, y, d) => C("#A8D8F5"));
                    for (int r = 0; r < 6; r++)
                        for (int c = -1; c < 3; c++)
                        {
                            float off = r % 2 == 0 ? 0 : 24;
                            float bx = c * 48 + 24 + off, by = r * 32 + 16;
                            p.Box(bx, by, 22, 14, 5, Color.Lerp(C("#D4EEFF"), C("#EFF9FF"), p.R(0, 1)));
                            p.Box(bx - 8, by + 7, 8, 2, 2, C("#FFFFFF", 0.7f));
                        }
                    break;
            }
            return p.Sprite();
        });

        // ------------------------------------------------------------------ platforms

        /// <summary>A platform in the scenery's style (9-sliced: 160 x 48 px, 26 px ends).</summary>
        public static Sprite Platform(TowerScenery s, PlatformKind kind)
        {
            string style = kind == PlatformKind.Ice ? "ice" : kind == PlatformKind.Goal ? "goal" : kind == PlatformKind.Crumble ? "crumble" : s.ToString();
            return Cached("plat_" + style, () =>
            {
                var p = new Painter(160, 48, 7) { Wrap = false };
                var cv = p.Canvas;
                Color body, top, edge;
                switch (style)
                {
                    case "ice": body = C("#8FDFFF"); top = C("#E8FBFF"); edge = C("#4FA8D8"); break;
                    case "goal": body = C("#FFC93C"); top = C("#FFF0A0"); edge = C("#C98A00"); break;
                    case "crumble": body = C("#A8774A"); top = C("#C9986A"); edge = C("#6E4826"); break;
                    case "Candy": body = C("#FF6F91"); top = C("#FFF4F8"); edge = C("#C83A62"); break;
                    case "Jungle": body = C("#7A5232"); top = C("#6BCB5A"); edge = C("#4A2E18"); break;
                    case "Forest": body = C("#8A5E3A"); top = C("#4E9A44"); edge = C("#5A3A20"); break;
                    case "Space": body = C("#5A60A8"); top = C("#A9B0FF"); edge = C("#2E3266"); break;
                    case "Lava": body = C("#3E2424"); top = C("#FF8A3C"); edge = C("#1E1010"); break;
                    default: body = C("#CFEFFF"); top = Color.white; edge = C("#7FB8DE"); break;
                }
                cv.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 80, 22, 78, 20, 14), (x, y, d) => Color.Lerp(body, PixelCanvas.Darken(body, 0.25f), 1f - y / 44f));
                cv.Paint((x, y) => Mathf.Abs(PixelCanvas.RoundedBox(x, y, 80, 22, 78, 20, 14)) - 1.5f, edge);
                // Top layer (snow / grass / frosting / glow)
                cv.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 80, 37, 78, 8, 8), top);
                switch (style)
                {
                    case "Jungle":
                    case "Forest":
                        for (int i = 0; i < 18; i++) { float gx = 8 + i * 8.4f; cv.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(gx - 3, 40), new Vector2(gx + 3, 40), new Vector2(gx + 1, 47) }), top); }
                        for (int i = 0; i < 5; i++) { float kx = 18 + i * 31; cv.Paint((x, y) => PixelCanvas.Segment(x, y, kx, 8, kx + 6, 26, 1.2f), PixelCanvas.Darken(body, 0.35f)); }
                        break;
                    case "Candy":
                        for (int i = 0; i < 7; i++) { float dx = 14 + i * 22; float h = 6 + (i * 7) % 9; cv.Paint((x, y) => PixelCanvas.RoundedBox(x, y, dx, 32 - h / 2, 4, h / 2, 4), top); }
                        for (int i = 0; i < 9; i++) { float dx = 10 + i * 17; cv.Paint((x, y) => PixelCanvas.Segment(x, y, dx, 39, dx + 4, 41, 1.2f), i % 2 == 0 ? C("#5AB8FF") : C("#FFC93C")); }
                        break;
                    case "Space":
                        for (int i = 0; i < 6; i++) { float lx = 18 + i * 25; cv.Paint((x, y) => PixelCanvas.Circle(x, y, lx, 18, 3.2f), i % 2 == 0 ? C("#3DE0FF") : C("#FF4FA3")); }
                        break;
                    case "Lava":
                        for (int i = 0; i < 4; i++) { float lx = 20 + i * 38; cv.Paint((x, y) => PixelCanvas.Segment(x, y, lx, 10, lx + 10, 26, 1.6f), C("#FF8A3C")); }
                        break;
                    case "crumble":
                        foreach (var cx in new[] { 40f, 92f, 128f })
                        {
                            cv.Paint((x, y) => PixelCanvas.Segment(x, y, cx, 40, cx + 6, 24, 1.6f), edge);
                            cv.Paint((x, y) => PixelCanvas.Segment(x, y, cx + 6, 24, cx - 2, 8, 1.6f), edge);
                        }
                        break;
                    case "goal":
                        for (int i = 0; i < 8; i++) { float lx = 12 + i * 19; cv.Paint((x, y) => PixelCanvas.Circle(x, y, lx, 22, 3f), Color.white); }
                        break;
                    default:
                        for (int i = 0; i < 6; i++) { float ix = 14 + i * 26; float h = 5 + (i * 5) % 8; cv.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(ix - 3, 30), new Vector2(ix + 3, 30), new Vector2(ix, 30 - h) }), C("#E8FBFF")); }
                        break;
                }
                return cv.ToSprite(100f, new Vector4(26, 0, 26, 0));
            });
        }

        /// <summary>Little marks drawn on special platforms.</summary>
        public static Sprite Chevrons() => Cached("chev", () =>
        {
            var c = new PixelCanvas(64, 32);
            foreach (var sx in new[] { -1f, 1f })
                c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(32 + sx * 10, 6), new Vector2(32 + sx * 26, 16), new Vector2(32 + sx * 10, 26) }) - 1f, new Color(1, 1, 1, 0.9f));
            return c.ToSprite(100f);
        });

        public static Sprite Spring() => Cached("spring", () =>
        {
            var c = new PixelCanvas(64, 48);
            for (int i = 0; i < 4; i++)
            {
                float y = 8 + i * 8;
                c.Paint((x, yy) => PixelCanvas.Segment(x, yy, 18, y, 46, y + 4, 2.6f), ColorUtil.Hex("#C8CCDD"));
            }
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 32, 42, 22, 5, 4), ColorUtil.Red);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 32, 4, 18, 4, 3), ColorUtil.Hex("#6B6F8A"));
            return c.ToSprite(100f);
        });

        public static Sprite Flag() => Cached("flag", () =>
        {
            var c = new PixelCanvas(64, 96);
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 14, 4, 14, 92, 2.5f), ColorUtil.Hex("#E8E8F0"));
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(16, 90), new Vector2(60, 76), new Vector2(16, 62) }), ColorUtil.Red);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 30, 76, 5), Color.white);
            return c.ToSprite(100f);
        });

        // ------------------------------------------------------------------ critters

        public enum CritterKind { Bird, Leaf, Snow, Firefly, Ember, Bubble, Butterfly, ShootingStar, Sprinkle }

        /// <summary>What drifts or flies around in each scenery.</summary>
        public static CritterKind[] CrittersFor(TowerScenery s)
        {
            switch (s)
            {
                case TowerScenery.Jungle: return new[] { CritterKind.Bird, CritterKind.Butterfly, CritterKind.Leaf, CritterKind.Firefly };
                case TowerScenery.Forest: return new[] { CritterKind.Bird, CritterKind.Leaf, CritterKind.Leaf, CritterKind.Firefly };
                case TowerScenery.Candy: return new[] { CritterKind.Bubble, CritterKind.Sprinkle, CritterKind.Butterfly };
                case TowerScenery.Space: return new[] { CritterKind.ShootingStar, CritterKind.Firefly, CritterKind.Firefly };
                case TowerScenery.Lava: return new[] { CritterKind.Ember, CritterKind.Ember, CritterKind.Bubble };
                default: return new[] { CritterKind.Snow, CritterKind.Snow, CritterKind.Bird };
            }
        }

        /// <summary>Critter sprite; birds and butterflies have two wing frames.</summary>
        public static Sprite Critter(CritterKind k, int frame = 0) => Cached("critter_" + k + frame, () =>
        {
            var c = new PixelCanvas(64, 64);
            switch (k)
            {
                case CritterKind.Bird:
                {
                    var body = ColorUtil.Hex("#1E2140");
                    float wy = frame == 0 ? 48 : 22;
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 8, wy, 30, 32, 4f), body);
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 56, wy, 34, 32, 4f), body);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 32, 31, 8, 6), body);
                    break;
                }
                case CritterKind.Butterfly:
                {
                    var w1 = ColorUtil.Hex("#FF8FC0"); var w2 = ColorUtil.Hex("#FFC93C");
                    float sx = frame == 0 ? 1f : 0.45f;
                    foreach (var side in new[] { -1f, 1f })
                    {
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 32 + side * 12 * sx, 38, 12 * sx + 1, 12), w1);
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 32 + side * 9 * sx, 22, 8 * sx + 1, 8), w2);
                    }
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 32, 30, 2.5f, 14), ColorUtil.Hex("#2A1440"));
                    break;
                }
                case CritterKind.Leaf:
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 32, 32, 22, 11), frame == 0 ? ColorUtil.Hex("#E8A13A") : ColorUtil.Hex("#7CC85A"));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 12, 32, 52, 32, 1.2f), new Color(0, 0, 0, 0.3f));
                    break;
                case CritterKind.Snow:
                    for (int i = 0; i < 3; i++)
                    {
                        float a = i * Mathf.PI / 3f;
                        c.Paint((x, y) => PixelCanvas.Segment(x, y, 32 - Mathf.Cos(a) * 22, 32 - Mathf.Sin(a) * 22, 32 + Mathf.Cos(a) * 22, 32 + Mathf.Sin(a) * 22, 3f), Color.white);
                    }
                    break;
                case CritterKind.Bubble:
                    c.Paint((x, y) => Mathf.Abs(PixelCanvas.Circle(x, y, 32, 32, 22)) - 2.5f, new Color(1, 1, 1, 0.75f));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 24, 40, 5), new Color(1, 1, 1, 0.8f));
                    break;
                case CritterKind.ShootingStar:
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 6, 50, 52, 14, 3f), (x, y, d) => new Color(1, 1, 1, Mathf.Clamp01((x - 6) / 46f)));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 52, 14, 5), Color.white);
                    break;
                case CritterKind.Sprinkle:
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 18, 24, 46, 40, 6f), Color.white);
                    break;
                default: // glowing dot (firefly / ember)
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 32, 32, 30), (x, y, d) => new Color(1, 1, 1, Mathf.Clamp01(-d / 30f) * Mathf.Clamp01(-d / 30f)), 1f);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 32, 32, 7), Color.white);
                    break;
            }
            return c.ToSprite(100f);
        });
    }
}
