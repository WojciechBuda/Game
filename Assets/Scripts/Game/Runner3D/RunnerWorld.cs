using System;
using System.Collections.Generic;
using JellyDrop.Audio;
using JellyDrop.City3D;
using JellyDrop.Core.Runner;
using JellyDrop.GameInput;
using JellyDrop.Services;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Runner3D
{
    /// <summary>
    /// The 3D view of Block Run. The avatar block runs down a three-lane street past little houses and trees;
    /// barriers, overhead gates and crates come at it and coins float along the lanes. Swipes (or arrow keys)
    /// drive the <see cref="RunnerSession"/>; this class only draws and animates it.
    /// Lives at x = 5000 with its own camera and light, active only while a run is on.
    /// </summary>
    public sealed class RunnerWorld : MonoBehaviour
    {
        public const float LaneWidth = 1.9f;
        private const float ChunkLength = 24f;
        private const int ChunkCount = 7;
        private static readonly Vector3 Origin = new Vector3(5000f, 0f, 0f);

        public RunnerSession Session { get; private set; }
        public bool IsActive { get; private set; }

        /// <summary>Raised once when the run ends (after the crash animation starts).</summary>
        public event Action RunEnded;
        public event Action CoinPicked;

        private AudioManager _audio;
        private Camera _mainCamera;
        private Camera _cam;
        private GameObject _light;
        private Transform _root;
        private Transform _player;
        private SpriteRenderer _avatar;
        private Transform _shadow;
        private readonly List<Transform> _chunks = new List<Transform>();
        private readonly Dictionary<RunRow, GameObject> _rowObjects = new Dictionary<RunRow, GameObject>();
        private readonly Dictionary<RunCoin, Transform> _coinObjects = new Dictionary<RunCoin, Transform>();
        private Mesh _coinMesh, _lowMesh, _highMesh, _crateMesh, _shadowMesh;
        private float _laneX;
        private float _crashTime = -1f;
        private bool _paused;

        // Swipe input
        private bool _pressed, _ignore, _swiped;
        private Vector2 _pressPos;

        // ------------------------------------------------------------------ setup

        public void Init(AudioManager audio, Camera mainCamera)
        {
            _audio = audio;
            _mainCamera = mainCamera;
            _root = new GameObject("Runner3D").transform;
            _root.SetParent(transform, false);
            _root.position = Origin;

            var camGo = new GameObject("RunnerCamera");
            camGo.transform.SetParent(transform, false);
            _cam = camGo.AddComponent<Camera>();
            _cam.clearFlags = CameraClearFlags.SolidColor;
            _cam.backgroundColor = ColorUtil.Hex("#161838");
            _cam.fieldOfView = 62f;
            _cam.nearClipPlane = 0.2f;
            _cam.farClipPlane = 140f;
            _cam.depth = 6;
            _cam.enabled = false;

            _light = new GameObject("RunnerSun");
            _light.transform.SetParent(_root, false);
            _light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var l = _light.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = ColorUtil.Hex("#C9D2FF");
            l.intensity = 0.95f;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.5f;
            _light.SetActive(false);

            BuildMeshes();
            for (int i = 0; i < ChunkCount; i++) _chunks.Add(BuildChunk(i));

            _player = new GameObject("Player").transform;
            _player.SetParent(_root, false);
            var av = new GameObject("Avatar");
            av.transform.SetParent(_player, false);
            av.transform.localScale = Vector3.one * 1.45f; // avatar sprites are 1 unit tall
            _avatar = av.AddComponent<SpriteRenderer>();
            _avatar.sortingOrder = 20;
            var sh = new GameObject("Shadow");
            sh.transform.SetParent(_root, false);
            sh.AddComponent<MeshFilter>().sharedMesh = _shadowMesh;
            sh.AddComponent<MeshRenderer>().sharedMaterial = MeshBuilder.SharedMaterial;
            _shadow = sh.transform;

            _root.gameObject.SetActive(false);
        }

        private void BuildMeshes()
        {
            var mb = new MeshBuilder { Graded = false };
            // Coin: a gold disc standing up (axis along z), with a darker rim
            mb.Transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(90f, 0f, 0f), Vector3.one);
            mb.Cylinder(new Vector3(0, -0.06f, 0), 0.34f, 0.12f, ColorUtil.Hex("#FFC93C"), 14);
            mb.Cylinder(new Vector3(0, -0.07f, 0), 0.22f, 0.14f, ColorUtil.Hex("#FFE27A"), 10);
            _coinMesh = mb.ToMesh("Coin");

            mb.Clear(); // Low barrier: striped bar on two legs (jump over)
            foreach (var x in new[] { -0.75f, 0.75f }) mb.Box(new Vector3(x, 0, 0), new Vector3(0.12f, 0.75f, 0.12f), ColorUtil.Hex("#8D91A6"));
            for (int i = 0; i < 6; i++)
                mb.Box(new Vector3(-0.75f + 0.25f + i * 0.25f - 0.125f, 0.45f, 0), new Vector3(0.25f, 0.26f, 0.16f), i % 2 == 0 ? ColorUtil.Hex("#FF4FA3") : ColorUtil.Hex("#F2F0FF"));
            _lowMesh = mb.ToMesh("Low");

            mb.Clear(); // High gate: tall posts with a warning bar overhead (slide under)
            foreach (var x in new[] { -0.82f, 0.82f }) mb.Box(new Vector3(x, 0, 0), new Vector3(0.14f, 2.3f, 0.14f), ColorUtil.Hex("#5B5F80"));
            for (int i = 0; i < 6; i++)
                mb.Box(new Vector3(-0.82f + 0.27f + i * 0.27f - 0.135f, 1.25f, 0), new Vector3(0.27f, 0.42f, 0.2f), i % 2 == 0 ? ColorUtil.Hex("#3DE0FF") : ColorUtil.Hex("#1F2240"));
            _highMesh = mb.ToMesh("High");

            mb.Clear(); // Crate: a big wooden box blocking the lane
            float len = RunnerSession.BlockLength;
            mb.Box(new Vector3(0, 0, len / 2f), new Vector3(1.55f, 1.7f, len), ColorUtil.Hex("#4A3A7A"), ColorUtil.Hex("#5E4C96"));
            for (int i = 0; i < 3; i++)
                mb.Box(new Vector3(0, 0.25f + i * 0.55f, -0.02f), new Vector3(1.58f, 0.08f, 0.06f), ColorUtil.Hex("#FF4FA3"));
            mb.Box(new Vector3(0, 1.7f, len / 2f), new Vector3(0.5f, 0.35f, 0.5f), ColorUtil.Hex("#FF6F91"));
            _crateMesh = mb.ToMesh("Crate");

            mb.Clear();
            mb.Disc(Vector3.zero, 0.55f, new Color(0.15f, 0.15f, 0.3f), 16);
            _shadowMesh = mb.ToMesh("Shadow");
        }

        /// <summary>One street segment: three lanes, pavements, grass and a few houses and trees.</summary>
        private Transform BuildChunk(int index)
        {
            var rng = new System.Random(index * 97 + 13);
            var mb = new MeshBuilder { Graded = false };
            float half = ChunkLength / 2f;
            mb.Box(new Vector3(0, -0.2f, half), new Vector3(LaneWidth * 3f + 0.4f, 0.2f, ChunkLength), ColorUtil.Hex("#1F2240"), ColorUtil.Hex("#272B4F"));
            for (int k = 0; k < 2; k++)
            {
                float x = (k - 0.5f) * LaneWidth;
                for (float z = 1f; z < ChunkLength; z += 3f) mb.Box(new Vector3(x, 0f, z), new Vector3(0.1f, 0.01f, 1.4f), ColorUtil.Hex("#3DE0FF"));
            }
            foreach (var side in new[] { -1f, 1f })
            {
                mb.Box(new Vector3(side * (LaneWidth * 1.5f + 0.9f), -0.2f, half), new Vector3(1.4f, 0.32f, ChunkLength), ColorUtil.Hex("#3A3F6E"));
                // Neon edge along the track
                mb.Box(new Vector3(side * (LaneWidth * 1.5f + 0.22f), 0.12f, half), new Vector3(0.06f, 0.06f, ChunkLength), side > 0 ? ColorUtil.Hex("#FF4FA3") : ColorUtil.Hex("#3DE0FF"));
                mb.Box(new Vector3(side * (LaneWidth * 1.5f + 6f), -0.25f, half), new Vector3(8.8f, 0.25f, ChunkLength), ColorUtil.Hex("#173B3F"));
                // Street lamps with glowing heads
                for (float lz = 4f; lz < ChunkLength; lz += 12f)
                {
                    var lp = new Vector3(side * (LaneWidth * 1.5f + 1.2f), 0f, lz);
                    mb.Box(lp, new Vector3(0.1f, 2.6f, 0.1f), ColorUtil.Hex("#2B2F5E"));
                    mb.Sphere(lp + new Vector3(-side * 0.25f, 2.65f, 0f), 0.2f, side > 0 ? ColorUtil.Hex("#FFB3E0") : ColorUtil.Hex("#9FF3FF"), 4, 8);
                }
                for (float z = 3f; z < ChunkLength; z += 6f + (float)rng.NextDouble() * 3f)
                {
                    var at = new Vector3(side * (LaneWidth * 1.5f + 3.6f + (float)rng.NextDouble() * 2f), 0f, z);
                    double r = rng.NextDouble();
                    if (r < 0.45)
                    {
                        // A house turned to face the street
                        var house = new MeshBuilder { Graded = false };
                        string[] ids = { "house", "bakery", "house", "tower" };
                        BuildingModels.Build(house, ids[rng.Next(ids.Length)], 1 + rng.Next(3));
                        var v = house.Vertices; var c = house.Colors; var t = house.Triangles;
                        float angle = side > 0 ? -90f : 90f;
                        var rot = Quaternion.Euler(0f, angle, 0f);
                        for (int i = 0; i < t.Count; i += 3)
                            mb.Triangle(at + rot * v[t[i]], at + rot * v[t[i + 1]], at + rot * v[t[i + 2]], c[t[i]]);
                        z += 2f;
                    }
                    else if (r < 0.8) BuildingModels.Tree(mb, at, 0.9f + (float)rng.NextDouble() * 0.5f);
                    else BuildingModels.RoundTree(mb, at, 0.9f + (float)rng.NextDouble() * 0.4f);
                }
            }
            var go = mb.ToObject("Chunk" + index, _root);
            go.transform.localPosition = new Vector3(0, 0, index * ChunkLength - ChunkLength);
            return go.transform;
        }

        // ------------------------------------------------------------------ run lifecycle

        public void Begin(int seed, Sprite avatar, int lives = RunnerSession.StartLives)
        {
            End();
            Session = new RunnerSession(seed, lives);
            Session.RowSpawned += SpawnRow;
            Session.CoinSpawned += SpawnCoin;
            Session.CoinCollected += OnCoin;
            Session.Crashed += OnCrash;
            Session.Hit += OnHit;
            _avatar.sprite = avatar;
            _laneX = 0f;
            _crashTime = -1f;
            _paused = false;
            for (int i = 0; i < _chunks.Count; i++) _chunks[i].localPosition = new Vector3(0, 0, i * ChunkLength - ChunkLength);

            IsActive = true;
            _root.gameObject.SetActive(true);
            _light.SetActive(true);
            _cam.enabled = true;
            if (_mainCamera != null) _mainCamera.enabled = false;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = _cam.backgroundColor;
            RenderSettings.fogStartDistance = 40f;
            RenderSettings.fogEndDistance = 95f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ColorUtil.Hex("#4A5290");
            QualitySettings.shadowDistance = 45f;

            Session.Start();
            UpdateView(0f);
        }

        /// <summary>Stops the run view and clears the track (call when leaving the screen).</summary>
        public void End()
        {
            foreach (var kv in _rowObjects) if (kv.Value != null) Destroy(kv.Value);
            _rowObjects.Clear();
            foreach (var kv in _coinObjects) if (kv.Value != null) Destroy(kv.Value.gameObject);
            _coinObjects.Clear();
            if (!IsActive) return;
            IsActive = false;
            _cam.enabled = false;
            _light.SetActive(false);
            _root.gameObject.SetActive(false);
            RenderSettings.fog = false;
            if (_mainCamera != null) _mainCamera.enabled = true;
            Session = null;
        }

        public void SetPaused(bool paused) => _paused = paused;

        private void SpawnRow(RunRow row)
        {
            var go = new GameObject("Row");
            go.transform.SetParent(_root, false);
            go.transform.localPosition = new Vector3(0, 0, row.Z);
            for (int lane = 0; lane < RunnerSession.LaneCount; lane++)
            {
                var o = row.Lanes[lane];
                if (o == RunObstacle.None) continue;
                var part = new GameObject(o.ToString());
                part.transform.SetParent(go.transform, false);
                part.transform.localPosition = new Vector3(LaneX(lane), 0, 0);
                part.AddComponent<MeshFilter>().sharedMesh = o == RunObstacle.Low ? _lowMesh : o == RunObstacle.High ? _highMesh : _crateMesh;
                part.AddComponent<MeshRenderer>().sharedMaterial = MeshBuilder.SharedMaterial;
            }
            _rowObjects[row] = go;
        }

        private void SpawnCoin(RunCoin coin)
        {
            var go = new GameObject("Coin");
            go.transform.SetParent(_root, false);
            go.transform.localPosition = new Vector3(LaneX(coin.Lane), coin.High ? 2.0f : 0.7f, coin.Z);
            go.AddComponent<MeshFilter>().sharedMesh = coin.Mushroom ? MushroomMesh : _coinMesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = MeshBuilder.SharedMaterial;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _coinObjects[coin] = go.transform;
        }

        private static Mesh _mushroomMesh;
        /// <summary>A little red-capped mushroom (also used on the islands).</summary>
        public static Mesh MushroomMesh
        {
            get
            {
                if (_mushroomMesh != null) return _mushroomMesh;
                var mb = new MeshBuilder { Graded = false };
                mb.Cylinder(new Vector3(0, -0.3f, 0), 0.14f, 0.32f, ColorUtil.Hex("#F2E6D0"), 8);
                mb.Sphere(new Vector3(0, 0.06f, 0), 0.34f, ColorUtil.Hex("#E0483E"), 4, 10);
                mb.Disc(new Vector3(0, 0.0f, 0), 0.34f, ColorUtil.Hex("#A82A2A"), 12);
                foreach (var p in new[] { new Vector3(0.15f, 0.3f, 0.12f), new Vector3(-0.14f, 0.28f, 0.16f), new Vector3(0.02f, 0.38f, -0.15f), new Vector3(-0.2f, 0.18f, -0.1f) })
                    mb.Sphere(p, 0.06f, Color.white, 2, 5);
                _mushroomMesh = mb.ToMesh("Mushroom");
                return _mushroomMesh;
            }
        }

        private void OnCoin(RunCoin coin)
        {
            _audio.PlayPitched(Sfx.Coin, 1f + Mathf.Repeat(Session.Coins * 0.05f, 0.5f), 0.7f);
            if (_coinObjects.TryGetValue(coin, out var t) && t != null)
            {
                t.gameObject.AddComponent<CoinPop>();
                _coinObjects.Remove(coin);
            }
            CoinPicked?.Invoke();
        }

        private void OnHit(int livesLeft)
        {
            _audio.Play(Sfx.Bomb);
            Haptics.Medium();
            LivesChanged?.Invoke(livesLeft);
        }

        /// <summary>Raised when a life is lost (lives left).</summary>
        public event System.Action<int> LivesChanged;

        private void OnCrash()
        {
            _crashTime = 0f;
            _audio.Play(Sfx.GameOver);
            Haptics.Big();
        }

        private static float LaneX(int lane) => (lane - 1) * LaneWidth;

        // ------------------------------------------------------------------ frame

        private void Update()
        {
            if (!IsActive || Session == null) return;
            float dt = Time.deltaTime;
            if (!_paused)
            {
                if (!Session.IsOver)
                {
                    HandleInput();
                    Session.Tick(dt);
                }
                else if (_crashTime >= 0f)
                {
                    _crashTime += dt;
                    if (_crashTime > 0.9f)
                    {
                        _crashTime = -1f;
                        RunEnded?.Invoke();
                    }
                }
            }
            UpdateView(dt);
        }

        private void UpdateView(float dt)
        {
            var s = Session;
            float z = s.Distance;
            _laneX = Mathf.Lerp(_laneX, LaneX(s.Lane), 1f - Mathf.Exp(-dt * 16f));

            // Player: hop while running, squash on slide, tumble on crash.
            float run = s.IsJumping || s.IsSliding || s.IsOver ? 0f : Mathf.Abs(Mathf.Sin(s.Time * 11f)) * 0.12f;
            float y = s.Height + run;
            _player.localPosition = new Vector3(_laneX, 0.75f + y, z);
            float squash = s.IsSliding ? 0.5f : s.IsJumping ? 1.08f : 1f - run * 0.4f;
            _player.localScale = new Vector3(1f / Mathf.Sqrt(squash), squash, 1f);
            if (s.IsOver) _player.localRotation = Quaternion.Euler(0f, 0f, Mathf.Min(1f, Math.Max(0f, _crashTime)) * 75f);
            else _player.localRotation = Quaternion.Euler(0f, 0f, (_laneX - LaneX(s.Lane)) * -12f);
            _avatar.transform.rotation = _cam.transform.rotation;
            // Blink while invincible after losing a life.
            _avatar.enabled = s.Invincible <= 0f || Mathf.Repeat(s.Invincible * 10f, 1f) < 0.55f;
            _shadow.localPosition = new Vector3(_laneX, 0.02f, z);
            _shadow.localScale = Vector3.one * (1f - Mathf.Min(0.5f, s.Height * 0.25f));

            // Chase camera
            var target = Origin + new Vector3(_laneX * 0.6f, 0f, z);
            _cam.transform.position = target + new Vector3(0f, 3.6f, -6.4f);
            _cam.transform.rotation = Quaternion.LookRotation((target + new Vector3(_laneX * 0.1f, 0.9f, 7f)) - _cam.transform.position, Vector3.up);

            // Recycle street chunks that fell behind
            foreach (var c in _chunks)
                if (c.localPosition.z + ChunkLength < z - 10f)
                    c.localPosition += new Vector3(0, 0, ChunkLength * ChunkCount);

            // Spin coins, drop objects that are behind the camera
            float spin = Time.time * 200f;
            var stale = new List<RunCoin>();
            foreach (var kv in _coinObjects)
            {
                if (kv.Value == null) { stale.Add(kv.Key); continue; }
                if (kv.Key.Z < z - 8f) { Destroy(kv.Value.gameObject); stale.Add(kv.Key); continue; }
                kv.Value.localRotation = Quaternion.Euler(0f, spin, 0f);
            }
            foreach (var c in stale) _coinObjects.Remove(c);
            var oldRows = new List<RunRow>();
            foreach (var kv in _rowObjects) if (kv.Key.Z + RunnerSession.BlockLength < z - 8f) oldRows.Add(kv.Key);
            foreach (var r in oldRows) { Destroy(_rowObjects[r]); _rowObjects.Remove(r); }
        }

        private void HandleInput()
        {
            if (PointerInput.KeyDown(GameKey.Left)) Session.Left();
            if (PointerInput.KeyDown(GameKey.Right)) Session.Right();
            if (PointerInput.KeyDown(GameKey.RotateCW) || PointerInput.KeyDown(GameKey.HardDrop)) Session.Jump();
            if (PointerInput.KeyDown(GameKey.Down)) Session.Slide();

            bool down = PointerInput.TryGetPointer(out var pos);
            if (down && !_pressed)
            {
                _pressed = true;
                _swiped = false;
                _ignore = PointerInput.IsOverUi(pos);
                _pressPos = pos;
                return;
            }
            if (!_pressed) return;
            if (!down) { _pressed = false; return; }
            if (_ignore || _swiped) return;
            // React as soon as the finger has moved far enough (feels instant).
            var d = pos - _pressPos;
            float threshold = Screen.height * 0.035f;
            if (d.magnitude < threshold) return;
            _swiped = true;
            if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) { if (d.x > 0) Session.Right(); else Session.Left(); }
            else if (d.y > 0) Session.Jump();
            else Session.Slide();
        }
    }

    /// <summary>Collected coin: zooms up, spins and vanishes.</summary>
    public sealed class CoinPop : MonoBehaviour
    {
        private float _t;

        private void Update()
        {
            _t += Time.deltaTime;
            transform.localPosition += Vector3.up * Time.deltaTime * 4f;
            transform.localScale = Vector3.one * Mathf.Max(0f, 1f - _t * 3f);
            if (_t > 0.35f) Destroy(gameObject);
        }
    }
}
