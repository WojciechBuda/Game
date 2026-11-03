using JellyDrop.City3D;
using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.Runner3D
{
    /// <summary>
    /// The runner's 3D character: a chunky jelly block in the avatar's colour with a backpack, a glowing antenna,
    /// and arms and legs that swing while running (the camera sees it from behind, so the depth reads).
    /// The transform's origin is the middle of the body, 0.75 above the feet.
    /// </summary>
    public sealed class RunnerHero : MonoBehaviour
    {
        private Transform _legL, _legR, _armL, _armR, _body, _antenna;
        private MeshRenderer[] _renderers;
        private Color _color = new Color(1f, 0.45f, 0.7f);

        public static RunnerHero Create(Transform parent)
        {
            var go = new GameObject("Hero");
            go.transform.SetParent(parent, false);
            var h = go.AddComponent<RunnerHero>();
            h.Build(h._color);
            return h;
        }

        public void SetColor(Color body)
        {
            if (Approximately(body, _color)) return;
            _color = body;
            foreach (Transform c in transform) Destroy(c.gameObject);
            Build(body);
        }

        private static bool Approximately(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b) < 0.01f;

        // Part offsets (body space).
        public static readonly Vector3 AntennaPos = new Vector3(0.12f, 0.44f, -0.05f);
        public static readonly Vector3 LegLPos = new Vector3(-0.2f, -0.37f, 0), LegRPos = new Vector3(0.2f, -0.37f, 0);
        public static readonly Vector3 ArmLPos = new Vector3(-0.52f, 0.02f, 0), ArmRPos = new Vector3(0.52f, 0.02f, 0);

        private static readonly Color Ink = ColorUtil.Hex("#1A1530");
        private static readonly Color Pack = ColorUtil.Hex("#2B2F5E");
        private static readonly Color Glow = ColorUtil.Hex("#3DE0FF");

        /// <summary>Body: a rounded block (core box + inset "bevel" boxes), face on the front, backpack on the back.</summary>
        public static void BodyMesh(MeshBuilder mb, Color body)
        {
            var light = PixelCanvas.Lighten(body, 0.22f);
            mb.Box(new Vector3(0, -0.36f, 0), new Vector3(0.82f, 0.78f, 0.7f), body, light);
            mb.Box(new Vector3(0, -0.3f, 0), new Vector3(0.94f, 0.66f, 0.6f), body, light);
            mb.Box(new Vector3(0, -0.3f, 0), new Vector3(0.7f, 0.66f, 0.82f), body, light);
            mb.Box(new Vector3(0, 0.42f, 0), new Vector3(0.7f, 0.06f, 0.58f), light);
            foreach (var x in new[] { -0.17f, 0.17f })
            {
                mb.Box(new Vector3(x, 0.0f, 0.41f), new Vector3(0.18f, 0.2f, 0.04f), Color.white);
                mb.Box(new Vector3(x, 0.03f, 0.43f), new Vector3(0.09f, 0.11f, 0.03f), Ink);
            }
            mb.Box(new Vector3(0, -0.16f, 0.41f), new Vector3(0.2f, 0.05f, 0.03f), Ink);
            mb.Box(new Vector3(0, -0.24f, -0.44f), new Vector3(0.56f, 0.5f, 0.16f), Pack);
            mb.Box(new Vector3(0, -0.08f, -0.53f), new Vector3(0.44f, 0.08f, 0.03f), Glow);
            mb.Box(new Vector3(0, -0.3f, -0.53f), new Vector3(0.3f, 0.16f, 0.04f), PixelCanvas.Lighten(Pack, 0.15f));
            foreach (var x in new[] { -0.22f, 0.22f }) mb.Box(new Vector3(x, 0.12f, -0.38f), new Vector3(0.07f, 0.3f, 0.06f), Pack);
        }

        public static void AntennaMesh(MeshBuilder mb, Color body)
        {
            mb.Cylinder(Vector3.zero, 0.035f, 0.32f, PixelCanvas.Darken(body, 0.3f), 6);
            mb.Sphere(new Vector3(0, 0.36f, 0), 0.09f, Glow, 3, 8);
        }

        public static void LegMesh(MeshBuilder mb, Color body)
        {
            mb.Box(new Vector3(0, -0.38f, 0), new Vector3(0.22f, 0.38f, 0.24f), PixelCanvas.Darken(body, 0.3f));
            mb.Box(new Vector3(0, -0.42f, 0.04f), new Vector3(0.26f, 0.1f, 0.34f), ColorUtil.Hex("#F2F0FF"));
        }

        public static void ArmMesh(MeshBuilder mb, Color body)
        {
            var light = PixelCanvas.Lighten(body, 0.22f);
            mb.Box(new Vector3(0, -0.32f, 0), new Vector3(0.16f, 0.32f, 0.16f), body, light);
            mb.Sphere(new Vector3(0, -0.34f, 0), 0.1f, light, 3, 6);
        }

        private void Build(Color body)
        {
            var mb = new MeshBuilder { Graded = false };
            BodyMesh(mb, body);
            _body = Part("Body", mb, transform, Vector3.zero);
            mb.Clear(); mb.Graded = false;
            AntennaMesh(mb, body);
            _antenna = Part("Antenna", mb, _body, AntennaPos);
            mb.Clear(); mb.Graded = false;
            LegMesh(mb, body);
            var legMesh = mb.ToMesh("Leg");
            _legL = MeshPart("LegL", legMesh, transform, LegLPos);
            _legR = MeshPart("LegR", legMesh, transform, LegRPos);
            mb.Clear(); mb.Graded = false;
            ArmMesh(mb, body);
            var armMesh = mb.ToMesh("Arm");
            _armL = MeshPart("ArmL", armMesh, _body, ArmLPos);
            _armR = MeshPart("ArmR", armMesh, _body, ArmRPos);
            _renderers = GetComponentsInChildren<MeshRenderer>();
        }

        private static Transform Part(string name, MeshBuilder mb, Transform parent, Vector3 pos)
        {
            var go = mb.ToObject(name, parent);
            go.transform.localPosition = pos;
            return go.transform;
        }

        private static Transform MeshPart(string name, Mesh mesh, Transform parent, Vector3 pos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = MeshBuilder.SharedMaterial;
            return go.transform;
        }

        // Smoothed pose weights (0..1), so the hero eases between running, jumping, sliding and crashing.
        private float _jumpW, _slideW, _crashW;

        /// <summary>
        /// Animates the hero in 3D: arms and legs swing while running, a tuck in the air, and a real slide
        /// (body drops and leans back, legs go forward) instead of squashing the model flat. Poses blend smoothly.
        /// </summary>
        public void Pose(float runPhase, bool jumping, bool sliding, bool crashed, float dt)
        {
            float k = 1f - Mathf.Exp(-Mathf.Max(0f, dt) * 14f);
            _jumpW = Mathf.Lerp(_jumpW, jumping && !crashed ? 1f : 0f, k);
            _slideW = Mathf.Lerp(_slideW, sliding && !crashed ? 1f : 0f, k);
            _crashW = Mathf.Lerp(_crashW, crashed ? 1f : 0f, k);
            float run = Mathf.Clamp01(1f - _jumpW - _slideW - _crashW);
            float swing = Mathf.Sin(runPhase);

            // Legs: swing while running, tuck in the air, stretch forward in the slide.
            float legL = swing * 42f * run + 35f * _jumpW - 82f * _slideW;
            float legR = -swing * 42f * run - 20f * _jumpW - 74f * _slideW;
            // Arms: swing opposite the legs, up in the air, back for balance in the slide.
            float armL = -swing * 48f * run - 140f * _jumpW + 55f * _slideW + 150f * _crashW;
            float armR = swing * 48f * run - 140f * _jumpW + 65f * _slideW + 150f * _crashW;
            float spread = 6f + 19f * _jumpW + 20f * _slideW + 30f * _crashW;
            // Body: leans into the run, upright in the air, drops and leans back in the slide.
            float lean = 10f * run - 8f * _jumpW - 52f * _slideW;
            float drop = 0.36f * _slideW;
            float bob = Mathf.Abs(swing) * 0.04f * run;

            _body.localPosition = new Vector3(0f, -drop + bob, -0.1f * _slideW);
            _body.localRotation = Quaternion.Euler(lean, 0f, swing * 4f * run);
            _legL.localPosition = LegLPos + new Vector3(0f, -drop * 0.9f, 0f);
            _legR.localPosition = LegRPos + new Vector3(0f, -drop * 0.9f, 0f);
            _legL.localRotation = Quaternion.Euler(legL, 0f, -4f * _slideW);
            _legR.localRotation = Quaternion.Euler(legR, 0f, 4f * _slideW);
            _armL.localRotation = Quaternion.Euler(armL, 0f, -spread);
            _armR.localRotation = Quaternion.Euler(armR, 0f, spread);
            _antenna.localRotation = Quaternion.Euler(-lean * 1.2f + swing * 10f * run + 25f * _jumpW, 0f, swing * 6f * run);
        }

        public void SetVisible(bool visible)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers) if (r != null) r.enabled = visible;
        }
    }
}
