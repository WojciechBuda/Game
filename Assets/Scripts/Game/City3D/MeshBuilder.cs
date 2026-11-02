using System.Collections.Generic;
using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>
    /// Builds flat-shaded, vertex-coloured low-poly meshes from simple shapes (boxes, roofs, cylinders, cones,
    /// spheres). Every face gets its own vertices so lighting looks crisp and faceted.
    /// </summary>
    public sealed class MeshBuilder
    {
        private readonly List<Vector3> _v = new List<Vector3>();
        private readonly List<Vector3> _n = new List<Vector3>();
        private readonly List<Color> _c = new List<Color>();
        private readonly List<int> _t = new List<int>();

        /// <summary>Applied to every shape added (lets models be built in local space and placed/rotated).</summary>
        public Matrix4x4 Transform = Matrix4x4.identity;

        public int VertexCount => _v.Count;
        public IReadOnlyList<Vector3> Vertices => _v;
        public IReadOnlyList<Color> Colors => _c;

        /// <summary>Changes the colours of everything added since <paramref name="first"/> (keeps the face shading).</summary>
        public void Recolor(int first, System.Func<Color, Color> map)
        {
            for (int i = first; i < _c.Count; i++) _c[i] = map(_c[i]);
        }
        public IReadOnlyList<int> Triangles => _t;

        public void Clear()
        {
            _v.Clear(); _n.Clear(); _c.Clear(); _t.Clear();
            Transform = Matrix4x4.identity;
        }

        public void Triangle(Vector3 a, Vector3 b, Vector3 c, Color col)
        {
            a = Transform.MultiplyPoint3x4(a);
            b = Transform.MultiplyPoint3x4(b);
            c = Transform.MultiplyPoint3x4(c);
            var n = Vector3.Cross(b - a, c - a).normalized;
            int i = _v.Count;
            _v.Add(a); _v.Add(b); _v.Add(c);
            _n.Add(n); _n.Add(n); _n.Add(n);
            _c.Add(col); _c.Add(col); _c.Add(col);
            _t.Add(i); _t.Add(i + 1); _t.Add(i + 2);
        }

        /// <summary>Quad a-b-c-d in clockwise order when seen from the front (Unity's winding).</summary>
        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color col)
        {
            Triangle(a, b, c, col);
            Triangle(a, c, d, col);
        }

        /// <summary>Axis-aligned box. <paramref name="bottom"/> is the centre of its base.</summary>
        public void Box(Vector3 bottom, Vector3 size, Color col, Color? top = null)
        {
            float hx = size.x / 2f, hz = size.z / 2f, h = size.y;
            var p = bottom;
            Vector3 b0 = p + new Vector3(-hx, 0, -hz), b1 = p + new Vector3(hx, 0, -hz), b2 = p + new Vector3(hx, 0, hz), b3 = p + new Vector3(-hx, 0, hz);
            Vector3 t0 = b0 + Vector3.up * h, t1 = b1 + Vector3.up * h, t2 = b2 + Vector3.up * h, t3 = b3 + Vector3.up * h;
            var side = col;
            Quad(b0, t0, t1, b1, side);                     // front (-z)
            Quad(b1, t1, t2, b2, Shade(side, 0.94f));       // right
            Quad(b2, t2, t3, b3, side);                     // back
            Quad(b3, t3, t0, b0, Shade(side, 0.94f));       // left
            Quad(t0, t3, t2, t1, top ?? Shade(col, 1.08f)); // top
        }

        /// <summary>Gable roof (ridge along x) sitting on <paramref name="bottom"/>, with a small overhang.</summary>
        public void Roof(Vector3 bottom, Vector3 size, Color col)
        {
            float hx = size.x / 2f, hz = size.z / 2f, h = size.y;
            var p = bottom;
            Vector3 a = p + new Vector3(-hx, 0, -hz), b = p + new Vector3(hx, 0, -hz), c = p + new Vector3(hx, 0, hz), d = p + new Vector3(-hx, 0, hz);
            Vector3 r0 = p + new Vector3(-hx, h, 0), r1 = p + new Vector3(hx, h, 0);
            Quad(a, r0, r1, b, col);                    // front slope
            Quad(c, r1, r0, d, Shade(col, 0.85f));      // back slope
            Triangle(b, r1, c, Shade(col, 0.92f));      // right gable
            Triangle(d, r0, a, Shade(col, 0.92f));      // left gable
        }

        /// <summary>Four-sided pyramid roof.</summary>
        public void Pyramid(Vector3 bottom, Vector3 size, Color col)
        {
            float hx = size.x / 2f, hz = size.z / 2f;
            var p = bottom;
            Vector3 a = p + new Vector3(-hx, 0, -hz), b = p + new Vector3(hx, 0, -hz), c = p + new Vector3(hx, 0, hz), d = p + new Vector3(-hx, 0, hz);
            var top = p + Vector3.up * size.y;
            Triangle(a, top, b, col);
            Triangle(b, top, c, Shade(col, 0.9f));
            Triangle(c, top, d, Shade(col, 0.85f));
            Triangle(d, top, a, Shade(col, 0.95f));
        }

        /// <summary>Cylinder (or truncated cone when the radii differ). Set <paramref name="topRadius"/> to 0 for a cone.</summary>
        public void Cylinder(Vector3 bottom, float radius, float height, Color col, int segments = 10, float topRadius = -1f, bool caps = true)
        {
            if (topRadius < 0f) topRadius = radius;
            var top = bottom + Vector3.up * height;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f, a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                var d0 = new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0));
                var d1 = new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1));
                var shade = Shade(col, 0.88f + 0.12f * Mathf.Abs(Mathf.Cos((a0 + a1) / 2f - 0.6f)));
                if (topRadius > 0.001f) Quad(bottom + d0 * radius, top + d0 * topRadius, top + d1 * topRadius, bottom + d1 * radius, shade);
                else Triangle(bottom + d0 * radius, top, bottom + d1 * radius, shade);
                if (caps && topRadius > 0.001f) Triangle(top, top + d1 * topRadius, top + d0 * topRadius, Shade(col, 1.08f));
                if (caps) Triangle(bottom, bottom + d0 * radius, bottom + d1 * radius, Shade(col, 0.7f));
            }
        }

        public void Cone(Vector3 bottom, float radius, float height, Color col, int segments = 8) =>
            Cylinder(bottom, radius, height, col, segments, 0f);

        /// <summary>Low-poly sphere (faceted).</summary>
        public void Sphere(Vector3 center, float radius, Color col, int rings = 5, int segments = 8)
        {
            for (int r = 0; r < rings; r++)
            {
                float t0 = Mathf.PI * r / rings - Mathf.PI / 2f, t1 = Mathf.PI * (r + 1) / rings - Mathf.PI / 2f;
                for (int s = 0; s < segments; s++)
                {
                    float p0 = s / (float)segments * Mathf.PI * 2f, p1 = (s + 1) / (float)segments * Mathf.PI * 2f;
                    Vector3 P(float t, float p) => center + new Vector3(Mathf.Cos(t) * Mathf.Cos(p), Mathf.Sin(t), Mathf.Cos(t) * Mathf.Sin(p)) * radius;
                    var shade = Shade(col, 0.85f + 0.2f * (r / (float)rings));
                    var a = P(t0, p0); var b = P(t1, p0); var c = P(t1, p1); var d = P(t0, p1);
                    if (r == 0) Triangle(a, b, c, shade);
                    else if (r == rings - 1) Triangle(a, b, d, shade);
                    else Quad(a, b, c, d, shade);
                }
            }
        }

        /// <summary>Flat disc (e.g. ground, water) facing up.</summary>
        public void Disc(Vector3 center, float radius, Color col, int segments = 32)
        {
            for (int i = 0; i < segments; i++)
            {
                float a0 = i / (float)segments * Mathf.PI * 2f, a1 = (i + 1) / (float)segments * Mathf.PI * 2f;
                Triangle(center, center + new Vector3(Mathf.Cos(a1), 0, Mathf.Sin(a1)) * radius, center + new Vector3(Mathf.Cos(a0), 0, Mathf.Sin(a0)) * radius, col);
            }
        }

        public Mesh ToMesh(string name = "Mesh")
        {
            var m = new Mesh { name = name };
            if (_v.Count > 65000) m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            m.SetVertices(_v);
            m.SetNormals(_n);
            m.SetColors(_c);
            m.SetTriangles(_t, 0);
            m.RecalculateBounds();
            return m;
        }

        public static Color Shade(Color c, float k) => new Color(Mathf.Clamp01(c.r * k), Mathf.Clamp01(c.g * k), Mathf.Clamp01(c.b * k), c.a);

        // ------------------------------------------------------------------ scene helpers

        private static Material _material;

        /// <summary>The shared vertex-colour lit material (falls back to Standard if the shader is missing).</summary>
        public static Material SharedMaterial
        {
            get
            {
                if (_material != null) return _material;
                var shader = Shader.Find("Stackville/VertexColorLit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
                _material = new Material(shader) { name = "LowPoly" };
                return _material;
            }
        }

        /// <summary>Creates a GameObject showing this mesh.</summary>
        public GameObject ToObject(string name, Transform parent, bool castShadows = true)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = ToMesh(name);
            var r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = SharedMaterial;
            r.shadowCastingMode = castShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = true;
            return go;
        }
    }
}
