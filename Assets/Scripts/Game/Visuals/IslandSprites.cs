using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>Icons for the forest and garden islands: mushrooms, berries, honey, crops, eggs (drawn in code).</summary>
    public static class IslandSprites
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        private static Sprite Cached(string key, System.Action<PixelCanvas> draw)
        {
            if (Cache.TryGetValue(key, out var s) && s != null) return s;
            var c = new PixelCanvas(128, 128);
            draw(c);
            s = c.ToSprite(128);
            s.name = "isl_" + key;
            Cache[key] = s;
            return s;
        }

        /// <summary>A small island with a palm tree (buy an extra island).</summary>
        public static Sprite IslandIcon() => Cached("island", c =>
        {
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 30, 58, 18), ColorUtil.Hex("#2F78C2"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 36, 46, 16), ColorUtil.Hex("#E8C98A"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 42, 38, 12), ColorUtil.Hex("#4FA65A"));
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 70, 44, 62, 98, 5), ColorUtil.Hex("#8A5A34"));
            for (int i = 0; i < 5; i++)
            {
                float a = 0.3f + i * 0.62f;
                float ex = 62 + Mathf.Cos(a) * 34, ey = 98 + Mathf.Sin(a) * 14 - 6;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, 62, 98, ex, ey, 7), ColorUtil.Hex("#3E9A44"));
            }
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 100, 108, 16, 16, 6), ColorUtil.Hex("#3DDBA8"));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 100, 108, 10, 3, 1.5f), Color.white);
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 100, 108, 3, 10, 1.5f), Color.white);
        });

        /// <summary>Icon for a town decoration (CityCatalog.Decorations).</summary>
        public static Sprite Decor(string id) => Cached("decor_" + id, c =>
        {
            var stone = ColorUtil.Hex("#D9D2C3");
            var water = ColorUtil.Hex("#4FA8E8");
            var green = ColorUtil.Hex("#3E9A44");
            switch (id)
            {
                case "flowers":
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 30, 46, 14, 7), ColorUtil.Hex("#8A5A34"));
                    string[] cols = { "#FF5A6E", "#FFC93C", "#B48CFF", "#FFFFFF", "#FF9F43" };
                    for (int i = 0; i < 5; i++)
                    {
                        float fx = 30 + i * 17, fy = 60 + (i % 2) * 14;
                        c.Paint((x, y) => PixelCanvas.Segment(x, y, fx, 40, fx, fy, 3), green);
                        var col = ColorUtil.Hex(cols[i]);
                        c.Paint((x, y) => PixelCanvas.Circle(x, y, fx, fy, 10), col);
                        c.Paint((x, y) => PixelCanvas.Circle(x, y, fx, fy, 3.5f), ColorUtil.Hex("#FFE27A"));
                    }
                    break;
                case "lamp":
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 92, 30), new Color(1f, 0.9f, 0.5f, 0.35f));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 14, 18, 6, 3), ColorUtil.Hex("#3B3F5C"));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 14, 64, 80, 5), ColorUtil.Hex("#2B2F4A"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 90, 13, 13, 3), ColorUtil.Hex("#FFE9A8"));
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(46, 103), new Vector2(82, 103), new Vector2(64, 118) }), ColorUtil.Hex("#2B2F4A"));
                    break;
                case "bench":
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 70, 46, 9, 3), ColorUtil.Hex("#C98552"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 48, 48, 8, 3), ColorUtil.Hex("#A86F43"));
                    foreach (var lx in new[] { 26f, 102f }) c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, lx, 30, 5, 18, 2), ColorUtil.Hex("#3B3F5C"));
                    break;
                case "topiary":
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 20, 22, 14, 3), ColorUtil.Hex("#C9714A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 52, 22), ColorUtil.Hex("#2F7A3A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 84, 15), ColorUtil.Hex("#3E8F4C"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 106, 9), ColorUtil.Hex("#4FA65A"));
                    break;
                case "pond":
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 54, 56, 32), ColorUtil.Hex("#9EA3B5"));
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 56, 48, 25), water);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 80, 62, 11, 6), ColorUtil.Hex("#4FA65A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 80, 66, 4), ColorUtil.Hex("#FF8FC0"));
                    break;
                case "fountain":
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 30, 54, 16), stone);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 34, 44, 10), water);
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 58, 7, 24, 3), ColorUtil.Hex("#CFC7B5"));
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 82, 28, 8), stone);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 98, 10), ColorUtil.Hex("#BFE8FF"));
                    foreach (var dx in new[] { -1f, 1f }) c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 100, 64 + dx * 30, 60, 3), ColorUtil.Hex("#9FD9FF"));
                    break;
                case "statue":
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 22, 34, 16, 3), ColorUtil.Hex("#B8B2A6"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 62, 20, 24, 6), ColorUtil.Hex("#E0B23A"));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 86, 70, 96, 104, 6), ColorUtil.Hex("#E0B23A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 97, 108, 7), ColorUtil.Hex("#FFE27A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 57, 66, 3), ColorUtil.Hex("#5A4210"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 71, 66, 3), ColorUtil.Hex("#5A4210"));
                    break;
                case "gazebo":
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 16, 50, 7, 3), stone);
                    foreach (var px in new[] { 26f, 52f, 76f, 102f }) c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, px, 48, 4, 26, 1), ColorUtil.Hex("#F2EEE6"));
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(10, 76), new Vector2(118, 76), new Vector2(64, 116) }), ColorUtil.Hex("#3FA39A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 118, 5), ColorUtil.Hex("#FFC93C"));
                    break;
                default:
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 64, 40), green);
                    break;
            }
        });

        public static Sprite Mushroom(bool gold = false) => Cached("mush_" + gold, c =>
        {
            var cap = gold ? ColorUtil.Hex("#FFC93C") : ColorUtil.Hex("#E0483E");
            var capDark = gold ? ColorUtil.Hex("#D99A00") : ColorUtil.Hex("#A82A2A");
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 34, 18, 26, 12), (x, y, d) => Color.Lerp(ColorUtil.Hex("#E8DCC4"), ColorUtil.Hex("#FFF8EA"), (x - 46) / 36f));
            c.Paint((x, y) => Mathf.Max(PixelCanvas.Ellipse(x, y, 64, 64, 54, 46), 58 - y), (x, y, d) => Color.Lerp(capDark, cap, (y - 58) / 40f));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 60, 52, 6), capDark);
            foreach (var p in new[] { new Vector3(44, 82, 8), new Vector3(74, 94, 6), new Vector3(88, 74, 5), new Vector3(58, 102, 4) })
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x, p.y, p.z), Color.white);
            if (gold)
                foreach (var p in new[] { new Vector2(22, 104), new Vector2(108, 96) })
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, PixelCanvas.StarPoints(p.x, p.y, 13, 4, 4)), ColorUtil.Hex("#FFF4B0"));
        });

        public static Sprite Berries() => Cached("berries", c =>
        {
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 104, 26, 9), ColorUtil.Hex("#3E9A44"));
            foreach (var p in new[] { new Vector2(46, 70), new Vector2(80, 72), new Vector2(62, 48), new Vector2(40, 40), new Vector2(86, 44), new Vector2(64, 82) })
            {
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x, p.y, 17), (x, y, d) => Color.Lerp(ColorUtil.Hex("#3A2A8A"), ColorUtil.Hex("#6A5AE0"), (y - p.y + 17) / 34f));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x - 5, p.y + 6, 4), new Color(1, 1, 1, 0.6f));
            }
        });

        public static Sprite Honey() => Cached("honey", c =>
        {
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 54, 40, 40, 16), (x, y, d) => Color.Lerp(ColorUtil.Hex("#E08A00"), ColorUtil.Hex("#FFC93C"), y / 100f));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 100, 30, 9, 4), ColorUtil.Hex("#C98552"));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 56, 24, 14, 5), ColorUtil.Hex("#FFF2C4"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 42, 70, 6, 14), new Color(1, 1, 1, 0.45f));
        });

        public static Sprite Egg() => Cached("egg", c =>
        {
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 60, 38, 50), (x, y, d) => Color.Lerp(ColorUtil.Hex("#E8D8C0"), ColorUtil.Hex("#FFF8EE"), y / 110f));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 50, 80, 9, 14), new Color(1, 1, 1, 0.7f));
        });

        public static Sprite Seedling() => Cached("seedling", c =>
        {
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 22, 40, 12), ColorUtil.Hex("#7A5232"));
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 26, 64, 70, 5), ColorUtil.Hex("#3E9A44"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 44, 78, 22, 12), ColorUtil.Hex("#5CCB60"));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 84, 86, 22, 12), ColorUtil.Hex("#5CCB60"));
        });

        public static Sprite WateringCan() => Cached("water", c =>
        {
            var b = ColorUtil.Hex("#3FB6FF");
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 60, 50, 32, 28, 8), b);
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 88, 52, 116, 82, 6), b);
            c.Paint((x, y) => Mathf.Max(Mathf.Abs(PixelCanvas.Circle(x, y, 44, 82, 18)) - 5f, 80 - y), b);
            foreach (var p in new[] { new Vector2(118, 66), new Vector2(124, 56), new Vector2(112, 52) })
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x, p.y, 3.5f), ColorUtil.Hex("#9FE3FF"));
        });

        public static Sprite Axe() => Cached("axe", c =>
        {
            c.Paint((x, y) => PixelCanvas.Segment(x, y, 34, 18, 88, 108, 6), ColorUtil.Hex("#A86F43"));
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(72, 92), new Vector2(110, 116), new Vector2(118, 84), new Vector2(90, 74) }), ColorUtil.Hex("#C8CCDD"));
        });

        /// <summary>Crop icons for the garden (index = CropType).</summary>
        public static Sprite Crop(int type) => Cached("crop_" + type, c =>
        {
            switch (type)
            {
                case 0: // carrot
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(44, 92), new Vector2(84, 92), new Vector2(64, 14) }), (x, y, d) => Color.Lerp(ColorUtil.Hex("#E06A1A"), ColorUtil.Hex("#FF9F43"), y / 100f));
                    for (int i = -1; i <= 1; i++) { float dx = i * 12; c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64 + dx, 104, 7, 16), ColorUtil.Hex("#4FB85A")); }
                    break;
                case 1: // tomato
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 58, 42), (x, y, d) => Color.Lerp(ColorUtil.Hex("#C82A2A"), ColorUtil.Hex("#FF5A4E"), y / 110f));
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, PixelCanvas.StarPoints(64, 96, 18, 6, 5)), ColorUtil.Hex("#3E9A44"));
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 46, 74, 8, 12), new Color(1, 1, 1, 0.5f));
                    break;
                case 2: // strawberry
                    c.Paint((x, y) => Mathf.Min(PixelCanvas.Circle(x, y, 64, 70, 34), PixelCanvas.Polygon(x, y, new List<Vector2> { new Vector2(30, 70), new Vector2(98, 70), new Vector2(64, 14) })), ColorUtil.Hex("#E0304A"));
                    for (int i = 0; i < 9; i++) { float sx = 46 + (i % 3) * 18, sy = 40 + (i / 3) * 16; c.Paint((x, y) => PixelCanvas.Ellipse(x, y, sx, sy, 2, 3), ColorUtil.Hex("#FFE27A")); }
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, PixelCanvas.StarPoints(64, 100, 22, 8, 5)), ColorUtil.Hex("#3E9A44"));
                    break;
                case 3: // pumpkin
                    foreach (var dx in new[] { -22f, 22f, 0f })
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64 + dx, 54, 30, 38), (x, y, d) => Color.Lerp(ColorUtil.Hex("#D9631A"), ColorUtil.Hex("#FF9F43"), y / 100f));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 98, 6, 10, 3), ColorUtil.Hex("#5A7A2A"));
                    break;
                default: // sunflower
                    for (int i = 0; i < 12; i++)
                    {
                        float a = i * Mathf.PI / 6f;
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64 + Mathf.Cos(a) * 34, 70 + Mathf.Sin(a) * 34, 12, 12), ColorUtil.Hex("#FFC93C"));
                    }
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 70, 24), ColorUtil.Hex("#6B3A1A"));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 10, 64, 46, 5), ColorUtil.Hex("#3E9A44"));
                    break;
            }
        });

        /// <summary>A cooked dish from the kitchen (index = recipe).</summary>
        public static Sprite Dish(int recipe) => Cached("dish_" + recipe, c =>
        {
            Color[] soups = { ColorUtil.Hex("#C98552"), ColorUtil.Hex("#E0483E"), ColorUtil.Hex("#FF9F43"), ColorUtil.Hex("#E0304A"), ColorUtil.Hex("#FFC93C") };
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 44, 52, 30), Color.white);
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 50, 44, 18), soups[recipe % soups.Length]);
            for (int i = 0; i < 3; i++) { float sx = 44 + i * 20; c.Paint((x, y) => PixelCanvas.Segment(x, y, sx, 76, sx + 6, 108, 3), new Color(1, 1, 1, 0.6f)); }
        });
    }
}
