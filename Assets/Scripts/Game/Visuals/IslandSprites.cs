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
