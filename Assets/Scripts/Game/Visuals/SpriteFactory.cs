using System.Collections.Generic;
using JellyDrop.Core;
using JellyDrop.Core.Economy;
using UnityEngine;

namespace JellyDrop.Visuals
{
    public enum AvatarMood { Happy, Calm, Excited, Shocked, Sad }

    /// <summary>
    /// Draws and caches every sprite in the game (blocks, icons, avatars, UI shapes).
    /// Replace any of these with hand-made art later by returning a loaded sprite instead.
    /// </summary>
    public static class SpriteFactory
    {
        public const int BlockPixels = 64;
        /// <summary>Blocks are 1 world unit wide.</summary>
        public const float BlockPpu = BlockPixels;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static void ClearCache() => Cache.Clear();

        private static Sprite Cached(string key, System.Func<Sprite> create)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            s = create();
            s.name = key;
            Cache[key] = s;
            return s;
        }

        // ================================================================== blocks

        public static Sprite Block(BlockStyle style, Color color) =>
            Cached($"block_{style}_{ColorUtility.ToHtmlStringRGB(color)}", () => DrawBlock(style, color));

        public static Sprite GoldenBlock() => Block(BlockStyle.Gold, ColorUtil.Hex("#FFD447"));

        private static Sprite DrawBlock(BlockStyle style, Color color)
        {
            int s = BlockPixels;
            var c = new PixelCanvas(s, s);
            float half = s * 0.5f - 2f;
            float radius = style == BlockStyle.Pixel ? 0f : style == BlockStyle.Gem ? 8f : 14f;

            if (style == BlockStyle.Pixel) return DrawPixelBlock(color);

            System.Func<float, float, float> body = (x, y) => PixelCanvas.RoundedBox(x, y, s / 2f, s / 2f, half, half, radius);

            if (style == BlockStyle.Neon)
            {
                // Bright neon rim fading into a slightly darker, still saturated centre.
                var core = PixelCanvas.Darken(color, 0.22f);
                c.Paint(body, (x, y, d) =>
                {
                    float edge = Mathf.Clamp01((-d) / 14f);
                    var fill = Color.Lerp(PixelCanvas.Lighten(color, 0.45f), core, edge);
                    return Color.Lerp(fill, PixelCanvas.Lighten(fill, 0.12f), y / s);
                });
                c.Paint((x, y) => Mathf.Abs(body(x, y) + 4f) - 1.6f, PixelCanvas.Lighten(color, 0.6f));
                c.Paint((x, y) => PixelCanvas.Ellipse(x, y, s * 0.33f, s * 0.7f, 7f, 3f), new Color(1, 1, 1, 0.55f));
                return c.ToSprite(BlockPpu);
            }

            var top = PixelCanvas.Lighten(color, 0.28f);
            var bottom = PixelCanvas.Darken(color, 0.12f);
            var outline = PixelCanvas.Darken(color, 0.38f);

            // Jelly body: vertical gradient with a darker rim.
            c.Paint(body, (x, y, d) =>
            {
                float t = y / s;
                var fill = Color.Lerp(bottom, top, t);
                float rim = Mathf.Clamp01((d + 4f) / 2f);
                return Color.Lerp(fill, outline, rim);
            });

            if (style == BlockStyle.Gem)
            {
                DrawGemFacets(c, color, s);
            }
            else
            {
                // Bottom inner shadow for a squishy look.
                c.Paint((x, y) => Mathf.Max(body(x, y), y - 14f), new Color(0, 0, 0, 0.12f), 6f);
                // Glossy highlights.
                c.Paint((x, y) => PixelCanvas.Ellipse(x, y, s * 0.36f, s * 0.74f, 13f, 5.5f), new Color(1, 1, 1, 0.7f));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, s * 0.68f, s * 0.75f, 3f), new Color(1, 1, 1, 0.8f));
            }

            switch (style)
            {
                case BlockStyle.Gummy:
                    DrawFace(c, s / 2f, s * 0.46f, 1f);
                    break;
                case BlockStyle.Sprinkles:
                    DrawSprinkles(c, s);
                    break;
                case BlockStyle.Gold:
                    var star = PixelCanvas.StarPoints(s * 0.7f, s * 0.36f, 9f, 3f, 4, 0f);
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, star), new Color(1, 1, 1, 0.95f));
                    break;
            }
            return c.ToSprite(BlockPpu);
        }

        private static Sprite DrawPixelBlock(Color color)
        {
            // 8x8 pixel-art tile scaled to 64 with point filtering.
            const int n = 8;
            var small = new Color[n, n];
            for (int y = 0; y < n; y++)
            {
                for (int x = 0; x < n; x++)
                {
                    Color c = color;
                    if (x == 0 || y == n - 1) c = PixelCanvas.Lighten(color, 0.45f);
                    if (x == n - 1 || y == 0) c = PixelCanvas.Darken(color, 0.35f);
                    small[x, y] = c;
                }
            }
            small[1, 6] = Color.white;
            small[2, 6] = PixelCanvas.Lighten(color, 0.7f);
            small[1, 5] = PixelCanvas.Lighten(color, 0.7f);

            int s = BlockPixels;
            var canvas = new PixelCanvas(s, s);
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                    canvas.Set(x, y, small[x * n / s, y * n / s]);
            return canvas.ToSprite(BlockPpu, default, FilterMode.Point);
        }

        private static void DrawGemFacets(PixelCanvas c, Color color, int s)
        {
            float inner = s * 0.22f;
            float cx = s / 2f;
            float bodyHalf = s * 0.5f - 6f;
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cx, bodyHalf, bodyHalf, 5f), (x, y, d) =>
            {
                float dx = x - cx, dy = y - cx;
                if (Mathf.Abs(dx) < inner && Mathf.Abs(dy) < inner) return PixelCanvas.Lighten(color, 0.3f);
                if (Mathf.Abs(dy) >= Mathf.Abs(dx)) return dy > 0 ? PixelCanvas.Lighten(color, 0.5f) : PixelCanvas.Darken(color, 0.3f);
                return dx < 0 ? PixelCanvas.Lighten(color, 0.12f) : PixelCanvas.Darken(color, 0.12f);
            });
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, s * 0.42f, s * 0.6f, 5f, 2.5f), new Color(1, 1, 1, 0.9f));
        }

        private static void DrawSprinkles(PixelCanvas c, int s)
        {
            var rng = new System.Random(1234);
            for (int i = 0; i < 7; i++)
            {
                float x = 14 + (float)rng.NextDouble() * (s - 28);
                float y = 12 + (float)rng.NextDouble() * (s - 30);
                float a = (float)rng.NextDouble() * Mathf.PI;
                float dx = Mathf.Cos(a) * 4f, dy = Mathf.Sin(a) * 4f;
                var col = ColorUtil.Rainbow[i % ColorUtil.Rainbow.Length];
                if (i % 3 == 0) col = Color.white;
                c.Paint((px, py) => PixelCanvas.Segment(px, py, x - dx, y - dy, x + dx, y + dy, 3.4f), col);
            }
        }

        /// <summary>Little cute face (eyes + smile) centred at cx, cy.</summary>
        private static void DrawFace(PixelCanvas c, float cx, float cy, float scale)
        {
            float eyeDx = 9f * scale, eyeR = 5f * scale, pupilR = 2.8f * scale;
            foreach (var sx in new[] { -1f, 1f })
            {
                float ex = cx + sx * eyeDx;
                c.Paint((x, y) => PixelCanvas.Circle(x, y, ex, cy + 3f * scale, eyeR), Color.white);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, ex + 0.8f * scale, cy + 2f * scale, pupilR), ColorUtil.FaceInk);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, ex + 1.8f * scale, cy + 3.5f * scale, 1.1f * scale), Color.white);
            }
            // Smile: lower half of a ring
            c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, cx, cy - 2f * scale, 5f * scale)) - 1.3f * scale, y - (cy - 3f * scale)), ColorUtil.FaceInk);
            // Blush
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx - 15f * scale, cy - 4f * scale, 3.5f * scale, 2f * scale), new Color(1f, 0.45f, 0.6f, 0.55f));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx + 15f * scale, cy - 4f * scale, 3.5f * scale, 2f * scale), new Color(1f, 0.45f, 0.6f, 0.55f));
        }

        // ================================================================== generic shapes

        public static Sprite Circle() => Cached("circle", () =>
        {
            var c = new PixelCanvas(128, 128);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 64, 62), Color.white);
            return c.ToSprite(128);
        });

        /// <summary>Soft round particle with radial falloff.</summary>
        public static Sprite SoftDot() => Cached("softdot", () =>
        {
            var c = new PixelCanvas(64, 64);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 32, 32, 30), (x, y, d) => new Color(1, 1, 1, Mathf.Clamp01(-d / 30f) * 0.9f + 0.1f), 2f);
            return c.ToSprite(64);
        });

        public static Sprite Confetti() => Cached("confetti", () =>
        {
            var c = new PixelCanvas(16, 32);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 8, 16, 7, 15, 3), Color.white);
            return c.ToSprite(64);
        });

        /// <summary>White rounded rectangle, 9-sliced, for all UI panels and buttons.</summary>
        public static Sprite RoundedRect() => Cached("roundrect", () =>
        {
            const int size = 96, r = 40;
            var c = new PixelCanvas(size, size);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, size / 2f, size / 2f, size / 2f - 1, size / 2f - 1, r), Color.white);
            return c.ToSprite(100, new Vector4(r + 2, r + 2, r + 2, r + 2));
        });

        /// <summary>White rounded outline, 9-sliced, for neon borders.</summary>
        public static Sprite RoundedRing() => Cached("roundring", () =>
        {
            const int size = 96, r = 40;
            const float thickness = 5f;
            var c = new PixelCanvas(size, size);
            c.Paint((x, y) => Mathf.Abs(PixelCanvas.RoundedBox(x, y, size / 2f, size / 2f, size / 2f - 4, size / 2f - 4, r - 3) + thickness / 2f) - thickness / 2f, Color.white);
            return c.ToSprite(100, new Vector4(r + 2, r + 2, r + 2, r + 2));
        });

        /// <summary>
        /// Soft glow halo, 9-sliced and hollow (transparent inside), so it can sit on top of a card and only light
        /// the area outside its edge. The glow starts 30 px (0.3 units) inside the sprite's edge.
        /// </summary>
        public static Sprite Glow() => Cached("glow", () =>
        {
            const int size = 128, pad = 30, r = 36;
            var c = new PixelCanvas(size, size);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, size / 2f, size / 2f, size / 2f - pad, size / 2f - pad, r) - pad,
                (x, y, d) =>
                {
                    float inner = PixelCanvas.RoundedBox(x, y, size / 2f, size / 2f, size / 2f - pad, size / 2f - pad, r);
                    if (inner < 0f) return new Color(1, 1, 1, 0f);
                    float k = Mathf.Clamp01(1f - inner / pad);
                    return new Color(1, 1, 1, k * k * 0.9f);
                }, 2f);
            return c.ToSprite(100, new Vector4(pad + r + 2, pad + r + 2, pad + r + 2, pad + r + 2));
        });

        /// <summary>Dull junk block for rows added by Hard / Insane difficulty.</summary>
        public static Sprite JunkBlock() => Cached("junk", () =>
        {
            int s = BlockPixels;
            var c = new PixelCanvas(s, s);
            var baseCol = ColorUtil.Junk;
            System.Func<float, float, float> body = (x, y) => PixelCanvas.RoundedBox(x, y, s / 2f, s / 2f, s / 2f - 2f, s / 2f - 2f, 10f);
            c.Paint(body, (x, y, d) => Color.Lerp(PixelCanvas.Darken(baseCol, 0.2f), PixelCanvas.Lighten(baseCol, 0.1f), y / s));
            c.Paint((x, y) => Mathf.Abs(body(x, y) + 3f) - 1.5f, PixelCanvas.Darken(baseCol, 0.35f));
            // Diagonal hazard stripes
            c.Paint((x, y) => Mathf.Max(body(x, y) + 6f, Mathf.Abs(((x + y) % 18f) - 9f) - 3f), new Color(0f, 0f, 0f, 0.18f));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, s * 0.3f, s * 0.7f, 3f), new Color(1, 1, 1, 0.35f));
            return c.ToSprite(BlockPpu);
        });

        /// <summary>Synthwave perspective grid (transparent), drawn at the bottom of the background.</summary>
        public static Sprite NeonGrid(Color color) => Cached("grid_" + ColorUtility.ToHtmlStringRGB(color), () =>
        {
            const int w = 512, h = 256;
            var c = new PixelCanvas(w, h);
            float horizon = h - 8f;
            // Horizontal lines get closer together towards the horizon.
            for (int i = 1; i < 14; i++)
            {
                float t = i / 14f;
                float y = horizon * (1f - Mathf.Pow(t, 0.45f));
                float fade = 1f - t * 0.7f;
                c.Paint((px, py) => Mathf.Abs(py - y) - 1.1f, PixelCanvas.WithAlpha(color, fade));
            }
            // Verticals converge to the vanishing point.
            for (int i = -12; i <= 12; i++)
            {
                float bx = w / 2f + i * 46f;
                float tx = w / 2f + i * 7f;
                c.Paint((px, py) => PixelCanvas.Segment(px, py, bx, 0f, tx, horizon, 2.2f), PixelCanvas.WithAlpha(color, 0.85f));
            }
            // Fade towards the horizon.
            for (int y = 0; y < h; y++)
            {
                float fade = Mathf.Clamp01(1f - y / horizon);
                for (int x = 0; x < w; x++)
                {
                    var p = c.Get(x, y);
                    p.a *= fade;
                    c.Set(x, y, p);
                }
            }
            return c.ToSprite(64);
        });

        public static Sprite Triangle() => Cached("triangle", () =>
        {
            var c = new PixelCanvas(64, 64);
            var pts = new List<Vector2> { new Vector2(32, 6), new Vector2(60, 58), new Vector2(4, 58) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts) - 2f, Color.white);
            return c.ToSprite(64);
        });

        /// <summary>Vertical gradient used for the background.</summary>
        public static Sprite Gradient(Color top, Color bottom) =>
            Cached($"grad_{ColorUtility.ToHtmlStringRGB(top)}_{ColorUtility.ToHtmlStringRGB(bottom)}", () =>
            {
                var c = new PixelCanvas(4, 256);
                for (int y = 0; y < 256; y++)
                {
                    float t = y / 255f;
                    t = t * t * (3f - 2f * t);
                    var col = Color.Lerp(bottom, top, t);
                    for (int x = 0; x < 4; x++) c.Set(x, y, col);
                }
                return c.ToSprite(4);
            });

        // ================================================================== icons

        public static Sprite Coin() => Cached("coin", () =>
        {
            var c = new PixelCanvas(96, 96);
            var rim = ColorUtil.Hex("#E89B0C");
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 48, 46, 44), rim);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 48, 50, 40), (x, y, d) => Color.Lerp(ColorUtil.Hex("#FFB21E"), ColorUtil.Hex("#FFE266"), y / 96f));
            c.Paint((x, y) => Mathf.Abs(PixelCanvas.Circle(x, y, 48, 50, 29)) - 2.2f, ColorUtil.Hex("#F0A010"));
            var star = PixelCanvas.StarPoints(48, 50, 18, 8, 5);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, star), ColorUtil.Hex("#FFF4C2"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 32, 72, 9, 4), new Color(1, 1, 1, 0.75f));
            return c.ToSprite(96);
        });

        public static Sprite Shard() => Cached("shard", () =>
        {
            var c = new PixelCanvas(96, 96);
            var outer = new List<Vector2> { new Vector2(48, 90), new Vector2(84, 54), new Vector2(48, 6), new Vector2(12, 54) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, outer), (x, y, d) =>
            {
                var baseCol = ColorUtil.Hex("#2EE6C5");
                if (y > 54) return x < 48 ? PixelCanvas.Lighten(baseCol, 0.5f) : PixelCanvas.Lighten(baseCol, 0.25f);
                return x < 48 ? baseCol : PixelCanvas.Darken(baseCol, 0.2f);
            });
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 36, 66, 6, 3), new Color(1, 1, 1, 0.8f));
            return c.ToSprite(96);
        });

        public static Sprite Sparkle() => Cached("sparkle", () =>
        {
            var c = new PixelCanvas(64, 64);
            var star = PixelCanvas.StarPoints(32, 32, 30, 6, 4, 0f);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, star), Color.white);
            return c.ToSprite(64);
        });

        public static Sprite Star() => Cached("star", () =>
        {
            var c = new PixelCanvas(96, 96);
            var star = PixelCanvas.StarPoints(48, 46, 44, 20, 5);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, star) - 3f, Color.white);
            return c.ToSprite(96);
        });

        public static Sprite PowerUpIcon(PowerUpType type) => Cached("pu_" + type, () =>
        {
            var c = new PixelCanvas(128, 128);
            switch (type)
            {
                case PowerUpType.Bomb:
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 58, 54, 40), (x, y, d) => Color.Lerp(ColorUtil.Hex("#2B1B3D"), ColorUtil.Hex("#6E4A8E"), Mathf.Clamp01((y - 20f) / 80f)));
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 44, 74, 12, 7), new Color(1, 1, 1, 0.55f));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 82, 92, 9, 7, 3), ColorUtil.Hex("#8B6FA8"));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 86, 98, 100, 112, 5), ColorUtil.Hex("#C9A27E"));
                    var spark = PixelCanvas.StarPoints(104, 114, 14, 5, 6);
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, spark), ColorUtil.Yellow);
                    break;
                case PowerUpType.LineZap:
                    var bolt = new List<Vector2>
                    {
                        new Vector2(74, 122), new Vector2(30, 60), new Vector2(60, 60),
                        new Vector2(46, 6), new Vector2(100, 76), new Vector2(68, 76)
                    };
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, bolt) - 4f, ColorUtil.Hex("#E08A00"));
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, bolt), (x, y, d) => Color.Lerp(ColorUtil.Hex("#FFC400"), ColorUtil.Hex("#FFF27A"), y / 128f));
                    break;
                case PowerUpType.SlowMo:
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 62, 52), ColorUtil.Hex("#2E86DE"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 62, 42), Color.white);
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 62, 64, 92, 8), ColorUtil.FaceInk);
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 62, 84, 50, 7), ColorUtil.FaceInk);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 62, 6), ColorUtil.Pink);
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 120, 12, 6, 3), ColorUtil.Hex("#2E86DE"));
                    break;
                case PowerUpType.Swap:
                    for (int i = 0; i < 4; i++)
                    {
                        float cy = 22 + i * 28;
                        c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, cy, 15, 13, 5), (x, y, d) =>
                            Color.Lerp(ColorUtil.Hex("#1EB7E6"), ColorUtil.Hex("#8BE9FF"), Mathf.Clamp01((y - cy + 13f) / 26f)));
                    }
                    var arrowL = new List<Vector2> { new Vector2(14, 64), new Vector2(34, 84), new Vector2(34, 44) };
                    var arrowR = new List<Vector2> { new Vector2(114, 64), new Vector2(94, 84), new Vector2(94, 44) };
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, arrowL), ColorUtil.Pink);
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, arrowR), ColorUtil.Pink);
                    break;
            }
            return c.ToSprite(128);
        });

        public static Sprite PauseIcon() => Cached("icon_pause", () =>
        {
            var c = new PixelCanvas(96, 96);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 34, 48, 10, 32, 7), Color.white);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 62, 48, 10, 32, 7), Color.white);
            return c.ToSprite(96);
        });

        public static Sprite PlayIcon() => Cached("icon_play", () =>
        {
            var c = new PixelCanvas(96, 96);
            var tri = new List<Vector2> { new Vector2(30, 16), new Vector2(80, 48), new Vector2(30, 80) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, tri) - 5f, Color.white);
            return c.ToSprite(96);
        });

        public static Sprite GearIcon() => Cached("icon_gear", () =>
        {
            var c = new PixelCanvas(96, 96);
            c.Paint((x, y) =>
            {
                float dx = x - 48, dy = y - 48;
                float a = Mathf.Atan2(dy, dx);
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float teeth = 34f + 7f * Mathf.Clamp(Mathf.Sin(a * 8f) * 3f, -1f, 1f);
                return Mathf.Max(r - teeth, 14f - r);
            }, Color.white);
            return c.ToSprite(96);
        });

        public static Sprite CloseIcon() => Cached("icon_close", () =>
        {
            var c = new PixelCanvas(96, 96);
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Segment(x, y, 24, 24, 72, 72, 14), PixelCanvas.Segment(x, y, 24, 72, 72, 24, 14)), Color.white);
            return c.ToSprite(96);
        });

        public static Sprite CheckIcon() => Cached("icon_check", () =>
        {
            var c = new PixelCanvas(96, 96);
            c.Paint((x, y) => Mathf.Min(PixelCanvas.Segment(x, y, 18, 50, 40, 26, 14), PixelCanvas.Segment(x, y, 40, 26, 80, 72, 14)), Color.white);
            return c.ToSprite(96);
        });

        public static Sprite HomeIcon() => Cached("icon_home", () =>
        {
            var c = new PixelCanvas(96, 96);
            var roof = new List<Vector2> { new Vector2(48, 86), new Vector2(88, 48), new Vector2(8, 48) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, roof) - 3f, Color.white);
            c.Paint((x, y) => Mathf.Max(PixelCanvas.RoundedBox(x, y, 48, 32, 28, 24, 4), -PixelCanvas.RoundedBox(x, y, 48, 22, 8, 14, 3)), Color.white);
            return c.ToSprite(96);
        });

        public static Sprite AdIcon() => Cached("icon_ad", () =>
        {
            var c = new PixelCanvas(96, 96);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 48, 48, 42, 32, 10), Color.white);
            var tri = new List<Vector2> { new Vector2(38, 30), new Vector2(66, 48), new Vector2(38, 66) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, tri) - 2f, ColorUtil.Pink);
            return c.ToSprite(96);
        });

        public static Sprite LockIcon() => Cached("icon_lock", () =>
        {
            var c = new PixelCanvas(96, 96);
            c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, 48, 56, 20)) - 6f, 50f - y), Color.white);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 48, 34, 32, 26, 8), Color.white);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 48, 38, 7), ColorUtil.FaceInk);
            return c.ToSprite(96);
        });

        // ================================================================== avatars

        public static Sprite Avatar(ShopItem avatar, AvatarMood mood) =>
            Cached($"avatar_{avatar.Id}_{mood}", () => DrawAvatar(ColorUtil.Hex(avatar.BodyColor), avatar.AvatarShape, mood));

        private static Sprite DrawAvatar(Color body, int shape, AvatarMood mood)
        {
            const int s = 256;
            var c = new PixelCanvas(s, s);
            float cx = 128, cy = 112;

            System.Func<float, float, float> sdf;
            switch (shape)
            {
                case 1: // drop
                    var tip = new List<Vector2> { new Vector2(128, 236), new Vector2(190, 130), new Vector2(66, 130) };
                    sdf = (x, y) => Mathf.Min(PixelCanvas.Circle(x, y, cx, cy, 86), PixelCanvas.Polygon(x, y, tip) - 2f);
                    break;
                case 2: // marshmallow square
                    sdf = (x, y) => PixelCanvas.RoundedBox(x, y, cx, cy + 6, 92, 88, 46);
                    break;
                case 3: // star
                    var star = PixelCanvas.StarPoints(cx, cy + 10, 118, 62, 5);
                    sdf = (x, y) => PixelCanvas.Polygon(x, y, star) - 12f;
                    break;
                default: // round blob (also used under the crown)
                    sdf = (x, y) => PixelCanvas.Ellipse(x, y, cx, cy, 100, 92);
                    break;
            }

            var outline = PixelCanvas.Darken(body, 0.35f);
            // Shadow
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx, 18, 80, 12), new Color(0, 0, 0, 0.15f), 4f);
            c.Paint(sdf, (x, y, d) =>
            {
                var fill = Color.Lerp(PixelCanvas.Darken(body, 0.1f), PixelCanvas.Lighten(body, 0.25f), y / s);
                return Color.Lerp(fill, outline, Mathf.Clamp01((d + 7f) / 3f));
            });
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx - 44, cy + 50, 26, 14), new Color(1, 1, 1, 0.6f));

            if (shape == 4) DrawCrown(c, cx, cy + 74);

            DrawAvatarFace(c, cx, cy - 4, mood);
            return c.ToSprite(s);
        }

        private static void DrawCrown(PixelCanvas c, float cx, float baseY)
        {
            var crown = new List<Vector2>
            {
                new Vector2(cx - 54, baseY), new Vector2(cx - 62, baseY + 52), new Vector2(cx - 28, baseY + 28),
                new Vector2(cx, baseY + 60), new Vector2(cx + 28, baseY + 28), new Vector2(cx + 62, baseY + 52),
                new Vector2(cx + 54, baseY)
            };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, crown) - 3f, ColorUtil.Hex("#D99A00"));
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, crown), (x, y, d) => Color.Lerp(ColorUtil.Gold, ColorUtil.Hex("#FFF0A0"), (y - baseY) / 60f));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, cx, baseY + 22, 9), ColorUtil.Red);
        }

        private static void DrawAvatarFace(PixelCanvas c, float cx, float cy, AvatarMood mood)
        {
            float eyeDx = 36f;
            var ink = ColorUtil.FaceInk;
            foreach (var sx in new[] { -1f, 1f })
            {
                float ex = cx + sx * eyeDx, ey = cy + 18f;
                switch (mood)
                {
                    case AvatarMood.Happy:
                    case AvatarMood.Excited:
                        // ^ ^ eyes: upper half of a ring
                        c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, ex, ey - 4f, 14f)) - 4f, (ey - 4f) - y), ink);
                        break;
                    case AvatarMood.Shocked:
                        c.Paint((x, y) => PixelCanvas.Circle(x, y, ex, ey, 18f), Color.white);
                        c.Paint((x, y) => PixelCanvas.Circle(x, y, ex, ey, 9f), ink);
                        break;
                    case AvatarMood.Sad:
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, ex, ey - 2f, 10f, 13f), ink);
                        c.Paint((x, y) => PixelCanvas.Segment(x, y, ex - sx * 14f, ey + 22f, ex + sx * 10f, ey + 16f, 5f), ink);
                        break;
                    default:
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, ex, ey, 11f, 15f), ink);
                        c.Paint((x, y) => PixelCanvas.Circle(x, y, ex + 4f, ey + 6f, 4.5f), Color.white);
                        break;
                }
                // Blush
                c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx + sx * 62f, cy - 8f, 15f, 8f), new Color(1f, 0.4f, 0.55f, 0.5f));
            }

            switch (mood)
            {
                case AvatarMood.Excited:
                    // Big open smile
                    c.Paint((x, y) => Mathf.Max(PixelCanvas.Circle(x, y, cx, cy - 6f, 22f), y - (cy - 6f)), ink);
                    c.Paint((x, y) => Mathf.Max(PixelCanvas.Circle(x, y, cx, cy - 22f, 10f), y - (cy - 6f)), ColorUtil.Hex("#FF6F91"));
                    break;
                case AvatarMood.Shocked:
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, cx, cy - 18f, 11f, 15f), ink);
                    break;
                case AvatarMood.Sad:
                    c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, cx, cy - 34f, 18f)) - 3.5f, (cy - 34f) - y), ink);
                    break;
                default:
                    c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, cx, cy - 4f, 18f)) - 4f, y - (cy - 4f)), ink);
                    break;
            }
        }
    }
}
