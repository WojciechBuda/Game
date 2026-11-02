using System;
using System.Collections.Generic;
using JellyDrop.Core.Islands;
using JellyDrop.Runner3D;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>
    /// The forest island next to the village (joined by a bridge). Four areas from front to back: the clearing,
    /// the thicket, the old forest and the magic glade; locked areas are hidden in mist. Mushrooms grow on their
    /// spots (tap to pick), trees can be chopped (a sapling grows back), bushes carry berries, forest buildings
    /// stand on their own plots, deer, rabbits and squirrels wander around, and now and then a fox runs off with
    /// a basket of mushrooms.
    /// </summary>
    public sealed class ForestWorld : IslandWorld
    {
        public static readonly Vector3 WorldOrigin = new Vector3(-400f, 0f, 2000f);
        private const float HalfW = 9.5f, ZFront = -15f, ZBack = 16f;

        // Area z ranges, front to back
        private static readonly float[] AreaZ = { -13f, -4f, 3.5f, 10f, 15f };

        public event Action<ForestPick, Vector3> MushroomPicked;
        public event Action<int, Vector3> TreeChopped;
        public event Action<int, Vector3> BerriesPicked;
        public event Action<ForestBuilding> BuildingTapped;
        public event Action<int, int> FoxProgress;
        public event Action<bool> FoxEnded;

        private ForestService _forest;

        private sealed class SpotView { public Transform T; public GameObject Normal, Gold; }
        private sealed class TreeView { public Transform T; public GameObject Full, Stump, Sapling; }
        private sealed class BushView { public Transform T; public GameObject Berries; }
        private sealed class BuildingView { public Transform T; public GameObject Model; public int Shown = -1; public GameObject Sign; }

        private readonly SpotView[] _spots = new SpotView[ForestCatalog.MaxSpots];
        private readonly TreeView[] _trees = new TreeView[ForestCatalog.Trees];
        private readonly BushView[] _bushes = new BushView[ForestCatalog.Bushes];
        private readonly BuildingView[] _buildings = new BuildingView[ForestCatalog.Buildings.Length];
        private readonly GameObject[] _mist = new GameObject[4];
        private Transform _fox;
        private int _foxHits;
        private float _foxTime;
        private readonly List<Transform> _glow = new List<Transform>();

        public const int FoxTaps = 3;
        public const float FoxSeconds = 15f;

        public bool FoxActive => _fox != null;
        public float FoxTimeLeft => _foxTime;

        public void Setup(ForestService forest) => _forest = forest;

        protected override void BuildWorld()
        {
            PanHalfWidth = 5f;
            PanZMin = ZFront + 4f;
            PanZMax = ZBack - 6f;
            ViewWidth = 15f;
            StartFocus = new Vector3(0f, 0f, -9f);

            var mb = new MeshBuilder();
            Island(mb, HalfW, ZFront, ZBack, ColorUtil.Hex("#4FA65A"), ColorUtil.Hex("#E8D29B"), ColorUtil.Hex("#2F78C2"));
            // Darker forest floor patches per area and a winding path
            Color[] floor = { ColorUtil.Hex("#5DB866"), ColorUtil.Hex("#3E8F4C"), ColorUtil.Hex("#2F7440"), ColorUtil.Hex("#4A6FB0") };
            for (int a = 0; a < 4; a++)
                mb.Box(new Vector3(0, 0.01f, (AreaZ[a] + AreaZ[a + 1]) / 2f), new Vector3(HalfW * 1.6f, 0.02f, AreaZ[a + 1] - AreaZ[a] - 0.4f), floor[a]);
            var path = ColorUtil.Hex("#C9A46A");
            for (float z = ZFront + 1f; z < ZBack - 2f; z += 0.8f)
                mb.Box(new Vector3(Mathf.Sin(z * 0.35f) * 2.2f, 0.03f, z), new Vector3(1.3f, 0.03f, 0.9f), path);
            Bridge(mb, new Vector3(-HalfW - 0.5f, 0.05f, -10f), 9f);
            // Dense decorative trees around the edge
            for (int i = 0; i < 140; i++)
            {
                float side = Rng.Next(2) == 0 ? -1f : 1f;
                var p = new Vector3(side * (HalfW - 0.6f - (float)Rng.NextDouble() * 2f), 0f, ZFront + 1.5f + (float)Rng.NextDouble() * (ZBack - ZFront - 3f));
                if (Mathf.Abs(p.z + 10f) < 1.4f && side < 0) continue; // keep the bridge free
                float s = 0.9f + (float)Rng.NextDouble() * 0.9f;
                if (Rng.NextDouble() < 0.6) BuildingModels.Tree(mb, p, s * 1.2f);
                else BuildingModels.RoundTree(mb, p, s);
            }
            // Rocks, flowers, ferns
            for (int i = 0; i < 40; i++)
            {
                var p = new Vector3(((float)Rng.NextDouble() * 2f - 1f) * (HalfW - 2.5f), 0f, ZFront + 2f + (float)Rng.NextDouble() * (ZBack - ZFront - 4f));
                double r = Rng.NextDouble();
                if (r < 0.3) mb.Sphere(p + Vector3.up * 0.1f, 0.25f, ColorUtil.Hex("#9EA3B5"), 3, 6);
                else if (r < 0.7)
                {
                    var c = r < 0.5 ? ColorUtil.Hex("#FF8FC0") : ColorUtil.Hex("#FFE27A");
                    mb.Box(p, new Vector3(0.04f, 0.25f, 0.04f), ColorUtil.Hex("#3E9A44"));
                    mb.Sphere(p + Vector3.up * 0.28f, 0.09f, c, 2, 5);
                }
                else
                    for (int k = 0; k < 4; k++)
                    {
                        float a = k * Mathf.PI / 2f + (float)r;
                        mb.Box(p + new Vector3(Mathf.Cos(a) * 0.15f, 0.12f, Mathf.Sin(a) * 0.15f), new Vector3(0.06f, 0.24f, 0.3f), ColorUtil.Hex("#3E8A44"));
                    }
            }
            // Magic glade: glowing stones in a ring
            for (int k = 0; k < 7; k++)
            {
                float a = k * Mathf.PI * 2f / 7f;
                mb.Box(new Vector3(Mathf.Cos(a) * 3f, 0f, 12.5f + Mathf.Sin(a) * 1.8f), new Vector3(0.4f, 1.1f + (k % 2) * 0.4f, 0.4f), ColorUtil.Hex("#8A90D6"));
            }
            mb.ToObject("ForestScenery", Root);

            for (int k = 0; k < 6; k++)
            {
                var g = MeshObject("Glow", GlowMesh(), Root, false).transform;
                float a = k * Mathf.PI * 2f / 6f;
                g.localPosition = new Vector3(Mathf.Cos(a) * 2.2f, 1.2f, 12.5f + Mathf.Sin(a) * 1.3f);
                _glow.Add(g);
            }

            BuildSpots();
            BuildTrees();
            BuildBushes();
            BuildBuildings();
            BuildAnimals();
            BuildMist();
        }

        // ------------------------------------------------------------------ content

        private Vector3 RandomIn(int area, System.Random rng, float xMax = 6.5f)
        {
            return new Vector3(((float)rng.NextDouble() * 2f - 1f) * xMax, 0f, Mathf.Lerp(AreaZ[area] + 0.8f, AreaZ[area + 1] - 0.8f, (float)rng.NextDouble()));
        }

        private void BuildSpots()
        {
            var rng = new System.Random(21);
            var goldMesh = GoldMushroomMesh();
            for (int i = 0; i < _spots.Length; i++)
            {
                var t = new GameObject("Spot" + i).transform;
                t.SetParent(Root, false);
                t.localPosition = RandomIn(ForestService.AreaOfSpot(i), rng) + Vector3.up * 0.3f;
                var n = MeshObject("Mushroom", RunnerWorld.MushroomMesh, t);
                n.transform.localScale = Vector3.one * 1.3f;
                var g = MeshObject("Gold", goldMesh, t);
                g.transform.localScale = Vector3.one * 1.4f;
                g.AddComponent<Hover>().Spin = true;
                int idx = i;
                _spots[i] = new SpotView { T = t, Normal = n, Gold = g };
                AddTappable(t, 0.9f, () => Pick(idx), () => _forest != null && _forest.IsSpotReady(idx));
            }
        }

        private void BuildTrees()
        {
            var rng = new System.Random(33);
            var full = new MeshBuilder();
            BuildingModels.Tree(full, Vector3.zero, 2.2f);
            var fullMesh = full.ToMesh("BigTree");
            var stump = new MeshBuilder();
            stump.Cylinder(Vector3.zero, 0.24f, 0.3f, ColorUtil.Hex("#8A5A34"), 8);
            stump.Disc(new Vector3(0, 0.3f, 0), 0.22f, ColorUtil.Hex("#E8C590"), 8);
            var stumpMesh = stump.ToMesh("Stump");
            var sap = new MeshBuilder();
            BuildingModels.Tree(sap, Vector3.zero, 1f);
            var sapMesh = sap.ToMesh("Sapling");
            for (int i = 0; i < _trees.Length; i++)
            {
                var t = new GameObject("Tree" + i).transform;
                t.SetParent(Root, false);
                t.localPosition = RandomIn(ForestService.AreaOfTree(i), rng, 7.5f);
                var v = new TreeView
                {
                    T = t,
                    Full = MeshObject("Full", fullMesh, t),
                    Stump = MeshObject("Stump", stumpMesh, t),
                    Sapling = MeshObject("Sapling", sapMesh, t),
                };
                _trees[i] = v;
                int idx = i;
                AddTappable(t, 1.2f, () => Chop(idx), () => _forest != null && _forest.IsTreeReady(idx));
            }
        }

        private void BuildBushes()
        {
            var rng = new System.Random(44);
            var bush = new MeshBuilder();
            bush.Sphere(new Vector3(0, 0.4f, 0), 0.55f, ColorUtil.Hex("#2F7A3A"), 4, 8);
            bush.Sphere(new Vector3(0.35f, 0.35f, 0.1f), 0.38f, ColorUtil.Hex("#3E8F4C"), 3, 7);
            var bushMesh = bush.ToMesh("Bush");
            var berries = new MeshBuilder();
            foreach (var p in new[] { new Vector3(0.3f, 0.7f, 0.35f), new Vector3(-0.25f, 0.6f, 0.4f), new Vector3(0.05f, 0.85f, 0.3f), new Vector3(0.5f, 0.45f, 0.3f), new Vector3(-0.4f, 0.4f, 0.25f) })
                berries.Sphere(p, 0.09f, ColorUtil.Hex("#4A3AC8"), 2, 5);
            var berryMesh = berries.ToMesh("Berries");
            for (int i = 0; i < _bushes.Length; i++)
            {
                var t = new GameObject("Bush" + i).transform;
                t.SetParent(Root, false);
                t.localPosition = RandomIn(ForestService.AreaOfBush(i), rng);
                MeshObject("Bush", bushMesh, t);
                var b = MeshObject("Berries", berryMesh, t);
                _bushes[i] = new BushView { T = t, Berries = b };
                int idx = i;
                AddTappable(t, 1f, () => PickBush(idx), () => _forest != null && _forest.IsBushReady(idx));
            }
        }

        private static readonly Vector3[] BuildingSpots =
        {
            new Vector3(4.6f, 0, -11.2f), new Vector3(-4.6f, 0, -6.5f), new Vector3(4.8f, 0, -1.5f), new Vector3(-4.8f, 0, 5.5f), new Vector3(4.5f, 0, 8f),
        };

        private void BuildBuildings()
        {
            for (int i = 0; i < _buildings.Length; i++)
            {
                var t = new GameObject("Building" + i).transform;
                t.SetParent(Root, false);
                t.localPosition = BuildingSpots[i];
                var plot = new MeshBuilder();
                plot.Box(new Vector3(0, 0.02f, 0), new Vector3(3f, 0.04f, 3f), ColorUtil.Hex("#B58B5A"));
                plot.ToObject("Plot", t, false);
                var sign = new MeshBuilder();
                sign.Box(new Vector3(0, 0, 0), new Vector3(0.1f, 1.1f, 0.1f), ColorUtil.Hex("#6E4826"));
                sign.Box(new Vector3(0, 1f, 0.06f), new Vector3(0.9f, 0.5f, 0.08f), ColorUtil.Hex("#E8C590"));
                sign.Box(new Vector3(0, 1f, 0.11f), new Vector3(0.5f, 0.1f, 0.02f), ColorUtil.Hex("#3E9A44"));
                sign.Box(new Vector3(0, 1f, 0.11f), new Vector3(0.1f, 0.4f, 0.02f), ColorUtil.Hex("#3E9A44"));
                var s = sign.ToObject("Sign", t);
                _buildings[i] = new BuildingView { T = t, Sign = s };
                var b = (ForestBuilding)i;
                AddTappable(t, 1.6f, () => BuildingTapped?.Invoke(b));
            }
        }

        private void BuildAnimals()
        {
            var deer = new MeshBuilder();
            var fur = ColorUtil.Hex("#B5764A");
            deer.Box(new Vector3(0, 0.55f, 0), new Vector3(0.35f, 0.3f, 0.8f), fur);
            foreach (var x in new[] { -0.12f, 0.12f })
                foreach (var z in new[] { -0.3f, 0.3f })
                    deer.Box(new Vector3(x, 0, z), new Vector3(0.07f, 0.45f, 0.07f), PixelCanvas.Darken(fur, 0.2f));
            deer.Box(new Vector3(0, 0.85f, 0.42f), new Vector3(0.2f, 0.3f, 0.25f), fur);
            deer.Box(new Vector3(0, 1.08f, 0.45f), new Vector3(0.04f, 0.2f, 0.04f), ColorUtil.Hex("#6E4826"));
            deer.Box(new Vector3(0.1f, 1.15f, 0.45f), new Vector3(0.18f, 0.04f, 0.04f), ColorUtil.Hex("#6E4826"));
            deer.Box(new Vector3(-0.1f, 1.15f, 0.45f), new Vector3(0.18f, 0.04f, 0.04f), ColorUtil.Hex("#6E4826"));
            var deerMesh = deer.ToMesh("Deer");

            var rabbit = new MeshBuilder();
            rabbit.Sphere(new Vector3(0, 0.18f, 0), 0.18f, ColorUtil.Hex("#E8E2D8"), 3, 6);
            rabbit.Sphere(new Vector3(0, 0.32f, 0.14f), 0.11f, ColorUtil.Hex("#E8E2D8"), 3, 6);
            rabbit.Box(new Vector3(-0.04f, 0.48f, 0.12f), new Vector3(0.04f, 0.18f, 0.03f), ColorUtil.Hex("#F2C0C8"));
            rabbit.Box(new Vector3(0.04f, 0.48f, 0.12f), new Vector3(0.04f, 0.18f, 0.03f), ColorUtil.Hex("#F2C0C8"));
            var rabbitMesh = rabbit.ToMesh("Rabbit");

            var squirrel = new MeshBuilder();
            var red = ColorUtil.Hex("#D9631A");
            squirrel.Sphere(new Vector3(0, 0.15f, 0), 0.13f, red, 3, 6);
            squirrel.Sphere(new Vector3(0, 0.28f, 0.1f), 0.09f, red, 3, 6);
            squirrel.Sphere(new Vector3(0, 0.3f, -0.15f), 0.13f, PixelCanvas.Lighten(red, 0.15f), 3, 6);
            var squirrelMesh = squirrel.ToMesh("Squirrel");

            int seed = 1;
            void Spawn(Mesh mesh, int area, float speed, float hop)
            {
                var go = MeshObject("Animal", mesh);
                var w = go.AddComponent<Wanderer>();
                w.Setup(seed++, new Vector2(-6f, AreaZ[area] + 0.5f), new Vector2(6f, AreaZ[area + 1] - 0.5f), speed, hop);
                go.transform.localPosition = new Vector3(0, 0, (AreaZ[area] + AreaZ[area + 1]) / 2f);
            }
            Spawn(deerMesh, 1, 0.9f, 0.04f);
            Spawn(deerMesh, 2, 0.8f, 0.04f);
            Spawn(rabbitMesh, 0, 1.4f, 0.18f);
            Spawn(rabbitMesh, 0, 1.2f, 0.18f);
            Spawn(rabbitMesh, 1, 1.3f, 0.18f);
            Spawn(squirrelMesh, 0, 1.6f, 0.12f);
            Spawn(squirrelMesh, 2, 1.6f, 0.12f);
        }

        private void BuildMist()
        {
            for (int a = 1; a < 4; a++)
            {
                var mb = new MeshBuilder();
                var rng = new System.Random(a * 7);
                float z0 = AreaZ[a], z1 = AreaZ[a + 1];
                for (int i = 0; i < 26; i++)
                {
                    var p = new Vector3(((float)rng.NextDouble() * 2f - 1f) * 8f, 0.6f + (float)rng.NextDouble() * 1.4f, Mathf.Lerp(z0, z1, (float)rng.NextDouble()));
                    mb.Sphere(p, 1.4f + (float)rng.NextDouble() * 1.2f, ColorUtil.Hex("#CFD6EA"), 4, 8);
                }
                // A little padlock sign in the middle
                var c = new Vector3(0, 2.6f, (z0 + z1) / 2f);
                mb.Box(c, new Vector3(1.1f, 0.9f, 0.3f), ColorUtil.Hex("#FFC93C"));
                mb.Cylinder(c + new Vector3(0, 0.45f, 0), 0.38f, 0.6f, ColorUtil.Hex("#C8CCDD"), 10, 0.38f, false);
                _mist[a] = mb.ToObject("Mist" + a, Root, false);
            }
        }

        private static Mesh GoldMushroomMesh()
        {
            var mb = new MeshBuilder();
            mb.Cylinder(new Vector3(0, -0.3f, 0), 0.14f, 0.32f, ColorUtil.Hex("#FFF4D8"), 8);
            mb.Sphere(new Vector3(0, 0.06f, 0), 0.34f, ColorUtil.Hex("#FFC93C"), 4, 10);
            mb.Disc(new Vector3(0, 0.0f, 0), 0.34f, ColorUtil.Hex("#D99A00"), 12);
            foreach (var p in new[] { new Vector3(0.15f, 0.3f, 0.12f), new Vector3(-0.14f, 0.28f, 0.16f), new Vector3(0.02f, 0.38f, -0.15f) })
                mb.Sphere(p, 0.06f, ColorUtil.Hex("#FFF8D0"), 2, 5);
            return mb.ToMesh("GoldMushroom");
        }

        private static Mesh _glowMesh;
        private static Mesh GlowMesh()
        {
            if (_glowMesh != null) return _glowMesh;
            var mb = new MeshBuilder();
            mb.Sphere(Vector3.zero, 0.12f, ColorUtil.Hex("#C8FFE0"), 3, 6);
            _glowMesh = mb.ToMesh("Firefly");
            return _glowMesh;
        }

        // ------------------------------------------------------------------ buildings

        /// <summary>Forest building models: bigger and richer with each level.</summary>
        private static Mesh BuildingMesh(ForestBuilding b, int level)
        {
            var mb = new MeshBuilder();
            var wood = ColorUtil.Hex("#8A5A34");
            var woodLight = ColorUtil.Hex("#B5804A");
            var roof = ColorUtil.Hex("#3E7A3A");
            float s = 0.85f + 0.15f * level;
            switch (b)
            {
                case ForestBuilding.MushroomHut:
                    // A house shaped like a giant mushroom
                    mb.Cylinder(Vector3.zero, 0.75f * s, 1.3f * s, ColorUtil.Hex("#F2E6D0"), 12);
                    mb.Box(new Vector3(0, 0, 0.72f * s), new Vector3(0.4f, 0.75f, 0.06f), wood);
                    mb.Sphere(new Vector3(0, 1.45f * s, 0), 1.25f * s, ColorUtil.Hex("#E0483E"), 5, 12);
                    mb.Disc(new Vector3(0, 1.35f * s, 0), 1.25f * s, ColorUtil.Hex("#A82A2A"), 14);
                    for (int i = 0; i < 4 + level * 2; i++)
                    {
                        float a = i * 2.4f;
                        mb.Sphere(new Vector3(Mathf.Cos(a) * 0.8f * s, 2.1f * s + (i % 2) * 0.15f, Mathf.Sin(a) * 0.8f * s), 0.16f * s, Color.white, 2, 5);
                    }
                    if (level >= 2) mb.Cylinder(new Vector3(1.1f, 0, -0.6f), 0.25f, 0.5f, ColorUtil.Hex("#F2E6D0"), 8);
                    if (level >= 3) mb.Sphere(new Vector3(1.1f, 0.6f, -0.6f), 0.45f, ColorUtil.Hex("#FFC93C"), 4, 8);
                    break;
                case ForestBuilding.LumberCamp:
                    mb.Box(new Vector3(0, 0, 0), new Vector3(2f * s, 1f * s, 1.4f), woodLight);
                    mb.Roof(new Vector3(0, 1f * s, 0), new Vector3(2.2f * s, 0.7f, 1.6f), roof);
                    for (int i = 0; i < 2 + level; i++) mb.Cylinder(new Vector3(-1f + i * 0.2f, 0.12f, 1.1f), 0.12f, 1.6f, wood, 6);
                    if (level >= 2) { mb.Box(new Vector3(1.2f, 0, 1f), new Vector3(0.5f, 0.6f, 0.5f), ColorUtil.Hex("#9EA3B5")); mb.Disc(new Vector3(1.2f, 0.62f, 1f), 0.35f, ColorUtil.Hex("#C8CCDD"), 12); }
                    if (level >= 3) mb.Box(new Vector3(-1.2f, 0, -0.6f), new Vector3(0.6f, 1.6f, 0.6f), woodLight);
                    break;
                case ForestBuilding.Apiary:
                    for (int i = 0; i < 1 + level; i++)
                    {
                        var p = new Vector3(-0.9f + i * 0.9f, 0, 0);
                        mb.Box(p, new Vector3(0.12f, 0.35f, 0.12f), wood);
                        for (int k = 0; k < 3; k++) mb.Cylinder(p + new Vector3(0, 0.35f + k * 0.28f, 0), 0.36f - k * 0.06f, 0.28f, ColorUtil.Hex("#FFC93C"), 10);
                        mb.Box(p + new Vector3(0, 0.45f, 0.32f), new Vector3(0.12f, 0.08f, 0.06f), ColorUtil.Hex("#3A2410"));
                    }
                    for (int i = 0; i < 5; i++) mb.Sphere(new Vector3(-1.2f + i * 0.6f, 0.15f, 1f), 0.16f, ColorUtil.Hex(i % 2 == 0 ? "#FF8FC0" : "#FFFFFF"), 2, 5);
                    break;
                case ForestBuilding.Watchtower:
                    foreach (var x in new[] { -0.6f, 0.6f })
                        foreach (var z in new[] { -0.6f, 0.6f })
                            mb.Box(new Vector3(x, 0, z), new Vector3(0.15f, 2.4f * s, 0.15f), wood);
                    mb.Box(new Vector3(0, 2.3f * s, 0), new Vector3(1.6f, 0.15f, 1.6f), woodLight);
                    mb.Box(new Vector3(0, 2.45f * s, 0.78f), new Vector3(1.6f, 0.4f, 0.06f), wood);
                    mb.Pyramid(new Vector3(0, 2.95f * s, 0), new Vector3(1.9f, 0.9f, 1.9f), roof);
                    if (level >= 2) mb.Box(new Vector3(0, 3.9f * s, 0), new Vector3(0.05f, 0.7f, 0.05f), wood);
                    if (level >= 3) mb.Triangle(new Vector3(0, 4.6f * s, 0), new Vector3(0.5f, 4.4f * s, 0), new Vector3(0, 4.2f * s, 0), ColorUtil.Hex("#FF5A6E"));
                    break;
                default: // Herbalist
                    mb.Box(Vector3.zero, new Vector3(1.8f * s, 1.1f * s, 1.5f), ColorUtil.Hex("#C9A46A"));
                    mb.Roof(new Vector3(0, 1.1f * s, 0), new Vector3(2f * s, 0.8f, 1.7f), ColorUtil.Hex("#7A5CFF"));
                    mb.Box(new Vector3(0, 0, 0.78f), new Vector3(0.45f, 0.8f, 0.06f), wood);
                    mb.Box(new Vector3(0.6f, 0.6f, 0.78f), new Vector3(0.35f, 0.35f, 0.04f), ColorUtil.Hex("#9FE3FF"));
                    mb.Cylinder(new Vector3(-0.7f, 1.4f * s, 0.2f), 0.14f, 0.7f, ColorUtil.Hex("#9EA3B5"), 6);
                    for (int i = 0; i < level * 2; i++) mb.Sphere(new Vector3(-1.2f + i * 0.35f, 0.15f, 1.05f), 0.13f, ColorUtil.Hex(i % 2 == 0 ? "#5CE1A8" : "#B48CFF"), 2, 5);
                    if (level >= 3) mb.Cylinder(new Vector3(1.1f, 0, -0.5f), 0.35f, 0.5f, ColorUtil.Hex("#6E4826"), 8);
                    break;
            }
            return mb.ToMesh("Forest" + b + level);
        }

        // ------------------------------------------------------------------ sync and actions

        public override void Sync()
        {
            if (_forest == null) return;
            for (int i = 0; i < _spots.Length; i++)
            {
                var v = _spots[i];
                bool open = _forest.IsSpotOpen(i);
                bool ready = open && _forest.IsSpotReady(i);
                bool gold = ready && _forest.SpotIsGold(i);
                v.Normal.SetActive(ready && !gold);
                v.Gold.SetActive(gold);
            }
            for (int i = 0; i < _trees.Length; i++)
            {
                var v = _trees[i];
                bool open = _forest.IsTreeOpen(i);
                v.T.gameObject.SetActive(open);
                if (!open) continue;
                bool ready = _forest.IsTreeReady(i);
                v.Full.SetActive(ready);
                v.Stump.SetActive(!ready);
                v.Sapling.SetActive(!ready);
                if (!ready) v.Sapling.transform.localScale = Vector3.one * Mathf.Lerp(0.2f, 1.1f, _forest.TreeGrowth(i));
            }
            for (int i = 0; i < _bushes.Length; i++)
            {
                bool open = _forest.IsBushOpen(i);
                _bushes[i].T.gameObject.SetActive(open);
                _bushes[i].Berries.SetActive(open && _forest.IsBushReady(i));
            }
            for (int i = 0; i < _buildings.Length; i++)
            {
                var v = _buildings[i];
                int level = _forest.BuildingLevel((ForestBuilding)i);
                v.Sign.SetActive(level == 0);
                if (level != v.Shown)
                {
                    bool grew = v.Shown >= 0 && level > v.Shown;
                    v.Shown = level;
                    if (v.Model != null) Destroy(v.Model);
                    if (level > 0)
                    {
                        v.Model = MeshObject("Model", BuildingMesh((ForestBuilding)i, level), v.T);
                        if (grew) v.Model.AddComponent<Squash>();
                    }
                }
            }
            int areas = _forest.AreasOpen;
            for (int a = 1; a < 4; a++) _mist[a].SetActive(a >= areas);
        }

        private void Pick(int spot)
        {
            var kind = _forest.PickSpot(spot);
            if (kind == ForestPick.None) return;
            var t = _spots[spot].T;
            var pos = ScreenOf(t);
            Sync();
            MushroomPicked?.Invoke(kind, pos);
        }

        private void Chop(int tree)
        {
            var t = _trees[tree].T;
            int wood = _forest.Chop(tree);
            if (wood <= 0) return;
            Sync();
            _trees[tree].Stump.AddComponent<Squash>();
            TreeChopped?.Invoke(wood, ScreenOf(t));
        }

        private void PickBush(int bush)
        {
            int berries = _forest.PickBush(bush);
            if (berries <= 0) return;
            Sync();
            BerriesPicked?.Invoke(berries, ScreenOf(_bushes[bush].T));
        }

        // ------------------------------------------------------------------ the fox

        public void BeginFox()
        {
            if (_fox != null) return;
            var mb = new MeshBuilder();
            var fur = ColorUtil.Hex("#E8742A");
            mb.Box(new Vector3(0, 0.3f, 0), new Vector3(0.3f, 0.26f, 0.7f), fur);
            foreach (var x in new[] { -0.1f, 0.1f }) foreach (var z in new[] { -0.25f, 0.25f }) mb.Box(new Vector3(x, 0, z), new Vector3(0.06f, 0.3f, 0.06f), ColorUtil.Hex("#3A2410"));
            mb.Box(new Vector3(0, 0.5f, 0.4f), new Vector3(0.24f, 0.22f, 0.26f), fur);
            mb.Box(new Vector3(0, 0.45f, 0.58f), new Vector3(0.1f, 0.1f, 0.14f), Color.white);
            mb.Cone(new Vector3(-0.08f, 0.6f, 0.4f), 0.05f, 0.14f, fur, 4);
            mb.Cone(new Vector3(0.08f, 0.6f, 0.4f), 0.05f, 0.14f, fur, 4);
            mb.Box(new Vector3(0, 0.4f, -0.55f), new Vector3(0.18f, 0.18f, 0.5f), fur);
            mb.Box(new Vector3(0, 0.4f, -0.82f), new Vector3(0.14f, 0.14f, 0.1f), Color.white);
            // the stolen basket
            mb.Cylinder(new Vector3(0.28f, 0.2f, 0.2f), 0.16f, 0.2f, ColorUtil.Hex("#C9A46A"), 8);
            mb.Sphere(new Vector3(0.28f, 0.42f, 0.2f), 0.1f, ColorUtil.Hex("#E0483E"), 2, 5);
            var go = MeshObject("Fox", mb.ToMesh("Fox"));
            go.transform.localScale = Vector3.one * 1.6f;
            go.transform.localPosition = new Vector3(0, 0, -9f);
            var w = go.AddComponent<Wanderer>();
            w.Setup(Environment.TickCount, new Vector2(-6f, AreaZ[0] + 0.5f), new Vector2(6f, AreaZ[1] - 1f), 3.2f, 0.15f);
            _fox = go.transform;
            _foxHits = 0;
            _foxTime = FoxSeconds;
            AddTappable(_fox, 1.6f, HitFox);
            FoxProgress?.Invoke(0, FoxTaps);
        }

        private void HitFox()
        {
            if (_fox == null) return;
            _foxHits++;
            var w = _fox.GetComponent<Wanderer>();
            if (w != null) w.Speed += 0.8f;
            if (_fox.GetComponent<Squash>() == null) _fox.gameObject.AddComponent<Squash>();
            FoxProgress?.Invoke(_foxHits, FoxTaps);
            if (_foxHits >= FoxTaps) EndFox(true);
        }

        private void EndFox(bool caught)
        {
            if (_fox == null) return;
            RemoveTappable(_fox);
            Destroy(_fox.gameObject);
            _fox = null;
            FoxEnded?.Invoke(caught);
        }

        protected override void Tick(float dt)
        {
            float t = Time.time;
            for (int i = 0; i < _glow.Count; i++)
                _glow[i].localPosition += new Vector3(Mathf.Sin(t * 1.3f + i) * 0.4f, Mathf.Cos(t * 1.7f + i * 2f) * 0.3f, Mathf.Cos(t * 0.9f + i) * 0.3f) * dt;
            if (_fox != null)
            {
                _foxTime -= dt;
                if (_foxTime <= 0f) EndFox(false);
            }
        }

        protected override void OnHidden()
        {
            if (_fox != null) EndFox(false);
        }
    }
}
