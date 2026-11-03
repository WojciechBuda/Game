using System.Collections.Generic;
using JellyDrop.City3D;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Runner3D
{
    /// <summary>
    /// The night sky behind Block Run, attached to the run camera: a gradient backdrop that fades into the fog
    /// colour at the horizon, twinkling stars, a glowing moon, slowly waving aurora ribbons, drifting clouds and
    /// the odd shooting star. Everything is unlit and far away, so the street always draws in front of it.
    /// </summary>
    public sealed class RunnerSky : MonoBehaviour
    {
        private const float Distance = 120f;
        private static Material _unlit;

        private readonly List<SpriteRenderer> _stars = new List<SpriteRenderer>();
        private readonly List<float> _starPhase = new List<float>();
        private readonly List<Transform> _aurora = new List<Transform>();
        private readonly List<MeshRenderer> _auroraRenderers = new List<MeshRenderer>();
        private readonly List<Transform> _clouds = new List<Transform>();
        private SpriteRenderer _shooting;
        private Transform _moon;
        private float _nextShot = 3f, _shotTime = -1f;
        private Vector3 _shotFrom, _shotTo;
        private System.Random _rng = new System.Random(7);

        public static Material Unlit
        {
            get
            {
                if (_unlit == null) _unlit = new Material(Shader.Find("Sprites/Default")) { name = "SkyUnlit" };
                return _unlit;
            }
        }

        public static RunnerSky Create(Camera cam, Color horizon)
        {
            var go = new GameObject("Sky");
            go.transform.SetParent(cam.transform, false);
            var sky = go.AddComponent<RunnerSky>();
            sky.Build(cam, horizon);
            return sky;
        }

        private float R(float a, float b) => a + (float)_rng.NextDouble() * (b - a);

        private void Build(Camera cam, Color horizon)
        {
            float halfH = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * Distance;
            float halfW = halfH * 1.2f; // phones are tall; leave slack for the camera's sway
            // Backdrop: horizon colour at the bottom (matches the fog), deep violet at the top.
            var mb = new MeshBuilder { Graded = false };
            var top = ColorUtil.Hex("#0B0C24");
            var mid = ColorUtil.Hex("#2A1F5C");
            Gradient(mb, -halfW, halfW, -halfH * 1.4f, 0f, horizon, horizon, Distance + 4f);
            Gradient(mb, -halfW, halfW, 0f, halfH * 0.5f, horizon, mid, Distance + 4f);
            Gradient(mb, -halfW, halfW, halfH * 0.5f, halfH * 1.4f, mid, top, Distance + 4f);
            Unlit3D("Backdrop", mb.ToMesh("Backdrop"), transform, 0);

            // Aurora ribbons: wide soft bands that wave and pulse.
            Color[] ribbon = { ColorUtil.Hex("#2ED3A0"), ColorUtil.Hex("#3FB6FF"), ColorUtil.Hex("#FF4FA3") };
            for (int i = 0; i < 3; i++)
            {
                mb.Clear();
                mb.Graded = false;
                var c = ribbon[i];
                var clear = new Color(c.r, c.g, c.b, 0f);
                var solid = new Color(c.r, c.g, c.b, 0.32f);
                float y = halfH * (0.35f + i * 0.17f);
                Gradient(mb, -halfW, halfW, y - 9f, y, clear, solid, 0f);
                Gradient(mb, -halfW, halfW, y, y + 6f, solid, clear, 0f);
                var t = Unlit3D("Aurora" + i, mb.ToMesh("Aurora"), transform, 1);
                t.localPosition = new Vector3(0, 0, Distance - 2f - i);
                _aurora.Add(t);
                _auroraRenderers.Add(t.GetComponent<MeshRenderer>());
            }

            // Stars
            for (int i = 0; i < 70; i++)
            {
                var s = SpriteAt("Star", i % 5 == 0 ? SpriteFactory.Sparkle() : SpriteFactory.Circle(),
                    new Vector3(R(-halfW, halfW), R(halfH * 0.1f, halfH * 1.3f), Distance - 6f), R(0.35f, i % 5 == 0 ? 1.6f : 0.8f), 2);
                s.color = i % 7 == 0 ? ColorUtil.Hex("#FFE27A") : i % 4 == 0 ? ColorUtil.Hex("#9FF3FF") : Color.white;
                _stars.Add(s);
                _starPhase.Add(R(0f, 10f));
            }

            // Moon with a soft halo
            var halo = SpriteAt("MoonHalo", SpriteFactory.Glow(), new Vector3(halfW * 0.55f, halfH * 0.95f, Distance - 8f), 26f, 3);
            halo.color = new Color(0.75f, 0.8f, 1f, 0.35f);
            _moon = halo.transform;
            var moon = SpriteAt("Moon", SpriteFactory.Circle(), Vector3.zero, 0.42f, 4);
            moon.transform.SetParent(_moon, false);
            moon.transform.localPosition = new Vector3(0, 0, -0.1f);
            moon.color = ColorUtil.Hex("#F4F0FF");
            var bite = SpriteAt("MoonShade", SpriteFactory.Circle(), Vector3.zero, 0.36f, 5);
            bite.transform.SetParent(_moon, false);
            bite.transform.localPosition = new Vector3(0.13f, 0.07f, -0.2f);
            bite.color = new Color(0.16f, 0.13f, 0.36f, 0.85f);

            // Clouds: dark translucent puffs drifting sideways in front of the stars.
            for (int i = 0; i < 6; i++)
            {
                var cloud = new GameObject("Cloud").transform;
                cloud.SetParent(transform, false);
                cloud.localPosition = new Vector3(R(-halfW, halfW), R(halfH * 0.15f, halfH * 0.8f), Distance - 10f - i);
                for (int k = 0; k < 4; k++)
                {
                    var puff = SpriteAt("Puff", SpriteFactory.Circle(), Vector3.zero, R(5f, 9f), 6);
                    puff.transform.SetParent(cloud, false);
                    puff.transform.localPosition = new Vector3((k - 1.5f) * 5f, R(-1.5f, 1.5f), 0);
                    puff.color = new Color(0.12f, 0.1f, 0.3f, 0.55f);
                }
                _clouds.Add(cloud);
            }

            _shooting = SpriteAt("ShootingStar", SpriteFactory.Glow(), Vector3.zero, 3f, 7);
            _shooting.color = new Color(1f, 1f, 1f, 0f);
        }

        private static void Gradient(MeshBuilder mb, float x0, float x1, float y0, float y1, Color bottom, Color top, float z)
        {
            // Two triangles; colours per vertex are set by splitting into a bottom and a top half-strip.
            int first = mb.VertexCount;
            mb.Quad(new Vector3(x0, y0, z), new Vector3(x0, y1, z), new Vector3(x1, y1, z), new Vector3(x1, y0, z), bottom);
            // Quad() gives all six vertices one colour: repaint the upper ones.
            mb.PaintVertices(first, v => v.y > (y0 + y1) * 0.5f ? top : bottom);
        }

        private static Transform Unlit3D(string name, Mesh mesh, Transform parent, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = Unlit;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            r.sortingOrder = -100 + order;
            return go.transform;
        }

        private SpriteRenderer SpriteAt(string name, Sprite sprite, Vector3 pos, float size, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;
            go.transform.localScale = Vector3.one * size;
            var r = go.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.sortingOrder = -100 + order;
            return r;
        }

        public void Tick(float dt, float speed)
        {
            float t = Time.time;
            for (int i = 0; i < _stars.Count; i++)
            {
                var c = _stars[i].color;
                c.a = 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(t * 1.3f + _starPhase[i]));
                _stars[i].color = c;
            }
            for (int i = 0; i < _aurora.Count; i++)
            {
                var a = _aurora[i];
                a.localPosition = new Vector3(Mathf.Sin(t * 0.13f + i * 2f) * 12f, Mathf.Sin(t * 0.21f + i) * 3f, a.localPosition.z);
                a.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 0.17f + i * 1.3f) * 4f);
                a.localScale = new Vector3(1f, 0.8f + 0.35f * Mathf.Sin(t * 0.4f + i * 1.7f), 1f);
            }
            foreach (var cl in _clouds)
            {
                var p = cl.localPosition;
                p.x -= dt * (1.2f + speed * 0.05f);
                if (p.x < -110f) p.x = 110f;
                cl.localPosition = p;
            }
            if (_moon != null) _moon.localScale = Vector3.one * (26f + Mathf.Sin(t * 0.8f) * 1.2f);

            // Shooting star every few seconds
            _nextShot -= dt;
            if (_shotTime < 0f && _nextShot <= 0f)
            {
                _shotTime = 0f;
                _nextShot = R(3.5f, 8f);
                _shotFrom = new Vector3(R(-60f, 40f), R(45f, 75f), Distance - 7f);
                _shotTo = _shotFrom + new Vector3(R(25f, 45f), -R(15f, 25f), 0f);
            }
            if (_shotTime >= 0f)
            {
                _shotTime += dt / 0.8f;
                float k = Mathf.Clamp01(_shotTime);
                _shooting.transform.localPosition = Vector3.Lerp(_shotFrom, _shotTo, k);
                var d = _shotTo - _shotFrom;
                _shooting.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
                _shooting.transform.localScale = new Vector3(6f, 0.6f, 1f);
                _shooting.color = new Color(1f, 0.95f, 0.8f, Mathf.Sin(k * Mathf.PI));
                if (_shotTime >= 1f) _shotTime = -1f;
            }
        }
    }
}
