using System;
using System.Collections.Generic;
using JellyDrop.Core.City;
using JellyDrop.GameInput;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>
    /// The 3D village: a little island with 12 plots (3 across, 4 deep), paths, trees and water, seen from a
    /// tilted camera like Ikariam or Grepolis. Buildings are procedural low-poly models that rise with
    /// scaffolding when built or upgraded, the windmill turns, villagers walk the paths, and buildings with
    /// something to collect show a floating icon. Drag to pan, pinch / mouse wheel to zoom, tap a plot.
    /// Everything lives far from the puzzle (z = 2000) and has its own camera, shown only on the city screen.
    /// </summary>
    public sealed class CityWorld : MonoBehaviour
    {
        public const int Cols = 3;
        /// <summary>Rows of plots the island can grow to.</summary>
        public static int MaxRows => (CityCatalog.MaxPlots + Cols - 1) / Cols;
        public const float PlotSize = 3f, Gap = 1.2f, Step = PlotSize + Gap;
        private static readonly Vector3 Origin = new Vector3(0f, 0f, 2000f);

        /// <summary>Raised when the player taps a plot (index into CityService plots).</summary>
        public event Action<int> PlotTapped;

        public bool IsActive { get; private set; }

        private CityService _city;
        private Camera _mainCamera;
        private Camera _cam;
        private Transform _root;
        private readonly PlotView[] _plots = new PlotView[CityCatalog.MaxPlots];
        /// <summary>Rows currently on the island (unlocked rows plus one locked row to look forward to).</summary>
        private int _rows;
        private GameObject _terrain;
        private readonly List<Villager> _villagers = new List<Villager>();
        private readonly List<Vector3> _nodes = new List<Vector3>();
        private readonly List<List<int>> _links = new List<List<int>>();
        private bool _synced;
        private float _iconTimer;
        private readonly System.Random _rng = new System.Random(7);

        // Village missions
        public event Action<int, int> MissionProgress;
        public event Action<bool> MissionEnded;
        public bool MissionActive { get; private set; }
        public float MissionTimeLeft { get; private set; }
        private JellyDrop.Core.City.CityMissionType _missionType;
        private int _missionHits, _missionNeeded;
        private readonly List<Transform> _targets = new List<Transform>();
        private float _spawnTimer, _catTimer;
        private readonly Dictionary<Transform, float> _mushroomLife = new Dictionary<Transform, float>();

        // Camera rig
        private Vector3 _target;
        private float _zoom = 1f;
        private const float Pitch = 52f, Fov = 38f;

        // Input
        private bool _pressed, _ignore, _dragging;
        private Vector2 _pressPos, _lastPos;
#if ENABLE_LEGACY_INPUT_MANAGER
        private bool _pinching;
        private float _pinchStart, _zoomStart;
#endif

        private sealed class PlotView
        {
            public Transform Anchor;
            public GameObject Content;
            public Transform Building;
            public Transform Blades;
            public GameObject Scaffold;
            public SpriteRenderer Icon;
            public string ShownId = "?";
            public int ShownLevel = -1;
            public bool ShownLocked;
            public int ShownTarget = -1;
            public GameObject Gear;
            public float Anim = -1f;
        }

        // ------------------------------------------------------------------ setup

        public void Init(CityService city, Camera mainCamera)
        {
            _city = city;
            _mainCamera = mainCamera;
            _root = new GameObject("City3D").transform;
            _root.SetParent(transform, false);
            _root.position = Origin;

            var camGo = new GameObject("CityCamera");
            camGo.transform.SetParent(transform, false);
            _cam = camGo.AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = WorldPalette.CitySky;
            _cam.fieldOfView = Fov;
            _cam.nearClipPlane = 0.3f;
            _cam.farClipPlane = 250f;
            _cam.depth = 5;
            _cam.enabled = false;

            var sunGo = new GameObject("Sun");
            sunGo.transform.SetParent(_root, false);
            sunGo.transform.rotation = Quaternion.Euler(48f, -35f, 0f);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            WorldPalette.SetupSun(sun);
            sunGo.SetActive(false);
            _sun = sunGo;

            for (int i = 0; i < _plots.Length; i++)
            {
                var anchor = new GameObject("Plot" + i).transform;
                anchor.SetParent(_root, false);
                anchor.localPosition = PlotCenter(i);
                _plots[i] = new PlotView { Anchor = anchor };
            }
            _root.gameObject.SetActive(false);
        }

        private GameObject _sun;

        /// <summary>Row 0 is nearest the camera; the island grows away from it.</summary>
        public static Vector3 PlotCenter(int index)
        {
            int c = index % Cols, r = index / Cols;
            return new Vector3((c - (Cols - 1) / 2f) * Step, 0f, (r - 1.5f) * Step);
        }

        private int WantedRows()
        {
            int unlockedRows = (_city.UnlockedPlots + Cols - 1) / Cols;
            return Mathf.Clamp(unlockedRows + 1, 2, MaxRows);
        }

        public void Show()
        {
            if (IsActive) return;
            IsActive = true;
            _root.gameObject.SetActive(true);
            _sun.SetActive(true);
            _cam.enabled = true;
            if (_mainCamera != null) _mainCamera.enabled = false;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = WorldPalette.Ambient;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = _cam.backgroundColor;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 110f;
            QualitySettings.shadowDistance = 70f;

            _target = new Vector3(0f, 0f, -1.2f);
            _zoom = 1f;
            Sync(animate: false);
            UpdateCamera();
        }

        public void Hide()
        {
            if (!IsActive) return;
            IsActive = false;
            if (MissionActive) EndMission(false);
            _cam.enabled = false;
            _sun.SetActive(false);
            _root.gameObject.SetActive(false);
            RenderSettings.fog = false;
            if (_mainCamera != null) _mainCamera.enabled = true;
            _pressed = false;
        }

        // ------------------------------------------------------------------ static scenery

        /// <summary>(Re)builds the island, paths and scenery for the current number of rows.</summary>
        private void BuildTerrain(int rows)
        {
            if (_terrain != null) Destroy(_terrain);
            _terrain = new GameObject("Terrain");
            _terrain.transform.SetParent(_root, false);
            BuildIsland(rows);
            BuildPaths(rows);
        }

        private float GridFront => -2f * Step;
        private float GridBack(int rows) => (rows - 2f) * Step;

        private void BuildIsland(int rows)
        {
            // A long rounded island (stadium shape) that grows with the rows of plots.
            float zFront = GridFront - 2.6f, zBack = GridBack(rows) + 2.6f;
            float halfW = 8.6f, zc = (zFront + zBack) / 2f, len = Mathf.Max(0.1f, (zBack - zFront) - 2f * halfW);
            var mb = new MeshBuilder();
            mb.Disc(new Vector3(0, -0.45f, zc), 140f, ColorUtil.Hex("#2F78C2"), 48);
            Stadium(mb, zc, len, halfW + 1.3f, -0.6f, 0.45f, ColorUtil.Hex("#F2D59B"));   // beach
            Stadium(mb, zc, len, halfW, -0.6f, 0.6f, ColorUtil.Hex("#6CC56F"));          // grass
            mb.ToObject("Island", _terrain.transform, castShadows: false);

            // Trees and rocks around the edge (outside the plot grid)
            var deco = new MeshBuilder();
            int trees = 30 + rows * 6;
            for (int i = 0; i < trees * 2; i++)
            {
                var p = new Vector3(((float)_rng.NextDouble() * 2f - 1f) * (halfW - 0.6f), 0f,
                    zFront + 0.6f + (float)_rng.NextDouble() * (zBack - zFront - 1.2f));
                // Inside the stadium?
                float dz = p.z < zc - len / 2f ? p.z - (zc - len / 2f) : p.z > zc + len / 2f ? p.z - (zc + len / 2f) : 0f;
                if (p.x * p.x + dz * dz > (halfW - 0.6f) * (halfW - 0.6f)) continue;
                if (Mathf.Abs(p.x) < 7.0f && p.z > GridFront - 0.6f && p.z < GridBack(rows) + 0.6f) continue;
                float s = 0.8f + (float)_rng.NextDouble() * 0.6f;
                if (_rng.NextDouble() < 0.55) BuildingModels.Tree(deco, p, s);
                else if (_rng.NextDouble() < 0.7) BuildingModels.RoundTree(deco, p, s);
                else deco.Sphere(p + Vector3.up * 0.15f, 0.35f * s, ColorUtil.Hex("#A3A7B8"), 3, 6);
            }
            // A couple of boats on the water
            foreach (var p in new[] { new Vector3(-17f, -0.45f, 6f), new Vector3(16f, -0.45f, -9f) })
            {
                deco.Box(p, new Vector3(1.6f, 0.35f, 0.6f), ColorUtil.Hex("#A86F43"));
                deco.Box(p + new Vector3(0, 0.35f, 0), new Vector3(0.06f, 1.2f, 0.06f), ColorUtil.Hex("#7A4E2E"));
                deco.Triangle(p + new Vector3(0.05f, 1.5f, 0), p + new Vector3(0.75f, 0.45f, 0), p + new Vector3(0.05f, 0.45f, 0), Color.white);
                deco.Triangle(p + new Vector3(0.05f, 1.5f, 0), p + new Vector3(0.05f, 0.45f, 0), p + new Vector3(0.75f, 0.45f, 0), Color.white);
            }
            deco.ToObject("Scenery", _terrain.transform);
        }

        /// <summary>Flat-topped stadium (rectangle with round ends along z).</summary>
        private static void Stadium(MeshBuilder mb, float zc, float len, float r, float y, float h, Color col)
        {
            mb.Box(new Vector3(0, y, zc), new Vector3(r * 2f, h, len), col, col);
            mb.Cylinder(new Vector3(0, y, zc - len / 2f), r, h, col, 28);
            mb.Cylinder(new Vector3(0, y, zc + len / 2f), r, h, col, 28);
        }

        private void BuildPaths(int rows)
        {
            // Path network on the lines between plots; nodes at the crossings are where villagers walk.
            var mb = new MeshBuilder();
            var stone = ColorUtil.Hex("#E6D7B5");
            float halfX = Cols / 2f * Step;
            float zFront = GridFront, zBack = GridBack(rows);
            for (int k = 0; k <= Cols; k++)
            {
                float x = (k - Cols / 2f) * Step;
                mb.Box(new Vector3(x, -0.02f, (zFront + zBack) / 2f), new Vector3(1.0f, 0.05f, zBack - zFront + 1f), stone);
            }
            for (int m = 0; m <= rows; m++)
            {
                float z = zFront + m * Step;
                mb.Box(new Vector3(0, -0.02f, z), new Vector3(halfX * 2f + 1f, 0.05f, 1.0f), stone);
            }
            mb.ToObject("Paths", _terrain.transform, castShadows: false);

            _nodes.Clear();
            _links.Clear();
            for (int m = 0; m <= rows; m++)
                for (int k = 0; k <= Cols; k++)
                {
                    _nodes.Add(new Vector3((k - Cols / 2f) * Step, 0f, zFront + m * Step));
                    _links.Add(new List<int>());
                }
            int W = Cols + 1;
            for (int m = 0; m <= rows; m++)
                for (int k = 0; k <= Cols; k++)
                {
                    int i = m * W + k;
                    if (k < Cols) { _links[i].Add(i + 1); _links[i + 1].Add(i); }
                    if (m < rows) { _links[i].Add(i + W); _links[i + W].Add(i); }
                }
        }

        // ------------------------------------------------------------------ syncing with the city

        /// <summary>Rebuilds plots whose building, level or lock state changed. New buildings rise with scaffolding.</summary>
        public void Sync(bool animate = true)
        {
            if (_city == null) return;
            _city.CheckConstruction();
            int rows = WantedRows();
            if (rows != _rows)
            {
                _rows = rows;
                BuildTerrain(rows);
                // The path network changed: send everyone out again.
                foreach (var v in _villagers) if (v != null) Destroy(v.gameObject);
                _villagers.Clear();
            }
            for (int i = 0; i < _plots.Length; i++)
            {
                var view = _plots[i];
                bool visible = i / Cols < _rows;
                view.Anchor.gameObject.SetActive(visible);
                if (!visible) continue;
                bool locked = !_city.IsPlotUnlocked(i);
                var def = locked ? null : _city.BuildingAt(i);
                string id = def?.Id ?? "";
                var plot = _city.Plot(i);
                int level = def != null ? plot.level : 0;
                int target = def != null && plot.buildEnds > 0 ? plot.targetLevel : 0;
                if (view.ShownLocked == locked && view.ShownId == id && view.ShownLevel == level && view.ShownTarget == target) continue;
                bool grew = _synced && animate && def != null && level > 0 && (view.ShownId != id || level > view.ShownLevel) && target == 0;
                view.ShownLocked = locked;
                view.ShownId = id;
                view.ShownLevel = level;
                view.ShownTarget = target;
                BuildPlot(i, view, locked, def, level, grew, target);
            }
            _synced = true;
            UpdateVillagerCount();
            UpdateIcons();
        }

        private void BuildPlot(int index, PlotView view, bool locked, BuildingDef def, int level, bool grew, int target = 0)
        {
            if (view.Content != null) Destroy(view.Content);
            view.Content = new GameObject("Content");
            view.Content.transform.SetParent(view.Anchor, false);
            view.Building = null;
            view.Blades = null;
            view.Scaffold = null;

            var mb = new MeshBuilder();
            if (locked)
            {
                // Overgrown plot with a little fence
                mb.Box(new Vector3(0, -0.05f, 0), new Vector3(PlotSize, 0.08f, PlotSize), ColorUtil.Hex("#4E9E58"));
                for (int k = 0; k < 4; k++)
                {
                    float t = -1.3f + k * 0.866f;
                    foreach (var z in new[] { -1.35f, 1.35f }) mb.Box(new Vector3(t, 0, z), new Vector3(0.1f, 0.5f, 0.1f), ColorUtil.Hex("#A86F43"));
                    foreach (var x in new[] { -1.35f, 1.35f }) mb.Box(new Vector3(x, 0, t), new Vector3(0.1f, 0.5f, 0.1f), ColorUtil.Hex("#A86F43"));
                }
                mb.Box(new Vector3(0, 0.3f, -1.35f), new Vector3(2.8f, 0.08f, 0.06f), ColorUtil.Hex("#C98552"));
                mb.Box(new Vector3(0, 0.3f, 1.35f), new Vector3(2.8f, 0.08f, 0.06f), ColorUtil.Hex("#C98552"));
                mb.Box(new Vector3(-1.35f, 0.3f, 0), new Vector3(0.06f, 0.08f, 2.8f), ColorUtil.Hex("#C98552"));
                mb.Box(new Vector3(1.35f, 0.3f, 0), new Vector3(0.06f, 0.08f, 2.8f), ColorUtil.Hex("#C98552"));
                BuildingModels.Bush(mb, new Vector3(-0.5f, 0, 0.3f), 0.35f);
                BuildingModels.Bush(mb, new Vector3(0.6f, 0, -0.4f), 0.28f);
                mb.ToObject("Locked", view.Content.transform);
                view.Icon = MakeIcon(view.Content.transform, SpriteFactory.LockIcon(), 0.55f, 1.4f);
                view.Icon.color = new Color(1f, 1f, 1f, 0.85f);
                return;
            }

            if (def == null)
            {
                // Empty plot: tilled soil and a hovering "+" to invite building
                mb.Box(new Vector3(0, -0.04f, 0), new Vector3(PlotSize - 0.2f, 0.08f, PlotSize - 0.2f), ColorUtil.Hex("#B07F52"));
                for (int k = 0; k < 4; k++) mb.Box(new Vector3(0, 0.04f, -1.0f + k * 0.66f), new Vector3(2.4f, 0.05f, 0.18f), ColorUtil.Hex("#8E6440"));
                mb.ToObject("Soil", view.Content.transform, castShadows: false);
                var plus = new MeshBuilder();
                var mint = ColorUtil.Hex("#3DDBA8");
                plus.Box(new Vector3(0, -0.12f, 0), new Vector3(0.8f, 0.24f, 0.24f), mint);
                plus.Box(new Vector3(0, -0.4f, 0), new Vector3(0.24f, 0.8f, 0.24f), mint);
                var p = plus.ToObject("Plus", view.Content.transform);
                p.transform.localPosition = new Vector3(0, 1.3f, 0);
                p.AddComponent<Hover>().Spin = true;
                view.Icon = null;
                return;
            }

            BuildingModels.Build(mb, def.Id, Mathf.Max(1, target > 0 ? target : level));
            var building = mb.ToObject(def.Id, view.Content.transform);
            view.Building = building.transform;
            view.Gear = null;
            if (target > 0)
            {
                // Under construction: scaffolding stays up, the building grows with the progress, a gear spins above.
                var sc = new MeshBuilder();
                BuildingModels.Scaffold(sc, 2.4f + Mathf.Min(3, target) * 0.4f);
                view.Scaffold = sc.ToObject("Scaffold", view.Content.transform);
                view.Gear = MakeIcon(view.Content.transform, SpriteFactory.GearIcon(), 0.8f, 3.8f).gameObject;
                view.Gear.GetComponent<Hover>().Spin = false;
                UpdateConstruction(index, view);
            }
            if (def.Id == "windmill")
            {
                var blades = new MeshBuilder();
                BuildingModels.WindmillBlades(blades);
                var b = blades.ToObject("Blades", building.transform);
                b.transform.localPosition = new Vector3(0, BuildingModels.WindmillHubHeight(level), -0.72f);
                view.Blades = b.transform;
            }
            view.Icon = MakeIcon(view.Content.transform, JellyDrop.Interface.CityUi.ProductIcon(def.Produces), 0.9f, 3.6f);
            view.Icon.gameObject.SetActive(false);

            if (grew)
            {
                var sc = new MeshBuilder();
                BuildingModels.Scaffold(sc, 2.4f + level * 0.4f);
                view.Scaffold = sc.ToObject("Scaffold", view.Content.transform);
                view.Anim = 0f;
                view.Building.localScale = new Vector3(1f, 0.05f, 1f);
                Puffs(view.Anchor.position);
            }
            else view.Anim = -1f;
        }

        private void UpdateConstruction(int index, PlotView view)
        {
            if (view.Building == null || view.ShownTarget <= 0) return;
            float p = _city.ConstructionProgress(index);
            // New buildings rise from the ground; upgrades stay full size.
            float y = view.ShownLevel == 0 ? Mathf.Lerp(0.15f, 0.9f, p) : 1f;
            view.Building.localScale = new Vector3(1f, y, 1f);
        }

        private SpriteRenderer MakeIcon(Transform parent, Sprite sprite, float scale, float height)
        {
            var go = new GameObject("Icon");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0, height, 0);
            go.transform.localScale = Vector3.one * scale;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 50;
            go.AddComponent<Hover>().Billboard = true;
            return sr;
        }

        private void UpdateIcons()
        {
            for (int i = 0; i < _plots.Length; i++)
            {
                var v = _plots[i];
                if (v.ShownTarget > 0) UpdateConstruction(i, v);
                if (v.Icon == null || v.ShownLocked || string.IsNullOrEmpty(v.ShownId)) continue;
                int pending = _city.Pending(i);
                bool full = _city.StorageFill(i) >= 1f;
                v.Icon.gameObject.SetActive(pending > 0 && v.Anim < 0f);
                v.Icon.transform.localScale = Vector3.one * (full ? 1.15f : 0.85f);
            }
        }

        private void Puffs(Vector3 at)
        {
            var mb = new MeshBuilder();
            mb.Box(Vector3.zero, new Vector3(0.25f, 0.25f, 0.25f), ColorUtil.Hex("#E9E3D6"));
            var mesh = mb.ToMesh("Puff");
            for (int k = 0; k < 10; k++)
            {
                var go = new GameObject("Puff");
                go.transform.SetParent(_root, false);
                go.transform.position = at + new Vector3((float)_rng.NextDouble() * 2.4f - 1.2f, 0.2f, (float)_rng.NextDouble() * 2.4f - 1.2f);
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = MeshBuilder.SharedMaterial;
                var p = go.AddComponent<Puff>();
                p.Velocity = new Vector3((float)_rng.NextDouble() - 0.5f, 1.2f + (float)_rng.NextDouble(), (float)_rng.NextDouble() - 0.5f);
            }
        }

        private static Mesh _smokeMesh, _sparkMesh;

        /// <summary>A soft grey puff rising from a chimney.</summary>
        private void Smoke(Vector3 at)
        {
            if (_smokeMesh == null)
            {
                var mb = new MeshBuilder();
                mb.Sphere(Vector3.zero, 0.16f, ColorUtil.Hex("#D9DCE8"), 3, 6);
                _smokeMesh = mb.ToMesh("Smoke");
            }
            var go = new GameObject("Smoke");
            go.transform.SetParent(_root, false);
            go.transform.position = at;
            go.AddComponent<MeshFilter>().sharedMesh = _smokeMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = MeshBuilder.SharedMaterial;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            var p = go.AddComponent<Puff>();
            p.Velocity = new Vector3(0.15f + (float)_rng.NextDouble() * 0.1f, 0.7f, 0.05f);
        }

        /// <summary>Gold sparks shooting up when a building finishes an upgrade.</summary>
        private void GoldBurst(Vector3 at)
        {
            if (_sparkMesh == null)
            {
                var mb = new MeshBuilder();
                mb.Box(Vector3.zero, new Vector3(0.14f, 0.14f, 0.14f), ColorUtil.Gold);
                _sparkMesh = mb.ToMesh("Spark");
            }
            for (int k = 0; k < 18; k++)
            {
                var go = new GameObject("Spark");
                go.transform.SetParent(_root, false);
                go.transform.position = at + Vector3.up * 1.5f;
                go.AddComponent<MeshFilter>().sharedMesh = _sparkMesh;
                go.AddComponent<MeshRenderer>().sharedMaterial = MeshBuilder.SharedMaterial;
                var p = go.AddComponent<Puff>();
                float a = k * Mathf.PI * 2f / 18f;
                p.Velocity = new Vector3(Mathf.Cos(a) * 2.2f, 3.5f + (float)_rng.NextDouble() * 1.5f, Mathf.Sin(a) * 2.2f);
            }
        }

        private float _smokeTimer;

        /// <summary>Chimney tops (relative to the plot) for buildings that smoke.</summary>
        private static bool ChimneyOf(string id, int level, out Vector3 top)
        {
            int t = Mathf.Min(3, BuildingModels.Tier(Mathf.Max(1, level)));
            switch (id)
            {
                case "house": top = new Vector3(0.55f, 0.15f + t * 1.0f + 0.6f, 0.35f); return true;
                case "bakery": top = new Vector3(-0.6f, 0.15f + 1.0f + 0.45f * (t - 1) + 1.0f, 0.3f); return true;
                case "brickworks": top = new Vector3(0.75f, 0.12f + 1.6f + 0.6f * t + 0.15f, 0.6f); return true;
                default: top = Vector3.zero; return false;
            }
        }

        // ------------------------------------------------------------------ villagers

        private void UpdateVillagerCount()
        {
            int want = Mathf.Clamp(2 + _city.Population / 8, 2, 18);
            while (_villagers.Count < want) SpawnVillager();
            while (_villagers.Count > want)
            {
                var v = _villagers[_villagers.Count - 1];
                _villagers.RemoveAt(_villagers.Count - 1);
                if (v != null) Destroy(v.gameObject);
            }
        }

        private static readonly string[] Shirts = { "#FF6F91", "#3FB6FF", "#FFC93C", "#2ED3A0", "#8E7CFF", "#FF9F43", "#F4F4F4" };

        private void SpawnVillager()
        {
            var mb = new MeshBuilder();
            var shirt = ColorUtil.Hex(Shirts[_rng.Next(Shirts.Length)]);
            mb.Cylinder(Vector3.zero, 0.12f, 0.32f, ColorUtil.Hex("#3B3F7A"), 6);         // legs
            mb.Cylinder(new Vector3(0, 0.3f, 0), 0.17f, 0.32f, shirt, 7, 0.13f);           // body
            mb.Sphere(new Vector3(0, 0.78f, 0), 0.15f, ColorUtil.Hex("#F5C9A0"), 4, 7);   // head
            if (_rng.NextDouble() < 0.4) mb.Cylinder(new Vector3(0, 0.88f, 0), 0.17f, 0.08f, ColorUtil.Hex("#C98552"), 8); // hat
            var go = mb.ToObject("Villager", _root);
            var v = go.AddComponent<Villager>();
            int start = _rng.Next(_nodes.Count);
            v.Setup(this, start, 0.9f + (float)_rng.NextDouble() * 0.5f, _rng.Next());
            _villagers.Add(v);
        }

        internal Vector3 NodePosition(int node) => _nodes[node];

        internal int RandomNeighbour(int node, int avoid, System.Random rng)
        {
            var links = _links[node];
            if (links.Count == 0) return node;
            for (int tries = 0; tries < 4; tries++)
            {
                int n = links[rng.Next(links.Count)];
                if (n != avoid || links.Count == 1) return n;
            }
            return links[0];
        }

        // ------------------------------------------------------------------ frame

        private void Update()
        {
            if (!IsActive) return;
            float dt = Time.deltaTime;

            for (int i = 0; i < _plots.Length; i++)
            {
                var v = _plots[i];
                if (v.Blades != null) v.Blades.Rotate(0f, 0f, 70f * dt, Space.Self);
                if (v.Anim >= 0f && v.Building != null)
                {
                    v.Anim += dt;
                    float k = Mathf.Clamp01(v.Anim / 1.1f);
                    float y = JellyDrop.Util.Ease.OutBack(k);
                    v.Building.localScale = new Vector3(1f, Mathf.Max(0.05f, y), 1f);
                    if (v.Anim > 1.5f)
                    {
                        v.Anim = -1f;
                        v.Building.localScale = Vector3.one;
                        GoldBurst(v.Anchor.position);
                        if (v.Scaffold != null) Destroy(v.Scaffold);
                        UpdateIcons();
                    }
                }
            }

            _smokeTimer += dt;
            if (_smokeTimer > 0.9f)
            {
                _smokeTimer = 0f;
                for (int i = 0; i < _plots.Length; i++)
                {
                    var v = _plots[i];
                    if (v.Building == null || v.ShownLevel <= 0 || v.Anim >= 0f || !v.Anchor.gameObject.activeSelf) continue;
                    if (ChimneyOf(v.ShownId, v.ShownLevel, out var top)) Smoke(v.Anchor.position + top);
                }
            }

            _iconTimer += dt;
            if (_iconTimer > 1f)
            {
                _iconTimer = 0f;
                // Constructions that finished while watching pop up right away.
                if (_city.CheckConstruction() > 0) Sync();
                else UpdateIcons();
            }

            HandleInput();
            UpdateMission(dt);
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            // Fit the 3-plot width to the screen, then apply zoom.
            float aspect = Mathf.Max(0.3f, _cam.aspect);
            float hHalf = Mathf.Atan(Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad) * aspect);
            float fitDistance = (Cols * Step * 0.5f + 1.6f) / Mathf.Tan(hHalf);
            float distance = fitDistance * _zoom;
            var rot = Quaternion.Euler(Pitch, 0f, 0f);
            var focus = Origin + _target;
            _cam.transform.position = focus - rot * Vector3.forward * distance;
            _cam.transform.rotation = rot;
        }

        private void HandleInput()
        {
            float wheel = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
            wheel = Input.mouseScrollDelta.y;
            if (Input.touchCount >= 2)
            {
                var t0 = Input.GetTouch(0).position;
                var t1 = Input.GetTouch(1).position;
                float dist = Vector2.Distance(t0, t1);
                if (!_pinching)
                {
                    _pinching = true;
                    _pinchStart = Mathf.Max(1f, dist);
                    _zoomStart = _zoom;
                    _dragging = true; // never treat a pinch as a tap
                }
                _zoom = Mathf.Clamp(_zoomStart * _pinchStart / Mathf.Max(1f, dist), 0.45f, 1.35f);
                return;
            }
            _pinching = false;
#endif
            if (Mathf.Abs(wheel) > 0.01f) _zoom = Mathf.Clamp(_zoom - wheel * 0.08f, 0.45f, 1.35f);

            bool down = PointerInput.TryGetPointer(out var pos);
            if (down && !_pressed)
            {
                _pressed = true;
                _ignore = PointerInput.IsOverUi(pos);
                _pressPos = _lastPos = pos;
                _dragging = false;
                return;
            }
            if (!_pressed) return;
            if (down)
            {
                if (_ignore) return;
                if (!_dragging && (pos - _pressPos).magnitude > Screen.height * 0.012f) _dragging = true;
                if (_dragging)
                {
                    var delta = pos - _lastPos;
                    float unitsPerPixel = 2f * Vector3.Distance(_cam.transform.position, Origin + _target) * Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad) / Mathf.Max(1, Screen.height);
                    _target -= new Vector3(delta.x, 0f, delta.y * 1.3f) * unitsPerPixel;
                    _target.x = Mathf.Clamp(_target.x, -5f, 5f);
                    _target.z = Mathf.Clamp(_target.z, GridFront - 1f, GridBack(_rows) - 2f);
                }
                _lastPos = pos;
                return;
            }
            // Released
            _pressed = false;
            if (_ignore || _dragging) return;
            if (MissionActive)
            {
                TapTargets(_pressPos);
                return;
            }
            int plot = PlotAt(_pressPos);
            if (plot >= 0) PlotTapped?.Invoke(plot);
        }

        // ------------------------------------------------------------------ village missions

        /// <summary>Starts a mission: spawns what has to be caught / popped / found.</summary>
        public void BeginMission(JellyDrop.Core.City.CityMissionType type)
        {
            CancelMission();
            _missionType = type;
            _missionHits = 0;
            _missionNeeded = JellyDrop.Core.City.CityMissionService.TapsNeeded(type);
            MissionTimeLeft = JellyDrop.Core.City.CityMissionService.TimeLimit(type);
            MissionActive = true;
            _spawnTimer = 0f;
            _catTimer = 0f;
            switch (type)
            {
                case JellyDrop.Core.City.CityMissionType.Chicken: SpawnRunner(ChickenMesh(), 3.0f, 0.25f); break;
                case JellyDrop.Core.City.CityMissionType.Thief: SpawnRunner(ThiefMesh(), 2.6f, 0.12f); break;
                case JellyDrop.Core.City.CityMissionType.Cat: SpawnCat(); break;
                default: break; // balloons spawn over time
            }
            MissionProgress?.Invoke(_missionHits, _missionNeeded);
        }

        public void CancelMission()
        {
            foreach (var t in _targets) if (t != null) Destroy(t.gameObject);
            _targets.Clear();
            _mushroomLife.Clear();
            MissionActive = false;
        }

        private void EndMission(bool success)
        {
            foreach (var t in _targets) if (t != null) Puffs(t.position);
            CancelMission();
            MissionEnded?.Invoke(success);
        }

        private void UpdateMission(float dt)
        {
            if (!MissionActive) return;
            MissionTimeLeft -= dt;
            if (MissionTimeLeft <= 0f) { EndMission(false); return; }

            if (_missionType == JellyDrop.Core.City.CityMissionType.Mushrooms)
            {
                // Mushrooms pop up next to buildings and hide again after a moment.
                _spawnTimer -= dt;
                if (_spawnTimer <= 0f && _targets.Count < 3)
                {
                    _spawnTimer = 0.7f;
                    var go = MeshObject("Mushroom", JellyDrop.Runner3D.RunnerWorld.MushroomMesh);
                    int plot = _rng.Next(Mathf.Min(_rows * Cols, _plots.Length));
                    float sx = _rng.Next(2) == 0 ? -1f : 1f, sz = _rng.Next(2) == 0 ? -1f : 1f;
                    go.transform.localPosition = PlotCenter(plot) + new Vector3(sx * 1.8f, 0.3f, sz * 1.8f);
                    go.transform.localScale = Vector3.one * 1.6f;
                    go.AddComponent<Squash>();
                    _targets.Add(go.transform);
                    _mushroomLife[go.transform] = 2.6f;
                }
                for (int i = _targets.Count - 1; i >= 0; i--)
                {
                    var m = _targets[i];
                    if (m == null || !_mushroomLife.ContainsKey(m)) continue;
                    _mushroomLife[m] -= dt;
                    if (_mushroomLife[m] <= 0f) { _mushroomLife.Remove(m); Destroy(m.gameObject); _targets.RemoveAt(i); }
                }
            }
            else if (_missionType == JellyDrop.Core.City.CityMissionType.Balloons)
            {
                _spawnTimer -= dt;
                if (_spawnTimer <= 0f && _targets.Count < 4)
                {
                    _spawnTimer = 0.9f;
                    SpawnBalloon();
                }
                for (int i = _targets.Count - 1; i >= 0; i--)
                {
                    var b = _targets[i];
                    if (b == null) { _targets.RemoveAt(i); continue; }
                    b.localPosition += new Vector3(Mathf.Sin(Time.time * 2f + i) * 0.4f * dt, 1.1f * dt, 0f);
                    if (b.localPosition.y > 10f) { Destroy(b.gameObject); _targets.RemoveAt(i); }
                }
            }
            else if (_missionType == JellyDrop.Core.City.CityMissionType.Cat)
            {
                _catTimer += dt;
                if (_catTimer > 3.2f) MoveCat();
            }
        }

        private void TapTargets(Vector2 screen)
        {
            float radius = Screen.height * 0.065f;
            for (int i = 0; i < _targets.Count; i++)
            {
                var t = _targets[i];
                if (t == null) continue;
                var sp = _cam.WorldToScreenPoint(t.position + Vector3.up * 0.5f);
                if (sp.z <= 0f) continue;
                if ((new Vector2(sp.x, sp.y) - screen).magnitude > radius * Mathf.Clamp(20f / sp.z, 0.7f, 1.6f)) continue;
                Hit(t, i);
                return;
            }
        }

        private void Hit(Transform target, int index)
        {
            _missionHits++;
            Puffs(target.position);
            switch (_missionType)
            {
                case JellyDrop.Core.City.CityMissionType.Balloons:
                case JellyDrop.Core.City.CityMissionType.Mushrooms:
                    _mushroomLife.Remove(target);
                    Destroy(target.gameObject);
                    _targets.RemoveAt(index);
                    break;
                case JellyDrop.Core.City.CityMissionType.Cat:
                    MoveCat();
                    break;
                default:
                    // Runners get faster every time you almost catch them.
                    var w = target.GetComponent<Villager>();
                    if (w != null) w.Speed += 0.7f;
                    if (target.GetComponent<Squash>() == null) target.gameObject.AddComponent<Squash>();
                    break;
            }
            MissionProgress?.Invoke(_missionHits, _missionNeeded);
            if (_missionHits >= _missionNeeded) EndMission(true);
        }

        private void SpawnRunner(Mesh mesh, float speed, float hop)
        {
            var go = MeshObject("Runner", mesh);
            go.transform.localScale = Vector3.one * 1.4f;
            var v = go.AddComponent<Villager>();
            v.Setup(this, _rng.Next(_nodes.Count), speed, _rng.Next());
            v.NoWaits = true;
            v.Hop = hop;
            _targets.Add(go.transform);
        }

        private void SpawnBalloon()
        {
            var mb = new MeshBuilder();
            string[] colors = { "#FF5A6E", "#3FB6FF", "#FFC93C", "#2ED3A0", "#B48CFF", "#FF9F43" };
            var col = ColorUtil.Hex(colors[_rng.Next(colors.Length)]);
            mb.Sphere(new Vector3(0, 0.45f, 0), 0.42f, col, 5, 9);
            mb.Cone(new Vector3(0, -0.02f, 0), 0.1f, 0.1f, col, 6);
            mb.Box(new Vector3(0, -0.9f, 0), new Vector3(0.03f, 0.9f, 0.03f), Color.white);
            var go = mb.ToObject("Balloon", _root);
            int plot = _rng.Next(Mathf.Min(_rows * Cols, _plots.Length));
            go.transform.localPosition = PlotCenter(plot) + new Vector3((float)_rng.NextDouble() - 0.5f, 0.6f, (float)_rng.NextDouble() - 0.5f);
            go.transform.localScale = Vector3.one * 1.3f;
            _targets.Add(go.transform);
        }

        private void SpawnCat()
        {
            var go = MeshObject("Cat", CatMesh());
            go.transform.localScale = Vector3.one * 1.4f;
            _targets.Add(go.transform);
            MoveCat();
        }

        /// <summary>The cat pops up next to a random plot, peeking out from a corner.</summary>
        private void MoveCat()
        {
            _catTimer = 0f;
            if (_targets.Count == 0 || _targets[0] == null) return;
            var cat = _targets[0];
            Puffs(cat.position);
            int plot = _rng.Next(Mathf.Min(_rows * Cols, _plots.Length));
            float sx = _rng.Next(2) == 0 ? -1f : 1f, sz = _rng.Next(2) == 0 ? -1f : 1f;
            cat.localPosition = PlotCenter(plot) + new Vector3(sx * 1.75f, 0f, sz * 1.75f);
            cat.localRotation = Quaternion.Euler(0f, (float)_rng.NextDouble() * 360f, 0f);
        }

        private GameObject MeshObject(string name, Mesh mesh)
        {
            var go = new GameObject(name);
            go.transform.SetParent(_root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = MeshBuilder.SharedMaterial;
            return go;
        }

        private static Mesh ChickenMesh()
        {
            var mb = new MeshBuilder();
            mb.Box(new Vector3(-0.08f, 0, 0), new Vector3(0.04f, 0.2f, 0.04f), ColorUtil.Hex("#FF9F43"));
            mb.Box(new Vector3(0.08f, 0, 0), new Vector3(0.04f, 0.2f, 0.04f), ColorUtil.Hex("#FF9F43"));
            mb.Sphere(new Vector3(0, 0.38f, 0), 0.24f, Color.white, 5, 8);
            mb.Sphere(new Vector3(0, 0.66f, 0.14f), 0.14f, Color.white, 4, 8);
            mb.Box(new Vector3(0, 0.78f, 0.14f), new Vector3(0.05f, 0.1f, 0.14f), ColorUtil.Hex("#FF5A6E"));
            mb.Box(new Vector3(0, 0.62f, 0.3f), new Vector3(0.08f, 0.06f, 0.1f), ColorUtil.Hex("#FFC93C"));
            mb.Box(new Vector3(0, 0.36f, -0.24f), new Vector3(0.18f, 0.22f, 0.1f), ColorUtil.Hex("#EDEDF5"));
            return mb.ToMesh("Chicken");
        }

        private static Mesh ThiefMesh()
        {
            var mb = new MeshBuilder();
            mb.Cylinder(Vector3.zero, 0.12f, 0.32f, ColorUtil.Hex("#22243F"), 6);
            mb.Cylinder(new Vector3(0, 0.3f, 0), 0.17f, 0.34f, ColorUtil.Hex("#3B3F7A"), 7, 0.13f);
            mb.Sphere(new Vector3(0, 0.8f, 0), 0.15f, ColorUtil.Hex("#F5C9A0"), 4, 7);
            mb.Box(new Vector3(0, 0.79f, 0.02f), new Vector3(0.32f, 0.07f, 0.3f), ColorUtil.Hex("#111222"));   // mask
            mb.Cylinder(new Vector3(0, 0.9f, 0), 0.16f, 0.1f, ColorUtil.Hex("#22243F"), 8);                    // beanie
            mb.Sphere(new Vector3(0, 0.55f, -0.22f), 0.2f, ColorUtil.Hex("#C98552"), 4, 7);                  // sack of coins
            mb.Sphere(new Vector3(0, 0.75f, -0.22f), 0.06f, ColorUtil.Hex("#FFC93C"), 3, 6);
            return mb.ToMesh("Thief");
        }

        private static Mesh CatMesh()
        {
            var mb = new MeshBuilder();
            var fur = ColorUtil.Hex("#FF9F43");
            mb.Box(new Vector3(0, 0.08f, 0), new Vector3(0.3f, 0.3f, 0.5f), fur);
            mb.Sphere(new Vector3(0, 0.5f, 0.24f), 0.17f, fur, 4, 8);
            mb.Cone(new Vector3(-0.09f, 0.6f, 0.24f), 0.06f, 0.14f, fur, 4);
            mb.Cone(new Vector3(0.09f, 0.6f, 0.24f), 0.06f, 0.14f, fur, 4);
            mb.Box(new Vector3(0, 0.3f, -0.3f), new Vector3(0.06f, 0.45f, 0.06f), ColorUtil.Hex("#E07A2E"));
            mb.Box(new Vector3(-0.06f, 0.52f, 0.4f), new Vector3(0.04f, 0.05f, 0.02f), ColorUtil.Hex("#2A1440"));
            mb.Box(new Vector3(0.06f, 0.52f, 0.4f), new Vector3(0.04f, 0.05f, 0.02f), ColorUtil.Hex("#2A1440"));
            return mb.ToMesh("Cat");
        }

        /// <summary>Which plot is under a screen position (-1 for none).</summary>
        public int PlotAt(Vector2 screen)
        {
            var ray = _cam.ScreenPointToRay(new Vector3(screen.x, screen.y, 0f));
            if (Mathf.Abs(ray.direction.y) < 0.0001f) return -1;
            float t = (Origin.y - ray.origin.y) / ray.direction.y;
            if (t < 0f) return -1;
            var p = ray.origin + ray.direction * t - Origin;
            int c = Mathf.RoundToInt(p.x / Step + (Cols - 1) / 2f);
            int r = Mathf.RoundToInt(p.z / Step + 1.5f);
            if (c < 0 || c >= Cols || r < 0 || r >= _rows) return -1;
            var center = PlotCenter(r * Cols + c);
            if (Mathf.Abs(p.x - center.x) > PlotSize * 0.5f + 0.3f || Mathf.Abs(p.z - center.z) > PlotSize * 0.5f + 0.3f) return -1;
            return r * Cols + c;
        }

        /// <summary>Screen position of a plot (for floating texts over it).</summary>
        public Vector3 PlotScreenPosition(int index) => _cam.WorldToScreenPoint(Origin + PlotCenter(index) + Vector3.up * 2.5f);

        public void PopPlot(int index)
        {
            var v = _plots[index];
            if (v.Building == null) return;
            v.Content.AddComponent<Squash>();
        }
    }

    // ------------------------------------------------------------------ small behaviours

    /// <summary>Gentle hovering; optionally spins or faces the camera.</summary>
    public sealed class Hover : MonoBehaviour
    {
        public bool Spin;
        public bool Billboard;
        private Vector3 _base;
        private float _phase;

        private void Start()
        {
            _base = transform.localPosition;
            _phase = transform.position.x * 0.7f + transform.position.z * 0.3f;
        }

        private void Update()
        {
            transform.localPosition = _base + Vector3.up * Mathf.Sin(Time.time * 2.4f + _phase) * 0.12f;
            if (Spin) transform.Rotate(0f, 90f * Time.deltaTime, 0f, Space.World);
            if (Billboard && Camera.allCamerasCount > 0)
            {
                var cam = FindCityCamera();
                if (cam != null) transform.rotation = cam.transform.rotation;
            }
        }

        private static Camera _cityCam;

        private static Camera FindCityCamera()
        {
            if (_cityCam != null && _cityCam.enabled) return _cityCam;
            foreach (var c in Camera.allCameras) if (c.name == "CityCamera") _cityCam = c;
            return _cityCam;
        }
    }

    /// <summary>A villager walking from crossing to crossing along the paths.</summary>
    public sealed class Villager : MonoBehaviour
    {
        private CityWorld _world;
        private int _from, _to;
        private float _t, _speed, _wait;
        private System.Random _rng;
        /// <summary>Runners (the chicken, the thief) never stop to chat.</summary>
        public bool NoWaits;
        public float Hop = 0.06f;
        public float Speed { get => _speed; set => _speed = value; }

        public void Setup(CityWorld world, int startNode, float speed, int seed)
        {
            _world = world;
            _rng = new System.Random(seed);
            _from = startNode;
            _to = world.RandomNeighbour(startNode, -1, _rng);
            _speed = speed;
            _t = (float)_rng.NextDouble();
            // Walk on the right-hand side of the path so people don't overlap too much.
            _offset = ((float)_rng.NextDouble() - 0.5f) * 0.6f;
        }

        private float _offset;

        private void Update()
        {
            if (_world == null) return;
            if (_wait > 0f) { _wait -= Time.deltaTime; return; }
            var a = _world.NodePosition(_from);
            var b = _world.NodePosition(_to);
            float len = Mathf.Max(0.01f, Vector3.Distance(a, b));
            _t += Time.deltaTime * _speed / len;
            if (_t >= 1f)
            {
                int next = _world.RandomNeighbour(_to, _from, _rng);
                _from = _to;
                _to = next;
                _t = 0f;
                if (!NoWaits && _rng.NextDouble() < 0.2) _wait = 0.5f + (float)_rng.NextDouble() * 1.5f;
                return;
            }
            var dir = (b - a) / len;
            var side = new Vector3(dir.z, 0f, -dir.x) * _offset;
            float bob = Mathf.Abs(Mathf.Sin(Time.time * 9f * Mathf.Min(_speed, 2f))) * Hop;
            transform.localPosition = Vector3.Lerp(a, b, _t) + side + Vector3.up * bob;
            transform.localRotation = Quaternion.LookRotation(dir, Vector3.up);
        }
    }

    /// <summary>Construction dust: drifts up, shrinks and disappears.</summary>
    public sealed class Puff : MonoBehaviour
    {
        public Vector3 Velocity;
        private float _age;

        private void Update()
        {
            _age += Time.deltaTime;
            transform.position += Velocity * Time.deltaTime;
            Velocity *= 1f - Time.deltaTime * 1.5f;
            transform.localScale = Vector3.one * Mathf.Max(0f, 1f - _age / 1.2f);
            transform.Rotate(60f * Time.deltaTime, 90f * Time.deltaTime, 0f);
            if (_age > 1.2f) Destroy(gameObject);
        }
    }

    /// <summary>Quick squash-and-stretch when a building is tapped or collected.</summary>
    public sealed class Squash : MonoBehaviour
    {
        private float _t;
        private Vector3 _base;
        private bool _init;

        private void Update()
        {
            if (!_init) { _base = transform.localScale; _init = true; }
            _t += Time.deltaTime;
            float k = Mathf.Clamp01(_t / 0.35f);
            float s = 1f + Mathf.Sin(k * Mathf.PI) * 0.12f;
            transform.localScale = Vector3.Scale(_base, new Vector3(1f / Mathf.Sqrt(s), s, 1f / Mathf.Sqrt(s)));
            if (k >= 1f)
            {
                transform.localScale = _base;
                Destroy(this);
            }
        }
    }
}
