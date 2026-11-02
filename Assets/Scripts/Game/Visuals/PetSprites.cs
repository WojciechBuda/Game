using System.Collections.Generic;
using JellyDrop.Core.Pet;
using UnityEngine;

namespace JellyDrop.Visuals
{
    /// <summary>Icons for the pet room: needs, foods, gifts and care actions (drawn in code, cached).</summary>
    public static class PetSprites
    {
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private const int S = 128;

        private static Sprite Cached(string key, System.Action<PixelCanvas> draw, int size = S)
        {
            if (Cache.TryGetValue(key, out var sp) && sp != null) return sp;
            var c = new PixelCanvas(size, size);
            draw(c);
            sp = c.ToSprite(size);
            sp.name = "pet_" + key;
            Cache[key] = sp;
            return sp;
        }

        public static Color NeedColor(PetNeed need)
        {
            switch (need)
            {
                case PetNeed.Food: return ColorUtil.Orange;
                case PetNeed.Energy: return ColorUtil.Yellow;
                case PetNeed.Fun: return ColorUtil.Pink;
                default: return ColorUtil.Blue;
            }
        }

        public static Sprite NeedIcon(PetNeed need)
        {
            switch (need)
            {
                case PetNeed.Food: return Item("apple");
                case PetNeed.Energy: return Bolt();
                case PetNeed.Fun: return SpriteFactory.Star();
                default: return Drop();
            }
        }

