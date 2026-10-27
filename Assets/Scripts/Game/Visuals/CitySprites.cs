using System.Collections.Generic;
using JellyDrop.Core.City;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// Procedural art for the city: one cute building per type, and icons for the three supplies.
    /// Coordinates are pixels, origin bottom-left, y up, on a 256 x 256 canvas.
    /// Swap any of these for hand-drawn sprites later by returning a loaded Sprite instead.
    /// </summary>
    public static class CitySprites
    {
        private const int S = 256;
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private static Color H(string hex) => ColorUtil.Hex(hex);
        private static Color Dark(Color c, float a = 0.28f) => PixelCanvas.Darken(c, a);
        private static Color Light(Color c, float a = 0.3f) => PixelCanvas.Lighten(c, a);

        public static Sprite Building(string id)
        {
            if (Cache.TryGetValue("b_" + id, out var cached) && cached != null) return cached;
            var c = new PixelCanvas(S, S);
            Shadow(c, 128, 26, 104);
            switch (id)
            {
                case "house": House(c, 128, H("#FFF1DC"), H("#FF7A8A")); break;
                case "sawmill": Sawmill(c); break;
                case "bakery": Bakery(c); break;
                case "brickworks": Brickworks(c); break;
                case "park": Park(c); break;
                case "windmill": Windmill(c); break;
                case "glassworks": Glassworks(c); break;
                case "market": MarketHall(c); break;
                case "tower": Tower(c); break;
                case "castle": Castle(c); break;
                default: House(c, 128, H("#EEE"), H("#AAA")); break;
            }
            var sprite = c.ToSprite(S);
            sprite.name = "building_" + id;
            Cache["b_" + id] = sprite;
            return sprite;
        }

        public static Sprite SupplyIcon(Supply s)
        {
            string key = "s_" + s;
            if (Cache.TryGetValue(key, out var cached) && cached != null) return cached;
            var c = new PixelCanvas(96, 96);
            switch (s)
            {
                case Supply.Wood:
                    var bark = H("#B9773E");
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 44, 44, 34, 20, 18), Dark(bark, 0.2f));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 44, 46, 32, 17, 16), bark);
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 20, 52, 58, 52, 3), Light(bark, 0.25f));
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 76, 45, 16, 19), H("#F4C98F"));
                    c.Paint((x, y) => Mathf.Abs(PixelCanvas.Ellipse(x, y, 76, 45, 10, 12)) - 1.6f, H("#D49A5C"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 76, 45, 3), H("#D49A5C"));
                    break;
                case Supply.Brick:
                    var brick = H("#F07A5A");
                    var top = new List<Vector2> { new Vector2(14, 58), new Vector2(66, 58), new Vector2(84, 72), new Vector2(32, 72) };
                    var side = new List<Vector2> { new Vector2(66, 26), new Vector2(84, 40), new Vector2(84, 72), new Vector2(66, 58) };
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 40, 42, 26, 16, 4), brick);
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, top), Light(brick, 0.25f));
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, side), Dark(brick, 0.2f));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 16, 42, 64, 42, 2.5f), Light(brick, 0.45f));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 40, 28, 40, 42, 2.5f), Light(brick, 0.45f));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 26, 42, 26, 56, 2.5f), Light(brick, 0.45f));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 54, 42, 54, 56, 2.5f), Light(brick, 0.45f));
                    break;
                default:
                    var glass = H("#8FD8FF");
                    var gem = new List<Vector2> { new Vector2(48, 88), new Vector2(84, 54), new Vector2(48, 8), new Vector2(12, 54) };
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, gem), (x, y, d) => y > 54 ? Light(glass, x < 48 ? 0.45f : 0.2f) : (x < 48 ? glass : Dark(glass, 0.15f)));
                    c.Paint((x, y) => Mathf.Abs(PixelCanvas.Polygon(x, y, gem)) - 1.5f, Dark(glass, 0.3f));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 30, 64, 40, 74, 5), new Color(1, 1, 1, 0.85f));
                    break;
            }
            var sprite = c.ToSprite(96);
            Cache[key] = sprite;
            return sprite;
        }

        // ------------------------------------------------------------------ pieces

        private static void Shadow(PixelCanvas c, float cx, float y, float rx) =>
            c.Paint((x, yy) => PixelCanvas.Ellipse(x, yy, cx, y, rx, 14), new Color(0.17f, 0.18f, 0.36f, 0.14f), 6f);

        /// <summary>Rectangle with a darker outline (bottom-left x0,y0 to x1,y1).</summary>
        private static void Block(PixelCanvas c, float x0, float y0, float x1, float y1, Color fill, float radius = 6f, bool shadeTop = true)
        {
            float cx = (x0 + x1) / 2f, cy = (y0 + y1) / 2f, hw = (x1 - x0) / 2f, hh = (y1 - y0) / 2f;
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, hw, hh, radius), Dark(fill, 0.25f));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, hw - 3, hh - 3, Mathf.Max(0, radius - 3)),
                (x, y, d) => shadeTop ? Color.Lerp(Dark(fill, 0.06f), Light(fill, 0.12f), Mathf.InverseLerp(y0, y1, y)) : fill);
        }

        private static void Roof(PixelCanvas c, float left, float right, float baseY, float peakY, Color color, float peakX = float.NaN)
        {
            if (float.IsNaN(peakX)) peakX = (left + right) / 2f;
            var pts = new List<Vector2> { new Vector2(left, baseY), new Vector2(right, baseY), new Vector2(peakX, peakY) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts) - 4f, Dark(color, 0.25f));
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts), (x, y, d) => Color.Lerp(color, Light(color, 0.2f), Mathf.InverseLerp(baseY, peakY, y)));
        }

        private static void Window(PixelCanvas c, float cx, float cy, float w, float h, bool round = false)
        {
            var frame = H("#FFFFFF");
            var pane = H("#BDE7FF");
            if (round)
            {
                c.Paint((x, y) => PixelCanvas.Circle(x, y, cx, cy, w / 2f + 3), frame);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, cx, cy, w / 2f - 1), pane);
            }
            else
            {
                c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, w / 2f + 3, h / 2f + 3, 4), frame);
                c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, cx, cy, w / 2f - 1, h / 2f - 1, 3), pane);
                c.Paint((x, y) => PixelCanvas.Segment(x, y, cx, cy - h / 2f, cx, cy + h / 2f, 2.5f), frame);
                c.Paint((x, y) => PixelCanvas.Segment(x, y, cx - w / 2f, cy, cx + w / 2f, cy, 2.5f), frame);
            }
            c.Paint((x, y) => PixelCanvas.Segment(x, y, cx - w * 0.3f, cy + h * 0.1f, cx - w * 0.1f, cy + h * 0.32f, 3), new Color(1, 1, 1, 0.7f));
        }

        private static void Door(PixelCanvas c, float cx, float bottom, float w, float h, Color color)
        {
            c.Paint((x, y) => Mathf.Min(PixelCanvas.RoundedBox(x, y, cx, bottom + (h - w / 2f) / 2f, w / 2f, (h - w / 2f) / 2f, 0),
                PixelCanvas.Circle(x, y, cx, bottom + h - w / 2f, w / 2f)) , Dark(color, 0.25f));
            c.Paint((x, y) => Mathf.Min(PixelCanvas.RoundedBox(x, y, cx, bottom + (h - w / 2f) / 2f, w / 2f - 3, (h - w / 2f) / 2f, 0),
                PixelCanvas.Circle(x, y, cx, bottom + h - w / 2f, w / 2f - 3)), color);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, cx + w * 0.25f, bottom + h * 0.4f, 3), H("#FFD25A"));
        }

        private static void Smoke(PixelCanvas c, float x0, float y0)
        {
            c.Paint((x, y) => PixelCanvas.Circle(x, y, x0, y0, 10), new Color(1, 1, 1, 0.85f));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, x0 + 12, y0 + 14, 13), new Color(1, 1, 1, 0.75f));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, x0 + 4, y0 + 30, 9), new Color(1, 1, 1, 0.6f));
        }

        private static void Flag(PixelCanvas c, float x0, float y0, Color color)
        {
            c.Paint((x, y) => PixelCanvas.Segment(x, y, x0, y0, x0, y0 + 34, 3.5f), H("#7C82B3"));
            var f = new List<Vector2> { new Vector2(x0 + 1, y0 + 34), new Vector2(x0 + 28, y0 + 27), new Vector2(x0 + 1, y0 + 19) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, f), color);
        }

        private static void Stripes(PixelCanvas c, float x0, float x1, float yBottom, float yTop, Color a, Color b, int count)
        {
            float w = (x1 - x0) / count;
            for (int i = 0; i < count; i++)
            {
                float l = x0 + i * w;
                var col = i % 2 == 0 ? a : b;
                var pts = new List<Vector2> { new Vector2(l, yBottom), new Vector2(l + w, yBottom), new Vector2(l + w * 0.85f, yTop), new Vector2(l + w * 0.15f, yTop) };
                c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts), col);
            }
            for (int i = 0; i < count; i++)
            {
                float cx = x0 + (i + 0.5f) * w;
                c.Paint((x, y) => Mathf.Max(PixelCanvas.Circle(x, y, cx, yBottom, w / 2f), y - yBottom), i % 2 == 0 ? a : b);
            }
        }

        // ------------------------------------------------------------------ buildings

        private static void House(PixelCanvas c, float cx, Color wall, Color roof)
        {
            Block(c, cx - 70, 30, cx + 70, 128, wall);
            Block(c, cx + 34, 140, cx + 54, 190, H("#C97B63"), 3);
            Roof(c, cx - 88, cx + 88, 120, 206, roof);
            Door(c, cx - 30, 30, 34, 64, H("#B9773E"));
            Window(c, cx + 30, 82, 38, 34);
            Window(c, cx, 160, 26, 26, round: true);
        }

        private static void Sawmill(PixelCanvas c)
        {
            var wood = H("#E0A86A");
            Block(c, 50, 30, 190, 120, wood);
            for (int i = 0; i < 4; i++) { float y0 = 52 + i * 18; c.Paint((x, y) => PixelCanvas.Segment(x, y, 56, y0, 184, y0, 2.5f), Dark(wood, 0.18f)); }
            Roof(c, 34, 206, 112, 180, H("#6FBF73"));
            Door(c, 150, 30, 40, 66, H("#8A5A2E"));
            Window(c, 92, 80, 36, 30);
            // Saw blade
            var blade = H("#C9CDE6");
            var teeth = PixelCanvas.StarPoints(214, 92, 34, 26, 12);
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, teeth), blade);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 214, 92, 22), Light(blade, 0.3f));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 214, 92, 6), H("#7C82B3"));
            // Logs
            var log = H("#B9773E");
            foreach (var p in new[] { new Vector2(36, 42), new Vector2(60, 42), new Vector2(48, 62) })
            {
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x, p.y, 13), Dark(log, 0.15f));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x, p.y, 10), H("#F4C98F"));
                c.Paint((x, y) => Mathf.Abs(PixelCanvas.Circle(x, y, p.x, p.y, 5)) - 1.2f, H("#D49A5C"));
            }
        }

        private static void Bakery(PixelCanvas c)
        {
            Block(c, 46, 30, 210, 130, H("#FFE3EC"));
            Roof(c, 30, 226, 122, 196, H("#B48CFF"));
            Block(c, 166, 150, 186, 196, H("#E8735A"), 3);
            Smoke(c, 176, 204);
            Stripes(c, 60, 196, 108, 128, H("#FF6F91"), H("#FFFFFF"), 7);
            Window(c, 92, 70, 44, 40);
            Door(c, 162, 30, 38, 66, H("#C97B63"));
            // Bread sign
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 128, 160, 22), H("#FFFFFF"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 128, 160, 15, 9), H("#E9A55B"));
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 121, 155, 125, 165, 2), H("#FFE0A8"));
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 131, 155, 135, 165, 2), H("#FFE0A8"));
        }

        private static void Brickworks(PixelCanvas c)
        {
            var brick = H("#F07A5A");
            Block(c, 40, 30, 180, 118, brick);
            for (int row = 0; row < 5; row++)
            {
                float y0 = 46 + row * 16;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, 46, y0, 174, y0, 2), Light(brick, 0.35f));
                for (int k = 0; k < 6; k++)
                {
                    float x0 = 58 + k * 22 + (row % 2) * 11;
                    if (x0 > 170) continue;
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, x0, y0, x0, y0 + 16, 2), Light(brick, 0.35f));
                }
            }
            Roof(c, 26, 194, 112, 160, H("#A8AFCB"));
            Block(c, 186, 30, 218, 200, Dark(brick, 0.1f), 6);
            Smoke(c, 204, 208);
            Door(c, 110, 30, 40, 60, H("#8A5A2E"));
            Window(c, 66, 72, 30, 28);
            Window(c, 154, 72, 30, 28);
        }

        private static void Park(PixelCanvas c)
        {
            var grass = H("#7FD88A");
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 128, 52, 112, 30), grass);
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 128, 52, 60, 14), H("#F3E3C3"));
            // Trees
            foreach (var t in new[] { new Vector3(62, 60, 1f), new Vector3(190, 64, 0.85f) })
            {
                float s = t.z;
                c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, t.x, t.y + 30 * s, 8 * s, 30 * s, 3), H("#9A6A3C"));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, t.x, t.y + 92 * s, 40 * s), H("#43B86A"));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, t.x - 18 * s, t.y + 74 * s, 30 * s), H("#4FC676"));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, t.x + 20 * s, t.y + 72 * s, 28 * s), H("#5ED184"));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, t.x - 10 * s, t.y + 104 * s, 9 * s), new Color(1, 1, 1, 0.35f));
            }
            // Fountain
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 128, 60, 36, 13), H("#C9CDE6"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 128, 62, 28, 8), H("#8FD8FF"));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 128, 82, 5, 20, 2), H("#C9CDE6"));
            c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, 128, 96, 18)) - 2.5f, 96 - y), H("#8FD8FF"));
            // Flowers
            var flowers = new[] { "#FF6F91", "#FFC93C", "#8E7CFF", "#FF9F43", "#FFFFFF" };
            for (int i = 0; i < 9; i++)
            {
                float fx = 40 + i * 22 + (i % 2) * 6, fy = 34 + (i * 13 % 26);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, fx, fy, 5), H(flowers[i % flowers.Length]));
            }
        }

        private static void Windmill(PixelCanvas c)
        {
            var body = H("#FFF1DC");
            var tower = new List<Vector2> { new Vector2(86, 30), new Vector2(170, 30), new Vector2(150, 150), new Vector2(106, 150) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, tower) - 3f, Dark(body, 0.25f));
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, tower), (x, y, d) => Color.Lerp(Dark(body, 0.05f), Light(body, 0.1f), y / 150f));
            Roof(c, 96, 160, 146, 190, H("#FF7A8A"));
            Door(c, 128, 30, 34, 56, H("#B9773E"));
            Window(c, 128, 112, 22, 22, round: true);
            // Blades
            var hub = new Vector2(128, 154);
            for (int i = 0; i < 4; i++)
            {
                float a = (45 + i * 90) * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                var n = new Vector2(-dir.y, dir.x);
                var p0 = hub + dir * 14;
                var p1 = hub + dir * 96;
                var blade = new List<Vector2> { p0 + n * 6, p1 + n * 6, p1 + n * 26, p0 + n * 20 };
                c.Paint((x, y) => PixelCanvas.Segment(x, y, hub.x, hub.y, p1.x, p1.y, 6), H("#9A6A3C"));
                c.Paint((x, y) => PixelCanvas.Polygon(x, y, blade) - 2f, H("#9A6A3C"));
                c.Paint((x, y) => PixelCanvas.Polygon(x, y, blade), new Color(1, 1, 1, 0.95f));
            }
            c.Paint((x, y) => PixelCanvas.Circle(x, y, hub.x, hub.y, 10), H("#FFC93C"));
        }

        private static void Glassworks(PixelCanvas c)
        {
            var wall = H("#D8E8FA");
            Block(c, 40, 30, 216, 120, wall);
            c.Paint((x, y) => Mathf.Max(PixelCanvas.Circle(x, y, 128, 118, 70), 118 - y) - 4f, Dark(H("#7FD3F7"), 0.2f));
            c.Paint((x, y) => Mathf.Max(PixelCanvas.Circle(x, y, 128, 118, 66), 118 - y), (x, y, d) => Color.Lerp(H("#7FD3F7"), H("#C7EEFF"), (y - 118) / 66f));
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 98, 150, 112, 170, 5), new Color(1, 1, 1, 0.75f));
            Block(c, 186, 120, 206, 196, H("#A8AFCB"), 3);
            Smoke(c, 196, 204);
            Window(c, 128, 70, 56, 56, round: true);
            Door(c, 72, 30, 34, 58, H("#6E9BFF"));
            Window(c, 182, 70, 26, 30);
        }

        private static void MarketHall(PixelCanvas c)
        {
            Block(c, 26, 30, 230, 118, H("#FFF6E0"));
            for (int i = 0; i < 4; i++)
            {
                float cx = 58 + i * 47;
                c.Paint((x, y) => Mathf.Min(PixelCanvas.RoundedBox(x, y, cx, 58, 16, 28, 0), PixelCanvas.Circle(x, y, cx, 86, 16)), H("#FFD8A8"));
                c.Paint((x, y) => Mathf.Min(PixelCanvas.RoundedBox(x, y, cx, 56, 12, 26, 0), PixelCanvas.Circle(x, y, cx, 82, 12)), H("#9A6A3C"));
            }
            Stripes(c, 26, 230, 118, 146, H("#FF9F43"), H("#FFFFFF"), 9);
            Roof(c, 16, 240, 144, 196, H("#FFC93C"));
            // Crates of goods
            Block(c, 34, 30, 64, 52, H("#C9925A"), 3);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 42, 56, 7), H("#FF5A6E"));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 56, 56, 7), H("#9BDB4D"));
            Block(c, 194, 30, 224, 52, H("#C9925A"), 3);
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 202, 56, 7), H("#FFC93C"));
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 216, 56, 7), H("#FF9F43"));
        }

        private static void Tower(PixelCanvas c)
        {
            var stone = H("#E1E4F2");
            Block(c, 92, 30, 164, 180, stone, 8);
            for (int i = 0; i < 6; i++)
            {
                float y0 = 54 + i * 22;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, 98, y0, 158, y0, 2), Dark(stone, 0.12f));
            }
            Roof(c, 80, 176, 174, 218, H("#6E9BFF"));
            Flag(c, 128, 212, H("#FF6F91"));
            Window(c, 128, 140, 22, 28);
            Window(c, 128, 96, 22, 28);
            Door(c, 128, 30, 30, 48, H("#8A5A2E"));
        }

        private static void Castle(PixelCanvas c)
        {
            var stone = H("#E6E3F5");
            // Main wall with battlements
            Block(c, 50, 30, 206, 130, stone, 4);
            for (int i = 0; i < 6; i++)
            {
                float x0 = 56 + i * 26;
                Block(c, x0, 124, x0 + 16, 144, stone, 2, shadeTop: false);
            }
            // Keep
            Block(c, 96, 100, 160, 190, Light(stone, 0.2f), 4);
            Roof(c, 88, 168, 186, 220, H("#B48CFF"));
            Flag(c, 128, 214, H("#FFC93C"));
            Window(c, 128, 150, 20, 26);
            // Side towers
            foreach (float tx in new[] { 44f, 212f })
            {
                Block(c, tx - 22, 30, tx + 22, 168, stone, 6);
                Roof(c, tx - 30, tx + 30, 162, 210, H("#8E7CFF"));
                Window(c, tx, 120, 14, 20);
            }
            Flag(c, 44, 204, H("#FF6F91"));
            // Gate
            c.Paint((x, y) => Mathf.Min(PixelCanvas.RoundedBox(x, y, 128, 52, 22, 22, 0), PixelCanvas.Circle(x, y, 128, 74, 22)), H("#8A5A2E"));
            for (int i = 0; i < 3; i++) { float gx = 116 + i * 12; c.Paint((x, y) => PixelCanvas.Segment(x, y, gx, 30, gx, 88, 2.5f), H("#5E3B1C")); }
        }
    }
}
