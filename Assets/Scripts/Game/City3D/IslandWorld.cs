using System;
using System.Collections.Generic;
using JellyDrop.GameInput;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>
    /// Base for the extra islands next to the village (forest, garden): a low-poly island with its own camera,
    /// drag to pan, pinch / wheel to zoom, and tappable things (a mushroom, a tree, a bed, a building...).
    /// Each island lives far away from everything else and is only shown on its own screen.
    /// </summary>
    public abstract class IslandWorld : MonoBehaviour
    {
        private const float Pitch = 50f, Fov = 40f;

        public bool IsActive { get; private set; }

        protected Transform Root;
        protected Camera Cam;
        protected Vector3 Origin;
        protected readonly System.Random Rng = new System.Random(11);

        /// <summary>Pan limits (local x half-width and z range of the focus point).</summary>
        protected float PanHalfWidth = 5f, PanZMin = -8f, PanZMax = 8f;
        /// <summary>Width that fits on screen at zoom 1.</summary>
        protected float ViewWidth = 15f;
        protected Vector3 StartFocus = Vector3.zero;

        private Camera _main;
        private GameObject _sun;
        private Color _sky;
        private Vector3 _target;
        private float _zoom = 1f;
        private bool _pressed, _ignore, _dragging;
        private Vector2 _pressPos, _lastPos;
#if ENABLE_LEGACY_INPUT_MANAGER
        private bool _pinching;
        private float _pinchStart, _zoomStart;
#endif
        private float _syncTimer;

        private sealed class Tappable
        {
            public Transform T;
            public float Radius;
            public Action OnTap;
            public Func<bool> Enabled;
        }

        private readonly List<Tappable> _tappables = new List<Tappable>();

        public void Init(Camera mainCamera, Vector3 origin, Color sky)
        {
            _main = mainCamera;
            Origin = origin;
            _sky = sky;
            Root = new GameObject(GetType().Name + "Root").transform;
            Root.SetParent(transform, false);
            Root.position = origin;

            var camGo = new GameObject(GetType().Name + "Camera");
            camGo.transform.SetParent(transform, false);
            Cam = camGo.AddComponent<Camera>();
            Cam.clearFlags = CameraClearFlags.SolidColor;
            Cam.backgroundColor = sky;
            Cam.fieldOfView = Fov;
            Cam.nearClipPlane = 0.3f;
            Cam.farClipPlane = 250f;
            Cam.depth = 5;
            Cam.enabled = false;

            _sun = new GameObject("Sun");
            _sun.transform.SetParent(Root, false);
            _sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            var l = _sun.AddComponent<Light>();
            l.type = LightType.Directional;
            l.color = ColorUtil.Hex("#FFE9C8");
            l.intensity = 1.1f;
            l.shadows = LightShadows.Soft;
            l.shadowStrength = 0.5f;

            BuildWorld();
            _sun.SetActive(false);
            Root.gameObject.SetActive(false);
        }

        protected abstract void BuildWorld();

        /// <summary>Called every frame while shown.</summary>
        protected virtual void Tick(float dt) { }

        /// <summary>Called about once a second while shown (refresh from the game state).</summary>
        public virtual void Sync() { }

        public void Show()
        {
            if (IsActive) return;
            IsActive = true;
            Root.gameObject.SetActive(true);
            _sun.SetActive(true);
            Cam.enabled = true;
            if (_main != null) _main.enabled = false;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = ColorUtil.Hex("#8C93C8");
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = _sky;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 110f;
            QualitySettings.shadowDistance = 70f;
            _target = StartFocus;
            _zoom = 1f;
            Sync();
            UpdateCamera();
        }

        public void Hide()
        {
            if (!IsActive) return;
            IsActive = false;
            OnHidden();
            Cam.enabled = false;
            _sun.SetActive(false);
            Root.gameObject.SetActive(false);
            RenderSettings.fog = false;
            if (_main != null) _main.enabled = true;
            _pressed = false;
        }

        protected virtual void OnHidden() { }

        // ------------------------------------------------------------------ helpers for subclasses

        protected void AddTappable(Transform t, float radius, Action onTap, Func<bool> enabled = null) =>
            _tappables.Add(new Tappable { T = t, Radius = radius, OnTap = onTap, Enabled = enabled });

        protected void RemoveTappable(Transform t) => _tappables.RemoveAll(x => x.T == t);

        protected GameObject MeshObject(string name, Mesh mesh, Transform parent = null, bool shadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent ?? Root, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = MeshBuilder.SharedMaterial;
            if (!shadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            return go;
        }

        /// <summary>A rounded island (stadium along z) with a beach, on blue water.</summary>
        protected static void Island(MeshBuilder mb, float halfW, float zFront, float zBack, Color grass, Color beach, Color water)
        {
            float zc = (zFront + zBack) / 2f, len = Mathf.Max(0.1f, (zBack - zFront) - 2f * halfW);
            mb.Disc(new Vector3(0, -0.45f, zc), 140f, water, 48);
            Stadium(mb, zc, len, halfW + 1.3f, -0.6f, 0.45f, beach);
            Stadium(mb, zc, len, halfW, -0.6f, 0.6f, grass);
        }

        protected static void Stadium(MeshBuilder mb, float zc, float len, float r, float y, float h, Color col)
        {
            mb.Box(new Vector3(0, y, zc), new Vector3(r * 2f, h, len), col, col);
            mb.Cylinder(new Vector3(0, y, zc - len / 2f), r, h, col, 28);
            mb.Cylinder(new Vector3(0, y, zc + len / 2f), r, h, col, 28);
        }

        /// <summary>A wooden bridge leaving the island to the left (towards the village).</summary>
        protected static void Bridge(MeshBuilder mb, Vector3 start, float length)
        {
            var wood = ColorUtil.Hex("#A86F43");
            var dark = ColorUtil.Hex("#6E4826");
            for (float x = 0; x < length; x += 0.5f)
                mb.Box(start + new Vector3(-x, -0.05f, 0), new Vector3(0.42f, 0.1f, 1.6f), x % 1f < 0.5f ? wood : PixelCanvas.Lighten(wood, 0.1f));
            for (float x = 0; x <= length; x += 1.5f)
                foreach (var z in new[] { -0.8f, 0.8f })
                {
                    mb.Box(start + new Vector3(-x, -0.6f, z), new Vector3(0.12f, 1.3f, 0.12f), dark);
                }
            foreach (var z in new[] { -0.8f, 0.8f })
                mb.Box(start + new Vector3(-length / 2f, 0.55f, z), new Vector3(length, 0.06f, 0.06f), dark);
        }

        /// <summary>Screen position of a world point on this island (for floating texts).</summary>
        public Vector3 ScreenOf(Transform t) => Cam.WorldToScreenPoint(t.position + Vector3.up * 1.2f);

        // ------------------------------------------------------------------ frame

        private void Update()
        {
            if (!IsActive) return;
            float dt = Time.deltaTime;
            Tick(dt);
            _syncTimer += dt;
            if (_syncTimer > 1f) { _syncTimer = 0f; Sync(); }
            HandleInput();
            UpdateCamera();
        }

        private void UpdateCamera()
        {
            float aspect = Mathf.Max(0.3f, Cam.aspect);
            float hHalf = Mathf.Atan(Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad) * aspect);
            float distance = (ViewWidth * 0.5f) / Mathf.Tan(hHalf) * _zoom;
            var rot = Quaternion.Euler(Pitch, 0f, 0f);
            Cam.transform.position = Origin + _target - rot * Vector3.forward * distance;
            Cam.transform.rotation = rot;
        }

        private void HandleInput()
        {
            float wheel = 0f;
#if ENABLE_LEGACY_INPUT_MANAGER
            wheel = Input.mouseScrollDelta.y;
            if (Input.touchCount >= 2)
            {
                float dist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
                if (!_pinching)
                {
                    _pinching = true;
                    _pinchStart = Mathf.Max(1f, dist);
                    _zoomStart = _zoom;
                    _dragging = true;
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
                    float unitsPerPixel = 2f * Vector3.Distance(Cam.transform.position, Origin + _target) * Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad) / Mathf.Max(1, Screen.height);
                    _target -= new Vector3(delta.x, 0f, delta.y * 1.3f) * unitsPerPixel;
                    _target.x = Mathf.Clamp(_target.x, -PanHalfWidth, PanHalfWidth);
                    _target.z = Mathf.Clamp(_target.z, PanZMin, PanZMax);
                }
                _lastPos = pos;
                return;
            }
            _pressed = false;
            if (_ignore || _dragging) return;
            TapAt(_pressPos);
        }

        private void TapAt(Vector2 screen)
        {
            Tappable best = null;
            float bestD = float.MaxValue;
            foreach (var t in _tappables)
            {
                if (t.T == null || !t.T.gameObject.activeInHierarchy) continue;
                if (t.Enabled != null && !t.Enabled()) continue;
                var sp = Cam.WorldToScreenPoint(t.T.position + Vector3.up * 0.4f);
                if (sp.z <= 0f) continue;
                // Radius in world units -> pixels at that depth.
                float pxPerUnit = Screen.height / (2f * sp.z * Mathf.Tan(Fov * 0.5f * Mathf.Deg2Rad));
                float r = Mathf.Max(Screen.height * 0.035f, t.Radius * pxPerUnit);
                float d = (new Vector2(sp.x, sp.y) - screen).magnitude;
                if (d <= r && d < bestD) { bestD = d; best = t; }
            }
            best?.OnTap?.Invoke();
        }
    }

    /// <summary>A little animal wandering around a rectangle of the island.</summary>
    public sealed class Wanderer : MonoBehaviour
    {
        public Vector2 Min, Max;
        public float Speed = 1f;
        public float Hop = 0.08f;
        private Vector3 _goal;
        private float _wait;
        private System.Random _rng;

        public void Setup(int seed, Vector2 min, Vector2 max, float speed, float hop)
        {
            _rng = new System.Random(seed);
            Min = min; Max = max; Speed = speed; Hop = hop;
            Pick();
        }

        private void Pick()
        {
            _goal = new Vector3(Mathf.Lerp(Min.x, Max.x, (float)_rng.NextDouble()), 0f, Mathf.Lerp(Min.y, Max.y, (float)_rng.NextDouble()));
        }

        private void Update()
        {
            if (_rng == null) return;
            if (_wait > 0f) { _wait -= Time.deltaTime; return; }
            var p = transform.localPosition;
            var flat = new Vector3(p.x, 0f, p.z);
            var d = _goal - flat;
            if (d.magnitude < 0.2f)
            {
                _wait = 0.5f + (float)_rng.NextDouble() * 2.5f;
                Pick();
                transform.localPosition = flat;
                return;
            }
            var step = d.normalized * Speed * Time.deltaTime;
            flat += step;
            float bob = Mathf.Abs(Mathf.Sin(Time.time * 10f * Speed)) * Hop;
            transform.localPosition = flat + Vector3.up * bob;
            transform.localRotation = Quaternion.LookRotation(d.normalized, Vector3.up);
        }
    }
}
