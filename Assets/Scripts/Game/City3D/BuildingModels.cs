using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>
    /// Low-poly 3D models for every city building, built from simple shapes. Higher levels add floors and
    /// details, so upgrades are visible. Footprint is about 2.6 x 2.6 units, centred on the origin.
    /// </summary>
    public static class BuildingModels
    {
        private static Color H(string hex) => JellyDrop.Visuals.ColorUtil.Hex(hex);

        private static readonly Color Wall = H("#F4E6CF");
        private static readonly Color WallWarm = H("#F2D3A6");
        private static readonly Color Wood = H("#A86F43");
        private static readonly Color WoodDark = H("#7A4E2E");
        private static readonly Color Stone = H("#B9BCCB");
        private static readonly Color StoneDark = H("#8D91A6");
        private static readonly Color Brick = H("#C8573E");
        private static readonly Color RoofRed = H("#E0573F");
        private static readonly Color RoofBlue = H("#4D7CD9");
        private static readonly Color RoofPurple = H("#8E6CE0");
        private static readonly Color Window = H("#FFE59A");
        private static readonly Color Door = H("#6B3F24");
        private static readonly Color Leaf = H("#4CB66A");
        private static readonly Color LeafDark = H("#2F8F55");
        private static readonly Color Glass = H("#7FD6F2");
        private static readonly Color Gold = H("#FFC93C");
        private static readonly Color Water = H("#4FB8FF");

        public static void Build(MeshBuilder mb, string id, int level)
        {
            level = Mathf.Clamp(level, 1, 3);
            switch (id)
            {
                case "house": House(mb, level); break;
                case "sawmill": Sawmill(mb, level); break;
                case "bakery": Bakery(mb, level); break;
                case "brickworks": Brickworks(mb, level); break;
                case "park": Park(mb, level); break;
                case "windmill": WindmillBase(mb, level); break;
                case "glassworks": Glassworks(mb, level); break;
                case "market": Market(mb, level); break;
                case "tower": Tower(mb, level); break;
                case "castle": Castle(mb, level); break;
                default: mb.Box(Vector3.zero, new Vector3(2f, 1.2f, 2f), Wall); break;
            }
        }

        /// <summary>Windmill blades are a separate object so they can spin. Hub height for the given level.</summary>
        public static float WindmillHubHeight(int level) => 2.6f + 0.5f * (Mathf.Clamp(level, 1, 3) - 1);

        public static void WindmillBlades(MeshBuilder mb)
        {
            mb.Cylinder(new Vector3(0, 0, -0.15f), 0.18f, 0.3f, WoodDark, 8);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 90f * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0);
                var right = new Vector3(-dir.y, dir.x, 0) * 0.22f;
                Vector3 p0 = dir * 0.2f, p1 = dir * 1.6f;
                // Sail: a thin flat quad on both sides
                mb.Quad(p0 - right, p1 - right, p1 + right, p0 + right, Wall);
                mb.Quad(p0 + right, p1 + right, p1 - right, p0 - right, Wall);
                mb.Box(p0 + dir * 0.0f, new Vector3(0.06f, 0.06f, 0.06f), Wood);
            }
        }

        // ------------------------------------------------------------------ buildings

        private static void Windows(MeshBuilder mb, float w, float d, float y, int countX, Color glass)
        {
            // Little window panes slightly in front of the front (-z) and back walls
            for (int i = 0; i < countX; i++)
            {
                float x = -w / 2f + (i + 0.5f) * w / countX;
                mb.Box(new Vector3(x, y, -d / 2f - 0.02f), new Vector3(0.32f, 0.38f, 0.05f), glass);
                mb.Box(new Vector3(x, y, d / 2f + 0.02f), new Vector3(0.32f, 0.38f, 0.05f), glass);
            }
        }

        private static void House(MeshBuilder mb, int level)
        {
            float w = 2.0f, d = 1.8f, floor = 1.0f;
            float h = floor * level;
            mb.Box(Vector3.zero, new Vector3(w + 0.2f, 0.15f, d + 0.2f), Stone);
            mb.Box(new Vector3(0, 0.15f, 0), new Vector3(w, h, d), level == 3 ? WallWarm : Wall);
            for (int f = 0; f < level; f++) Windows(mb, w, d, 0.15f + f * floor + 0.45f, 2, Window);
            mb.Box(new Vector3(0, 0.15f, -d / 2f - 0.03f), new Vector3(0.4f, 0.65f, 0.06f), Door);
            mb.Roof(new Vector3(0, 0.15f + h, 0), new Vector3(w + 0.3f, 0.9f, d + 0.3f), RoofRed);
            mb.Box(new Vector3(0.55f, 0.15f + h + 0.2f, 0.35f), new Vector3(0.25f, 0.7f, 0.25f), Brick);
            if (level >= 2) Bush(mb, new Vector3(-1.05f, 0, -0.9f), 0.32f);
            if (level >= 3) Bush(mb, new Vector3(1.05f, 0, -0.9f), 0.32f);
        }

        private static void Sawmill(MeshBuilder mb, int level)
        {
            mb.Box(Vector3.zero, new Vector3(2.4f, 0.12f, 2.2f), H("#9A7A55"));
            mb.Box(new Vector3(-0.3f, 0.12f, 0.2f), new Vector3(1.6f, 0.9f + 0.3f * level, 1.4f), Wood);
            mb.Roof(new Vector3(-0.3f, 1.02f + 0.3f * level, 0.2f), new Vector3(1.8f, 0.6f, 1.6f), WoodDark);
            // Log pile
            for (int i = 0; i < 2 + level; i++)
            {
                mb.Box(new Vector3(0.88f, 0.12f + (i / 2) * 0.26f, -0.7f + (i % 2) * 0.28f), new Vector3(0.9f, 0.24f, 0.24f), Wood, H("#D9A066"));
            }
            // Saw blade
            mb.Cylinder(new Vector3(-0.3f, 0.6f, -0.62f), 0.35f, 0.05f, Stone, 12);
        }

        private static void Bakery(MeshBuilder mb, int level)
        {
            float w = 2.0f, d = 1.8f, h = 1.0f + 0.45f * (level - 1);
            mb.Box(Vector3.zero, new Vector3(w + 0.2f, 0.15f, d + 0.2f), Stone);
            mb.Box(new Vector3(0, 0.15f, 0), new Vector3(w, h, d), H("#FFE2C2"));
            Windows(mb, w, d, 0.6f, 2, Window);
            // Striped awning over the door
            for (int i = 0; i < 4; i++)
                mb.Box(new Vector3(-0.6f + i * 0.4f, 0.95f, -d / 2f - 0.25f), new Vector3(0.4f, 0.08f, 0.5f), i % 2 == 0 ? H("#FF6F91") : Color.white);
            mb.Box(new Vector3(0, 0.15f, -d / 2f - 0.03f), new Vector3(0.45f, 0.65f, 0.06f), Door);
            mb.Roof(new Vector3(0, 0.15f + h, 0), new Vector3(w + 0.3f, 0.8f, d + 0.3f), H("#C0743E"));
            mb.Cylinder(new Vector3(-0.6f, 0.15f + h + 0.2f, 0.3f), 0.16f, 0.8f, Brick, 8);
            // Giant bread on the sign
            mb.Sphere(new Vector3(0, 0.15f + h + 0.85f, -0.2f), 0.25f + 0.05f * level, H("#E3A257"));
        }

        private static void Brickworks(MeshBuilder mb, int level)
        {
            mb.Box(Vector3.zero, new Vector3(2.4f, 0.12f, 2.2f), H("#A0705A"));
            mb.Box(new Vector3(-0.2f, 0.12f, 0.1f), new Vector3(1.8f, 1.1f, 1.6f), Brick);
            mb.Pyramid(new Vector3(-0.2f, 1.22f, 0.1f), new Vector3(1.9f, 0.6f, 1.7f), StoneDark);
            float chimney = 1.6f + 0.6f * level;
            mb.Cylinder(new Vector3(0.75f, 0.12f, 0.6f), 0.25f, chimney, H("#9E3F2E"), 10, 0.2f);
            mb.Cylinder(new Vector3(0.75f, 0.12f + chimney, 0.6f), 0.24f, 0.12f, StoneDark, 10);
            // Brick stacks
            for (int i = 0; i < level; i++) mb.Box(new Vector3(-0.8f + i * 0.45f, 0.12f, -0.85f), new Vector3(0.35f, 0.25f, 0.3f), Brick, H("#E07A5F"));
        }

        private static void Park(MeshBuilder mb, int level)
        {
            mb.Box(Vector3.zero, new Vector3(2.6f, 0.1f, 2.6f), H("#62C46E"));
            // Path cross
            mb.Box(new Vector3(0, 0.1f, 0), new Vector3(2.6f, 0.02f, 0.4f), H("#E8D8B0"));
            mb.Box(new Vector3(0, 0.1f, 0), new Vector3(0.4f, 0.02f, 2.6f), H("#E8D8B0"));
            Tree(mb, new Vector3(-0.8f, 0.1f, 0.8f), 1f);
            Tree(mb, new Vector3(0.8f, 0.1f, -0.8f), 0.9f);
            if (level >= 2) { Tree(mb, new Vector3(0.85f, 0.1f, 0.85f), 0.8f); Bush(mb, new Vector3(-0.85f, 0.1f, -0.85f), 0.35f); }
            // Fountain in the middle
            mb.Cylinder(new Vector3(0, 0.1f, 0), 0.45f, 0.25f, Stone, 12);
            mb.Cylinder(new Vector3(0, 0.33f, 0), 0.38f, 0.04f, Water, 12);
            if (level >= 3) mb.Cylinder(new Vector3(0, 0.35f, 0), 0.08f, 0.6f, Stone, 8);
        }

        private static void WindmillBase(MeshBuilder mb, int level)
        {
            float hub = WindmillHubHeight(level);
            mb.Cylinder(Vector3.zero, 1.0f, 0.15f, Stone, 12);
            mb.Cylinder(new Vector3(0, 0.15f, 0), 0.85f, hub - 0.15f, Wall, 8, 0.55f);
            mb.Cone(new Vector3(0, hub, 0), 0.7f, 0.8f, RoofRed, 8);
            mb.Box(new Vector3(0, 0.15f, -0.82f), new Vector3(0.4f, 0.7f, 0.1f), Door);
            mb.Box(new Vector3(0, hub - 0.9f, -0.62f), new Vector3(0.28f, 0.3f, 0.08f), Window);
        }

        private static void Glassworks(MeshBuilder mb, int level)
        {
            mb.Box(Vector3.zero, new Vector3(2.4f, 0.15f, 2.2f), Stone);
            mb.Box(new Vector3(0, 0.15f, 0), new Vector3(2.0f, 0.9f, 1.8f), H("#DDE3F0"));
            Windows(mb, 2.0f, 1.8f, 0.55f, 3, Glass);
            // Glass dome
            mb.Sphere(new Vector3(0, 1.05f, 0), 0.75f + 0.12f * level, Glass, 4, 10);
            mb.Cylinder(new Vector3(0.75f, 0.15f, 0.6f), 0.18f, 1.6f + 0.3f * level, StoneDark, 8);
        }

        private static void Market(MeshBuilder mb, int level)
        {
            mb.Box(Vector3.zero, new Vector3(2.6f, 0.1f, 2.6f), H("#E8D8B0"));
            Color[] canopies = { H("#FF6F91"), H("#3FB6FF"), H("#FFC93C"), H("#2ED3A0") };
            int stalls = 1 + level;
            for (int i = 0; i < stalls; i++)
            {
                float x = (i % 2 == 0 ? -0.6f : 0.6f), z = (i < 2 ? -0.55f : 0.65f);
                var c = canopies[i % canopies.Length];
                mb.Box(new Vector3(x, 0.1f, z), new Vector3(0.9f, 0.45f, 0.6f), Wood);
                foreach (var cx in new[] { -0.4f, 0.4f })
                    mb.Box(new Vector3(x + cx, 0.1f, z - 0.25f), new Vector3(0.06f, 0.95f, 0.06f), WoodDark);
                mb.Roof(new Vector3(x, 1.05f, z), new Vector3(1.05f, 0.35f, 0.8f), c);
                // Goods on the counter
                mb.Sphere(new Vector3(x - 0.2f, 0.62f, z), 0.12f, i % 2 == 0 ? H("#FF5A6E") : H("#9BDB4D"), 3, 6);
                mb.Sphere(new Vector3(x + 0.15f, 0.62f, z), 0.12f, H("#FF9F43"), 3, 6);
            }
        }

        private static void Tower(MeshBuilder mb, int level)
        {
            float h = 3.2f + 0.9f * (level - 1);
            mb.Cylinder(Vector3.zero, 1.0f, 0.2f, StoneDark, 10);
            mb.Cylinder(new Vector3(0, 0.2f, 0), 0.75f, h, Stone, 10, 0.65f);
            for (int i = 0; i < level + 1; i++)
                mb.Box(new Vector3(0, 0.9f + i * 0.9f, -0.7f), new Vector3(0.22f, 0.36f, 0.08f), Window);
            mb.Cylinder(new Vector3(0, 0.2f + h, 0), 0.85f, 0.25f, StoneDark, 10);
            mb.Cone(new Vector3(0, 0.45f + h, 0), 0.85f, 1.3f, RoofBlue, 10);
            Flag(mb, new Vector3(0, 1.75f + h, 0), H("#FF6F91"));
        }

        private static void Castle(MeshBuilder mb, int level)
        {
            float wall = 0.9f + 0.25f * level;
            mb.Box(Vector3.zero, new Vector3(2.8f, 0.15f, 2.8f), StoneDark);
            // Walls
            mb.Box(new Vector3(0, 0.15f, -1.15f), new Vector3(2.3f, wall, 0.3f), Stone);
            mb.Box(new Vector3(0, 0.15f, 1.15f), new Vector3(2.3f, wall, 0.3f), Stone);
            mb.Box(new Vector3(-1.15f, 0.15f, 0), new Vector3(0.3f, wall, 2.3f), Stone);
            mb.Box(new Vector3(1.15f, 0.15f, 0), new Vector3(0.3f, wall, 2.3f), Stone);
            // Gate
            mb.Box(new Vector3(0, 0.15f, -1.32f), new Vector3(0.6f, 0.75f, 0.06f), Door);
            // Corner towers
            foreach (var x in new[] { -1.15f, 1.15f })
                foreach (var z in new[] { -1.15f, 1.15f })
                {
                    mb.Cylinder(new Vector3(x, 0.15f, z), 0.38f, wall + 0.6f, Stone, 8);
                    mb.Cone(new Vector3(x, 0.75f + wall, z), 0.45f, 0.7f, RoofPurple, 8);
                }
            // Keep
            float keep = 1.6f + 0.5f * level;
            mb.Box(new Vector3(0, 0.15f, 0.1f), new Vector3(1.1f, keep, 1.1f), WallWarm);
            mb.Pyramid(new Vector3(0, 0.15f + keep, 0.1f), new Vector3(1.3f, 0.9f, 1.3f), RoofPurple);
            Flag(mb, new Vector3(0, 1.05f + keep, 0.1f), Gold);
        }

        // ------------------------------------------------------------------ decoration

        public static void Tree(MeshBuilder mb, Vector3 at, float scale)
        {
            mb.Cylinder(at, 0.1f * scale, 0.5f * scale, Wood, 6);
            mb.Cone(at + Vector3.up * 0.4f * scale, 0.5f * scale, 0.8f * scale, LeafDark, 7);
            mb.Cone(at + Vector3.up * 0.8f * scale, 0.38f * scale, 0.7f * scale, Leaf, 7);
        }

        public static void RoundTree(MeshBuilder mb, Vector3 at, float scale)
        {
            mb.Cylinder(at, 0.1f * scale, 0.6f * scale, Wood, 6);
            mb.Sphere(at + Vector3.up * 0.95f * scale, 0.5f * scale, Leaf, 4, 7);
        }

        public static void Bush(MeshBuilder mb, Vector3 at, float r) => mb.Sphere(at + Vector3.up * r * 0.7f, r, LeafDark, 3, 7);

        private static void Flag(MeshBuilder mb, Vector3 at, Color color)
        {
            mb.Box(at, new Vector3(0.05f, 0.7f, 0.05f), WoodDark);
            var top = at + Vector3.up * 0.7f;
            mb.Triangle(top, top + new Vector3(0.45f, -0.15f, 0), top + new Vector3(0, -0.3f, 0), color);
            mb.Triangle(top, top + new Vector3(0, -0.3f, 0), top + new Vector3(0.45f, -0.15f, 0), color);
        }

        /// <summary>Wooden scaffolding shown while a building goes up.</summary>
        public static void Scaffold(MeshBuilder mb, float height)
        {
            foreach (var x in new[] { -1.2f, 1.2f })
                foreach (var z in new[] { -1.2f, 1.2f })
                    mb.Box(new Vector3(x, 0, z), new Vector3(0.08f, height, 0.08f), Wood);
            for (float y = 0.6f; y < height; y += 0.7f)
            {
                mb.Box(new Vector3(0, y, -1.2f), new Vector3(2.48f, 0.06f, 0.06f), Wood);
                mb.Box(new Vector3(0, y, 1.2f), new Vector3(2.48f, 0.06f, 0.06f), Wood);
                mb.Box(new Vector3(-1.2f, y, 0), new Vector3(0.06f, 0.06f, 2.48f), Wood);
                mb.Box(new Vector3(1.2f, y, 0), new Vector3(0.06f, 0.06f, 2.48f), Wood);
            }
        }
    }
}