        public static Sprite Bolt() => Cached("bolt", c =>
        {
            var pts = new List<Vector2> { new Vector2(74, 122), new Vector2(30, 58), new Vector2(60, 58), new Vector2(48, 6), new Vector2(98, 74), new Vector2(66, 74) };
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts) - 2f, ColorUtil.Hex("#D99A00"));
            c.Paint((x, y) => PixelCanvas.Polygon(x, y, pts) + 2f, (x, y, d) => Color.Lerp(ColorUtil.Yellow, ColorUtil.Hex("#FFF3A6"), y / S));
        });

        public static Sprite Drop() => Cached("drop", c =>
        {
            var tip = new List<Vector2> { new Vector2(64, 124), new Vector2(98, 60), new Vector2(30, 60) };
            System.Func<float, float, float> sdf = (x, y) => Mathf.Min(PixelCanvas.Circle(x, y, 64, 50, 36), PixelCanvas.Polygon(x, y, tip));
            c.Paint(sdf, (x, y, d) => Color.Lerp(ColorUtil.Hex("#2A8FE0"), ColorUtil.Hex("#8FD8FF"), y / S));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 50, 56, 8, 14), new Color(1, 1, 1, 0.6f));
        });

        public static Sprite Moon() => Cached("moon", c =>
        {
            c.Paint((x, y) => Mathf.Max(PixelCanvas.Circle(x, y, 64, 64, 46), -PixelCanvas.Circle(x, y, 86, 80, 40)),
                (x, y, d) => Color.Lerp(ColorUtil.Hex("#FFC93C"), ColorUtil.Hex("#FFF0B0"), y / S));
            foreach (var p in new[] { new Vector2(100, 34), new Vector2(110, 60) })
                c.Paint((x, y) => PixelCanvas.Polygon(x, y, PixelCanvas.StarPoints(p.x, p.y, 10, 4, 4)), Color.white);
        });

        public static Sprite Sun() => Cached("sun", c =>
        {
            for (int k = 0; k < 8; k++)
            {
                float a = k * Mathf.PI / 4f;
                float x0 = 64 + Mathf.Cos(a) * 40, y0 = 64 + Mathf.Sin(a) * 40, x1 = 64 + Mathf.Cos(a) * 58, y1 = 64 + Mathf.Sin(a) * 58;
                c.Paint((x, y) => PixelCanvas.Segment(x, y, x0, y0, x1, y1, 6f), ColorUtil.Yellow);
            }
            c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 64, 32), (x, y, d) => Color.Lerp(ColorUtil.Orange, ColorUtil.Yellow, y / S));
        });

        public static Sprite Medicine() => Cached("medicine", c =>
        {
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 64, 54, 26, 26), (x, y, d) => x < 64 ? ColorUtil.Red : Color.white);
            c.Paint((x, y) => Mathf.Abs(PixelCanvas.RoundedBox(x, y, 64, 64, 54, 26, 26)) - 2.5f, ColorUtil.Hex("#C83A52"));
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 40, 76, 14, 5, 4), new Color(1, 1, 1, 0.55f));
        });

        public static Sprite Soap() => Cached("soap", c =>
        {
            c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 60, 46, 44, 26, 16), (x, y, d) => Color.Lerp(ColorUtil.Pink, ColorUtil.Hex("#FFC2D4"), y / 80f));
            foreach (var b in new[] { new Vector3(80, 92, 16), new Vector3(52, 98, 11), new Vector3(100, 112, 9), new Vector3(66, 118, 7) })
            {
                c.Paint((x, y) => Mathf.Abs(PixelCanvas.Circle(x, y, b.x, b.y, b.z)) - 2f, ColorUtil.Hex("#9FE3FF"));
                c.Paint((x, y) => PixelCanvas.Circle(x, y, b.x - b.z * 0.35f, b.y + b.z * 0.35f, b.z * 0.25f), Color.white);
            }
        });

        public static Sprite Heart() => Cached("heart", c =>
        {
            System.Func<float, float, float> sdf = (x, y) =>
            {
                var tri = new List<Vector2> { new Vector2(16, 76), new Vector2(112, 76), new Vector2(64, 14) };
                return Mathf.Min(Mathf.Min(PixelCanvas.Circle(x, y, 42, 82, 28), PixelCanvas.Circle(x, y, 86, 82, 28)), PixelCanvas.Polygon(x, y, tri));
            };
            c.Paint(sdf, (x, y, d) => Color.Lerp(ColorUtil.Red, ColorUtil.Hex("#FF9AB0"), y / S));
            c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 38, 92, 9, 6), new Color(1, 1, 1, 0.55f));
        });

        /// <summary>Food and gift icons.</summary>
        public static Sprite Item(string id)
        {
            switch (id)
            {
                case "mushroom_soup": return IslandSprites.Dish(0);
                case "berry_bowl": return IslandSprites.Berries();
                case "honey_toast": return IslandSprites.Honey();
            }
            return DrawItem(id);
        }

        private static Sprite DrawItem(string id) => Cached("item_" + id, c =>
        {
            switch (id)
            {
                case "apple":
                    c.Paint((x, y) => Mathf.Min(PixelCanvas.Circle(x, y, 50, 56, 36), PixelCanvas.Circle(x, y, 78, 56, 36)),
                        (x, y, d) => Color.Lerp(ColorUtil.Hex("#D9324A"), ColorUtil.Hex("#FF7A8A"), y / S));
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 64, 84, 68, 108, 4f), ColorUtil.Hex("#7A4A2A"));
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 84, 104, 16, 8), ColorUtil.Mint);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 40, 70, 7, 11), new Color(1, 1, 1, 0.5f));
                    break;
                case "sandwich":
                {
                    var bread = new List<Vector2> { new Vector2(14, 30), new Vector2(114, 30), new Vector2(64, 112) };
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, bread) - 4f, ColorUtil.Hex("#E8B66A"));
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, bread) + 6f, ColorUtil.Hex("#FFE7B0"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 40, 46, 5, 4), ColorUtil.Lime);
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 50, 40, 5, 4), ColorUtil.Red);
                    break;
                }
                case "smoothie":
                {
                    var cup = new List<Vector2> { new Vector2(30, 100), new Vector2(98, 100), new Vector2(86, 10), new Vector2(42, 10) };
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, cup), (x, y, d) => Color.Lerp(ColorUtil.Pink, ColorUtil.Hex("#FFB0C8"), y / S));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 102, 38, 7, 6), Color.white);
                    c.Paint((x, y) => PixelCanvas.Segment(x, y, 70, 104, 86, 126, 4.5f), ColorUtil.Blue);
                    break;
                }
                case "burger":
                    c.Paint((x, y) => Mathf.Max(PixelCanvas.Ellipse(x, y, 64, 70, 52, 40), 70 - y), ColorUtil.Hex("#E89A3C"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 64, 54, 6, 5), ColorUtil.Lime);
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 52, 50, 9, 8), ColorUtil.Hex("#7A3E22"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 34, 50, 10, 9), ColorUtil.Hex("#E89A3C"));
                    foreach (var p in new[] { new Vector2(46, 92), new Vector2(66, 100), new Vector2(84, 90) })
                        c.Paint((x, y) => PixelCanvas.Ellipse(x, y, p.x, p.y, 4, 2.5f), Color.white);
                    break;
                case "cake":
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 46, 46, 30, 8), ColorUtil.Hex("#FFE7B0"));
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 70, 48, 10, 8), ColorUtil.Pink);
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 44, 46, 4, 3), ColorUtil.Pink);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 92, 11), ColorUtil.Red);
                    break;
                case "ball":
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 62, 50),
                        (x, y, d) => Mathf.Repeat((x + y) / 40f, 1f) < 0.5f ? ColorUtil.Blue : ColorUtil.Yellow);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 46, 84, 12, 8), new Color(1, 1, 1, 0.5f));
                    break;
                case "teddy":
                {
                    var fur = ColorUtil.Hex("#C98552");
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 36, 104, 16), fur);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 92, 104, 16), fur);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 80, 36), fur);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 70, 16, 12), ColorUtil.Hex("#F2C79A"));
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 74, 5), ColorUtil.FaceInk);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 50, 88, 4.5f), ColorUtil.FaceInk);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 78, 88, 4.5f), ColorUtil.FaceInk);
                    c.Paint((x, y) => PixelCanvas.RoundedBox(x, y, 64, 30, 30, 24, 18), fur);
                    break;
                }
                case "rocket":
                {
                    var fin = new List<Vector2> { new Vector2(28, 14), new Vector2(100, 14), new Vector2(64, 60) };
                    c.Paint((x, y) => PixelCanvas.Polygon(x, y, fin), ColorUtil.Red);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 66, 22, 54), Color.white);
                    c.Paint((x, y) => PixelCanvas.Circle(x, y, 64, 74, 10), ColorUtil.Blue);
                    c.Paint((x, y) => PixelCanvas.Ellipse(x, y, 64, 10, 10, 10), ColorUtil.Orange);
                    break;
                }
            }
        });

        /// <summary>Mud splats laid over the avatar when it is dirty (256 px, same frame as the avatar).</summary>
        public static Sprite Dirt() => Cached("dirt", c =>
        {
            var mud = new Color(0.42f, 0.28f, 0.16f, 0.8f);
            foreach (var p in new[] { new Vector3(70, 150, 18), new Vector3(180, 90, 22), new Vector3(110, 60, 14), new Vector3(190, 170, 12), new Vector3(60, 100, 10) })
            {
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x, p.y, p.z), mud);
                c.Paint((x, y) => PixelCanvas.Circle(x, y, p.x + p.z * 0.9f, p.y - p.z * 0.6f, p.z * 0.4f), mud);
            }
            // a couple of little flies
            foreach (var f in new[] { new Vector2(214, 226), new Vector2(40, 214) })
            {
                c.Paint((x, y) => PixelCanvas.Circle(x, y, f.x, f.y, 5), ColorUtil.FaceInk);
                c.Paint((x, y) => PixelCanvas.Ellipse(x, y, f.x - 5, f.y + 6, 5, 3), new Color(1, 1, 1, 0.7f));
                c.Paint((x, y) => PixelCanvas.Ellipse(x, y, f.x + 5, f.y + 6, 5, 3), new Color(1, 1, 1, 0.7f));
            }
        }, 256);
    }
}
