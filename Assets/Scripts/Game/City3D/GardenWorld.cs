using System;
using System.Collections.Generic;
using JellyDrop.Core.Islands;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>
    /// The garden island on the other side of the village: a fenced vegetable garden with 12 beds (more open
    /// as the garden levels up), crops that grow in front of you, a greenhouse, a well, a compost bin and a
    /// chicken coop with chickens pecking around, a scarecrow, flowers and a pond. Crows come now and then.
    /// </summary>
    public sealed class GardenWorld : IslandWorld
    {
        public static readonly Vector3 WorldOrigin = new Vector3(400f, 0f, 2000f);
        private const int Cols = 3;
        private const float Step = 2.6f;

        public event Action<int> BedTapped;
        public event Action<GardenBuilding> BuildingTapped;
        public event Action<int, int> CrowProgress;
        public event Action<bool> CrowsEnded;

        private GardenService _garden;

        private sealed class BedView
        {
            public Transform T;
            public GameObject Soil, WetSoil, Lock, Plant;
            public int ShownCrop = -2, ShownStage = -1;
            public GameObject Sparkle;
        }

        private sealed class BuildingView { public Transform T; public GameObject Model, Sign; public int Shown = -1; }

        private readonly BedView[] _beds = new BedView[GardenCatalog.MaxBeds];
        private readonly BuildingView[] _buildings = new BuildingView[GardenCatalog.Buildings.Length];
        private readonly List<Transform> _crows = new List<Transform>();
        private int _crowHits;
        private float _crowTime;
        private Transform _scarecrowArm;

        public const int CrowTaps = 4;
        public const float CrowSeconds = 15f;
        public bool CrowsActive => _crows.Count > 0;
        public float CrowTimeLeft => _crowTime;

        public void Setup(GardenService garden) => _garden = garden;

        public static Vector3 BedPos(int i)
        {
            int c = i % Cols, r = i / Cols;
            return new Vector3((c - 1) * Step, 0f, -6f + r * Step);
        }

        protected override void BuildWorld()
        {
            PanHalfWidth = 5f;
            PanZMin = -9f;
            PanZMax = 6f;
            ViewWidth = 14f;
            StartFocus = new Vector3(0f, 0f, -4f);

            var mb = new MeshBuilder();
            Island(mb, 9f, -13f, 12f, ColorUtil.Hex("#7CCB62"), ColorUtil.Hex("#F2D59B"), ColorUtil.Hex("#2F78C2"));
            // Garden ground and a fence around the beds
            mb.Box(new Vector3(0, 0.01f, -2.1f), new Vector3(9.4f, 0.02f, 11.6f), ColorUtil.Hex("#A8D978"));
            var fence = ColorUtil.Hex("#F2E6D0");
            for (float x = -4.6f; x <= 4.6f; x += 0.6f)
                foreach (var z in new[] { -7.8f, 3.6f })
                    mb.Box(new Vector3(x, 0, z), new Vector3(0.1f, 0.6f, 0.1f), fence);
            for (float z = -7.8f; z <= 3.6f; z += 0.6f)
                foreach (var x in new[] { -4.7f, 4.7f })
                    if (!(x < 0 && Mathf.Abs(z + 6f) < 0.8f)) mb.Box(new Vector3(x, 0, z), new Vector3(0.1f, 0.6f, 0.1f), fence);
            foreach (var z in new[] { -7.8f, 3.6f }) mb.Box(new Vector3(0, 0.42f, z), new Vector3(9.4f, 0.06f, 0.05f), fence);
            // Stone path from the bridge
            for (float x = 4.7f; x < 9f; x += 0.7f) mb.Box(new Vector3(x, 0.02f, -2f), new Vector3(0.6f, 0.04f, 0.9f), ColorUtil.Hex("#D9D2C3"));
            Bridge(mb, new Vector3(9.5f + 9f, 0.05f, -2f), 9f);   // bridge goes off to the right, towards the village
            // Pond with lily pads
            mb.Cylinder(new Vector3(-5.5f, -0.02f, 7f), 2.2f, 0.05f, ColorUtil.Hex("#4FB8FF"), 18);
            foreach (var p in new[] { new Vector3(-6f, 0.04f, 7.5f), new Vector3(-4.8f, 0.04f, 6.4f), new Vector3(-5.6f, 0.04f, 6.2f) })
                mb.Cylinder(p, 0.3f, 0.02f, ColorUtil.Hex("#3E9A44"), 8);
            // Flower rows and trees outside the fence
            Color[] flowers = { ColorUtil.Hex("#FF6F91"), ColorUtil.Hex("#FFE27A"), ColorUtil.Hex("#B48CFF"), Color.white };
            for (int i = 0; i < 60; i++)
            {
                var p = new Vector3(((float)Rng.NextDouble() * 2f - 1f) * 7.5f, 0, 4.5f + (float)Rng.NextDouble() * 6f);
                if (Vector3.Distance(p, new Vector3(-5.5f, 0, 7f)) < 2.6f) continue;
                mb.Box(p, new Vector3(0.04f, 0.22f, 0.04f), ColorUtil.Hex("#3E9A44"));
                mb.Sphere(p + Vector3.up * 0.26f, 0.09f, flowers[i % flowers.Length], 2, 5);
            }
            for (int i = 0; i < 26; i++)
            {
                float side = Rng.Next(2) == 0 ? -1f : 1f;
                var p = new Vector3(side * (7f + (float)Rng.NextDouble() * 1.4f), 0, -11f + (float)Rng.NextDouble() * 21f);
                if (side > 0 && Mathf.Abs(p.z + 2f) < 1.4f) continue;
                BuildingModels.RoundTree(mb, p, 1f + (float)Rng.NextDouble() * 0.5f);
            }
            mb.ToObject("GardenScenery", Root);

            // Scarecrow with a waving arm
            var sc = new MeshBuilder();
            sc.Box(new Vector3(0, 0, 0), new Vector3(0.1f, 1.6f, 0.1f), ColorUtil.Hex("#8A5A34"));
            sc.Box(new Vector3(0, 0.9f, 0), new Vector3(0.5f, 0.6f, 0.3f), ColorUtil.Hex("#3FB6FF"));
            sc.Sphere(new Vector3(0, 1.45f, 0), 0.22f, ColorUtil.Hex("#F2D59B"), 3, 7);
            sc.Cone(new Vector3(0, 1.6f, 0), 0.38f, 0.35f, ColorUtil.Hex("#E3A257"), 8);
            var scGo = sc.ToObject("Scarecrow", Root);
            scGo.transform.localPosition = new Vector3(3.4f, 0, 5.4f);
            var arm = new MeshBuilder();
            arm.Box(new Vector3(0.45f, 0, 0), new Vector3(0.9f, 0.1f, 0.1f), ColorUtil.Hex("#3FB6FF"));
            var armGo = arm.ToObject("Arm", scGo.transform);
            armGo.transform.localPosition = new Vector3(0.2f, 1.1f, 0);
            _scarecrowArm = armGo.transform;

            BuildBeds();
            BuildBuildings();
            BuildChickens();
        }

        private static Mesh _soil, _wet, _lock;

        private void BuildBeds()
        {
            var s = new MeshBuilder();
            s.Box(Vector3.zero, new Vector3(2.1f, 0.22f, 2.1f), ColorUtil.Hex("#8A5A34"));
            s.Box(new Vector3(0, 0.22f, 0), new Vector3(1.9f, 0.04f, 1.9f), ColorUtil.Hex("#6B4426"));
            for (int k = 0; k < 3; k++) s.Box(new Vector3(0, 0.25f, -0.6f + k * 0.6f), new Vector3(1.8f, 0.05f, 0.18f), ColorUtil.Hex("#5A3820"));
            _soil = s.ToMesh("Soil");
            var w = new MeshBuilder();
            w.Box(new Vector3(0, 0.23f, 0), new Vector3(1.9f, 0.05f, 1.9f), ColorUtil.Hex("#3E2A18"));
            _wet = w.ToMesh("WetSoil");
            var l = new MeshBuilder();
            l.Box(new Vector3(0, 0.25f, 0), new Vector3(2f, 0.06f, 2f), ColorUtil.Hex("#9EA3B5"));
            l.Box(new Vector3(0, 0.6f, 0), new Vector3(0.5f, 0.4f, 0.2f), ColorUtil.Hex("#FFC93C"));
            l.Cylinder(new Vector3(0, 0.8f, 0), 0.18f, 0.3f, ColorUtil.Hex("#C8CCDD"), 10, 0.18f, false);
            _lock = l.ToMesh("BedLock");
            for (int i = 0; i < _beds.Length; i++)
            {
                var t = new GameObject("Bed" + i).transform;
                t.SetParent(Root, false);
                t.localPosition = BedPos(i);
                var v = new BedView
                {
                    T = t,
                    Soil = MeshObject("Soil", _soil, t),
                    WetSoil = MeshObject("Wet", _wet, t, false),
                    Lock = MeshObject("Lock", _lock, t),
                };
                _beds[i] = v;
                int idx = i;
                AddTappable(t, 1.3f, () => BedTapped?.Invoke(idx), () => _garden != null && _garden.IsBedOpen(idx));
            }
        }

        private static readonly Vector3[] BuildingSpots =
        {
            new Vector3(-6.2f, 0, -5.2f), new Vector3(6.2f, 0, -7f), new Vector3(-6.2f, 0, 0.8f), new Vector3(6.2f, 0, 2.6f),
        };

        private void BuildBuildings()
        {
            for (int i = 0; i < _buildings.Length; i++)
            {
                var t = new GameObject("GardenBuilding" + i).transform;
                t.SetParent(Root, false);
                t.localPosition = BuildingSpots[i];
                var sign = new MeshBuilder();
                sign.Box(new Vector3(0, 0.02f, 0), new Vector3(2.6f, 0.04f, 2.6f), ColorUtil.Hex("#C9A46A"));
                sign.Box(Vector3.zero, new Vector3(0.1f, 1.1f, 0.1f), ColorUtil.Hex("#6E4826"));
                sign.Box(new Vector3(0, 1f, 0.06f), new Vector3(0.9f, 0.5f, 0.08f), ColorUtil.Hex("#E8C590"));
                sign.Box(new Vector3(0, 1f, 0.11f), new Vector3(0.5f, 0.1f, 0.02f), ColorUtil.Hex("#FF9F43"));
                sign.Box(new Vector3(0, 1f, 0.11f), new Vector3(0.1f, 0.4f, 0.02f), ColorUtil.Hex("#FF9F43"));
                var v = new BuildingView { T = t, Sign = sign.ToObject("Sign", t) };
                _buildings[i] = v;
                var b = (GardenBuilding)i;
                AddTappable(t, 1.5f, () => BuildingTapped?.Invoke(b));
            }
        }

        private void BuildChickens()
        {
            var mb = new MeshBuilder();
            mb.Sphere(new Vector3(0, 0.25f, 0), 0.2f, Color.white, 3, 6);
            mb.Sphere(new Vector3(0, 0.42f, 0.14f), 0.11f, Color.white, 3, 6);
            mb.Box(new Vector3(0, 0.52f, 0.14f), new Vector3(0.04f, 0.07f, 0.1f), ColorUtil.Hex("#FF5A6E"));
            mb.Box(new Vector3(0, 0.4f, 0.27f), new Vector3(0.06f, 0.04f, 0.07f), ColorUtil.Hex("#FFC93C"));
            mb.Box(new Vector3(0.06f, 0, 0), new Vector3(0.03f, 0.12f, 0.03f), ColorUtil.Hex("#FF9F43"));
            mb.Box(new Vector3(-0.06f, 0, 0), new Vector3(0.03f, 0.12f, 0.03f), ColorUtil.Hex("#FF9F43"));
            var mesh = mb.ToMesh("Chicken");
            for (int i = 0; i < 4; i++)
            {
                var go = MeshObject("Chicken", mesh);
                go.transform.localPosition = new Vector3(5.5f, 0, 4f);
                go.AddComponent<Wanderer>().Setup(100 + i, new Vector2(4.9f, 0.5f), new Vector2(8f, 5.5f), 0.8f, 0.06f);
                _chickens.Add(go);
            }
        }

        private readonly List<GameObject> _chickens = new List<GameObject>();

        // ------------------------------------------------------------------ models

        private static Mesh CropMesh(CropType t, int stage)
        {
            var mb = new MeshBuilder();
            var leaf = ColorUtil.Hex("#4FB85A");
            var rng = new System.Random((int)t * 10 + stage);
            for (int k = 0; k < 6; k++)
            {
                var p = new Vector3(-0.55f + (k % 3) * 0.55f, 0.27f, -0.4f + (k / 3) * 0.8f);
                if (stage == 0)
                {
                    mb.Box(p, new Vector3(0.05f, 0.18f, 0.05f), leaf);
                    mb.Box(p + new Vector3(0.06f, 0.16f, 0), new Vector3(0.12f, 0.04f, 0.08f), leaf);
                    continue;
                }
                float g = stage == 1 ? 0.6f : 1f;
                switch (t)
                {
                    case CropType.Carrot:
                        for (int j = -1; j <= 1; j++) mb.Box(p + new Vector3(j * 0.06f, 0, 0), new Vector3(0.05f, 0.4f * g, 0.05f), leaf);
                        if (stage == 2) mb.Cone(p + new Vector3(0, -0.02f, 0), 0.1f, 0.12f, ColorUtil.Hex("#FF8A2A"), 6);
                        break;
                    case CropType.Tomato:
                        mb.Box(p, new Vector3(0.05f, 0.7f * g, 0.05f), ColorUtil.Hex("#3E7A3A"));
                        mb.Sphere(p + Vector3.up * 0.55f * g, 0.2f * g, leaf, 3, 6);
                        if (stage == 2) { mb.Sphere(p + new Vector3(0.14f, 0.45f, 0.1f), 0.09f, ColorUtil.Hex("#E0304A"), 3, 6); mb.Sphere(p + new Vector3(-0.12f, 0.35f, -0.08f), 0.08f, ColorUtil.Hex("#E0304A"), 3, 6); }
                        break;
                    case CropType.Strawberry:
                        mb.Sphere(p + Vector3.up * 0.08f, 0.18f * g, leaf, 3, 6);
                        if (stage == 2) foreach (var d in new[] { new Vector3(0.14f, 0.06f, 0.1f), new Vector3(-0.12f, 0.05f, 0.12f) }) mb.Cone(p + d, 0.06f, -0.1f, ColorUtil.Hex("#E0304A"), 5);
                        break;
                    case CropType.Pumpkin:
                        if (k % 2 == 0)
                        {
                            mb.Box(p, new Vector3(0.5f, 0.04f, 0.04f), leaf);
                            if (stage == 2) mb.Sphere(p + Vector3.up * 0.18f, 0.24f, ColorUtil.Hex("#FF8A2A"), 4, 8);
                            else mb.Sphere(p + Vector3.up * 0.1f, 0.12f, ColorUtil.Hex("#9BDB4D"), 3, 6);
                        }
                        else mb.Sphere(p + Vector3.up * 0.06f, 0.14f, leaf, 2, 5);
                        break;
                    default: // sunflower
                        mb.Box(p, new Vector3(0.05f, 0.9f * g, 0.05f), ColorUtil.Hex("#3E7A3A"));
                        if (stage == 2)
                        {
                            mb.Cylinder(p + new Vector3(0, 0.9f, -0.05f), 0.18f, 0.05f, ColorUtil.Hex("#FFC93C"), 10);
                            mb.Cylinder(p + new Vector3(0, 0.9f, -0.07f), 0.09f, 0.06f, ColorUtil.Hex("#6B3A1A"), 8);
                        }
                        else mb.Sphere(p + Vector3.up * 0.9f * g, 0.08f, leaf, 2, 5);
                        break;
                }
            }
            return mb.ToMesh("Crop" + t + stage);
        }

        private static readonly Dictionary<int, Mesh> CropMeshes = new Dictionary<int, Mesh>();

        private static Mesh GetCropMesh(CropType t, int stage)
        {
            int key = (int)t * 10 + stage;
            if (!CropMeshes.TryGetValue(key, out var m)) CropMeshes[key] = m = CropMesh(t, stage);
            return m;
        }

        private static Mesh BuildingMesh(GardenBuilding b, int level)
        {
            var mb = new MeshBuilder();
            var wood = ColorUtil.Hex("#A86F43");
            var white = ColorUtil.Hex("#F4E6CF");
            float s = 0.85f + 0.15f * level;
            switch (b)
            {
                case GardenBuilding.Greenhouse:
                    mb.Box(Vector3.zero, new Vector3(2.2f * s, 0.2f, 1.6f), ColorUtil.Hex("#B9BCCB"));
                    mb.Box(new Vector3(0, 0.2f, 0), new Vector3(2.1f * s, 1f, 1.5f), ColorUtil.Hex("#9FE3FF"));
                    mb.Roof(new Vector3(0, 1.2f, 0), new Vector3(2.2f * s, 0.7f, 1.6f), ColorUtil.Hex("#C8F2FF"));
                    for (int i = 0; i < 3 + level; i++) mb.Sphere(new Vector3(-0.8f * s + i * 0.45f, 0.45f, 0), 0.15f, ColorUtil.Hex(i % 2 == 0 ? "#4FB85A" : "#E0304A"), 2, 5);
                    break;
                case GardenBuilding.Well:
                    mb.Cylinder(Vector3.zero, 0.6f, 0.6f, ColorUtil.Hex("#9EA3B5"), 12);
                    mb.Cylinder(new Vector3(0, 0.58f, 0), 0.5f, 0.04f, ColorUtil.Hex("#4FB8FF"), 12);
                    foreach (var x in new[] { -0.55f, 0.55f }) mb.Box(new Vector3(x, 0.6f, 0), new Vector3(0.08f, 0.9f * s, 0.08f), wood);
                    mb.Roof(new Vector3(0, 0.6f + 0.9f * s, 0), new Vector3(1.4f, 0.5f, 1f), ColorUtil.Hex("#E0573F"));
                    if (level >= 2) mb.Cylinder(new Vector3(0.9f, 0, 0.5f), 0.22f, 0.4f, wood, 8);
                    break;
                case GardenBuilding.Compost:
                    for (int i = 0; i < Math.Min(3, level); i++)
                    {
                        var p = new Vector3(-0.8f + i * 0.8f, 0, 0);
                        mb.Box(p, new Vector3(0.7f, 0.7f, 0.9f), wood);
                        mb.Box(p + new Vector3(0, 0.7f, 0), new Vector3(0.6f, 0.08f, 0.8f), ColorUtil.Hex("#5A3820"));
                        mb.Sphere(p + new Vector3(0, 0.75f, 0), 0.25f, ColorUtil.Hex("#6B8A3A"), 3, 6);
                    }
                    break;
                default: // coop
                    mb.Box(new Vector3(0, 0.3f, 0), new Vector3(1.6f * s, 0.9f, 1.2f), ColorUtil.Hex("#E0573F"));
                    foreach (var x in new[] { -0.7f, 0.7f }) foreach (var z in new[] { -0.5f, 0.5f }) mb.Box(new Vector3(x * s, 0, z), new Vector3(0.1f, 0.3f, 0.1f), wood);
                    mb.Roof(new Vector3(0, 1.2f, 0), new Vector3(1.8f * s, 0.6f, 1.4f), white);
                    mb.Box(new Vector3(0, 0.35f, -0.62f), new Vector3(0.35f, 0.45f, 0.04f), ColorUtil.Hex("#3A2410"));
                    mb.Box(new Vector3(0, 0.05f, -0.95f), new Vector3(0.3f, 0.04f, 0.7f), wood);
                    if (level >= 3) mb.Box(new Vector3(0, 1.7f, 0), new Vector3(0.05f, 0.4f, 0.05f), wood);
                    break;
            }
            return mb.ToMesh("Garden" + b + level);
        }

        private static Mesh _crowMesh;
        private static Mesh CrowMesh()
        {
            if (_crowMesh != null) return _crowMesh;
            var mb = new MeshBuilder();
            var black = ColorUtil.Hex("#1E2140");
            mb.Sphere(new Vector3(0, 0.25f, 0), 0.2f, black, 3, 6);
            mb.Sphere(new Vector3(0, 0.42f, 0.16f), 0.12f, black, 3, 6);
            mb.Cone(new Vector3(0, 0.4f, 0.26f), 0.05f, 0.14f, ColorUtil.Hex("#FFC93C"), 4);
            mb.Box(new Vector3(0.22f, 0.3f, -0.02f), new Vector3(0.3f, 0.04f, 0.18f), black);
            mb.Box(new Vector3(-0.22f, 0.3f, -0.02f), new Vector3(0.3f, 0.04f, 0.18f), black);
            _crowMesh = mb.ToMesh("Crow");
            return _crowMesh;
        }

        // ------------------------------------------------------------------ sync

        public override void Sync()
        {
            if (_garden == null) return;
            for (int i = 0; i < _beds.Length; i++)
            {
                var v = _beds[i];
                bool open = _garden.IsBedOpen(i);
                v.Lock.SetActive(!open);
                v.Soil.SetActive(true);
                var crop = open ? _garden.CropIn(i) : null;
                v.WetSoil.SetActive(open && crop.HasValue && _garden.IsWatered(i) && !_garden.IsRipe(i));
                int stage = !crop.HasValue ? -1 : _garden.IsRipe(i) ? 2 : _garden.Growth(i) < 0.4f ? 0 : 1;
                int cropId = crop.HasValue ? (int)crop.Value : -1;
                if (cropId != v.ShownCrop || stage != v.ShownStage)
                {
                    bool ripened = stage == 2 && v.ShownStage == 1;
                    v.ShownCrop = cropId;
                    v.ShownStage = stage;
                    if (v.Plant != null) Destroy(v.Plant);
                    if (crop.HasValue)
                    {
                        v.Plant = MeshObject("Plant", GetCropMesh(crop.Value, stage), v.T);
                        if (ripened || stage == 0) v.Plant.AddComponent<Squash>();
                        if (stage == 2) { var h = v.Plant.AddComponent<Hover>(); }
                    }
                }
            }
            for (int i = 0; i < _buildings.Length; i++)
            {
                var v = _buildings[i];
                int level = _garden.BuildingLevel((GardenBuilding)i);
                v.Sign.SetActive(level == 0);
                if (level != v.Shown)
                {
                    bool grew = v.Shown >= 0 && level > v.Shown;
                    v.Shown = level;
                    if (v.Model != null) Destroy(v.Model);
                    if (level > 0)
                    {
                        v.Model = MeshObject("Model", BuildingMesh((GardenBuilding)i, level), v.T);
                        if (grew) v.Model.AddComponent<Squash>();
                    }
                }
            }
            bool coop = _garden.BuildingLevel(GardenBuilding.Coop) > 0;
            foreach (var c in _chickens) c.SetActive(coop);
        }

        public Vector3 BedScreen(int bed) => ScreenOf(_beds[bed].T);

        public void PopBed(int bed)
        {
            if (_beds[bed].Plant != null) _beds[bed].Plant.AddComponent<Squash>();
        }

        // ------------------------------------------------------------------ crows

        public void BeginCrows()
        {
            if (CrowsActive) return;
            _crowHits = 0;
            _crowTime = CrowSeconds;
            for (int i = 0; i < 2; i++) SpawnCrow();
            CrowProgress?.Invoke(0, CrowTaps);
        }

        private void SpawnCrow()
        {
            var go = MeshObject("Crow", CrowMesh());
            go.transform.localScale = Vector3.one * 1.8f;
            int bed = Rng.Next(_garden.OpenBeds);
            go.transform.localPosition = BedPos(bed) + new Vector3(((float)Rng.NextDouble() - 0.5f), 0.3f, ((float)Rng.NextDouble() - 0.5f));
            _crows.Add(go.transform);
            var t = go.transform;
            AddTappable(t, 1.2f, () => HitCrow(t));
        }

        private void HitCrow(Transform crow)
        {
            RemoveTappable(crow);
            _crows.Remove(crow);
            // Flies off
            var p = crow.gameObject.AddComponent<Puff>();
            p.Velocity = new Vector3(((float)Rng.NextDouble() - 0.5f) * 4f, 6f, 3f);
            _crowHits++;
            CrowProgress?.Invoke(_crowHits, CrowTaps);
            if (_crowHits >= CrowTaps) EndCrows(true);
            else SpawnCrow();
        }

        private void EndCrows(bool chased)
        {
            foreach (var c in _crows) if (c != null) { RemoveTappable(c); Destroy(c.gameObject); }
            _crows.Clear();
            CrowsEnded?.Invoke(chased);
        }

        protected override void Tick(float dt)
        {
            if (_scarecrowArm != null) _scarecrowArm.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 2f) * 25f);
            if (CrowsActive)
            {
                _crowTime -= dt;
                foreach (var c in _crows) if (c != null) c.localPosition += new Vector3(0, Mathf.Sin(Time.time * 12f + c.localPosition.x) * 0.01f, 0);
                if (_crowTime <= 0f) EndCrows(false);
            }
        }

        protected override void OnHidden()
        {
            if (CrowsActive) EndCrows(false);
        }
    }
}
