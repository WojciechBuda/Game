using JellyDrop.Visuals;
using UnityEngine;

namespace JellyDrop.City3D
{
    /// <summary>Low-poly decorations for the town's islands (see CityCatalog.Decorations). About 1.4 units across.</summary>
    public static class DecorModels
    {
        private static Color H(string hex) => ColorUtil.Hex(hex);

        public static void Build(MeshBuilder mb, string id)
        {
            switch (id)
            {
                case "flowers":
                    mb.Cylinder(Vector3.zero, 0.7f, 0.18f, H("#8A5A34"), 10);
                    mb.Disc(new Vector3(0, 0.19f, 0), 0.62f, H("#5A3A22"), 10);
                    string[] cols = { "#FF5A6E", "#FFC93C", "#B48CFF", "#FFFFFF", "#FF9F43" };
                    for (int i = 0; i < 9; i++)
                    {
                        float a = i * 2.4f, r = i == 0 ? 0f : 0.22f + (i % 2) * 0.2f;
                        var p = new Vector3(Mathf.Cos(a) * r, 0.18f, Mathf.Sin(a) * r);
                        mb.Box(p, new Vector3(0.04f, 0.22f, 0.04f), H("#3E8A44"));
                        mb.Sphere(p + new Vector3(0, 0.26f, 0), 0.09f, H(cols[i % cols.Length]), 3, 6);
                    }
                    break;
                case "lamp":
                    mb.Cylinder(Vector3.zero, 0.22f, 0.12f, H("#3B3F5C"), 8);
                    mb.Cylinder(new Vector3(0, 0.12f, 0), 0.06f, 1.8f, H("#2B2F4A"), 6);
                    mb.Box(new Vector3(0, 1.9f, 0), new Vector3(0.34f, 0.34f, 0.34f), H("#FFE9A8"));
                    mb.Pyramid(new Vector3(0, 2.24f, 0), new Vector3(0.46f, 0.22f, 0.46f), H("#2B2F4A"));
                    break;
                case "bench":
                    foreach (var x in new[] { -0.5f, 0.5f }) mb.Box(new Vector3(x, 0, 0), new Vector3(0.08f, 0.36f, 0.4f), H("#3B3F5C"));
                    mb.Box(new Vector3(0, 0.36f, 0), new Vector3(1.2f, 0.08f, 0.42f), H("#A86F43"));
                    mb.Box(new Vector3(0, 0.44f, -0.2f), new Vector3(1.2f, 0.4f, 0.07f), H("#C98552"));
                    break;
                case "topiary":
                    mb.Box(Vector3.zero, new Vector3(0.6f, 0.4f, 0.6f), H("#C9714A"), H("#D98A5E"));
                    mb.Sphere(new Vector3(0, 0.8f, 0), 0.42f, H("#2F7A3A"), 4, 8);
                    mb.Sphere(new Vector3(0, 1.4f, 0), 0.28f, H("#3E8F4C"), 4, 8);
                    mb.Sphere(new Vector3(0, 1.78f, 0), 0.16f, H("#4FA65A"), 3, 6);
                    break;
                case "pond":
                    mb.Cylinder(Vector3.zero, 0.85f, 0.1f, H("#9EA3B5"), 14);
                    mb.Disc(new Vector3(0, 0.11f, 0), 0.72f, H("#3A86C8"), 14);
                    mb.Disc(new Vector3(0.25f, 0.12f, 0.15f), 0.16f, H("#4FA65A"), 8);
                    mb.Disc(new Vector3(-0.3f, 0.12f, -0.2f), 0.12f, H("#4FA65A"), 8);
                    mb.Sphere(new Vector3(0.25f, 0.17f, 0.15f), 0.05f, H("#FF8FC0"), 2, 5);
                    for (int i = 0; i < 4; i++) { float a = i * 1.7f; mb.Sphere(new Vector3(Mathf.Cos(a) * 0.85f, 0.12f, Mathf.Sin(a) * 0.85f), 0.13f, H("#8A8FA3"), 3, 5); }
                    break;
                case "fountain":
                    mb.Cylinder(Vector3.zero, 0.85f, 0.3f, H("#D9D2C3"), 14);
                    mb.Disc(new Vector3(0, 0.31f, 0), 0.72f, H("#4FA8E8"), 14);
                    mb.Cylinder(new Vector3(0, 0.3f, 0), 0.14f, 0.7f, H("#CFC7B5"), 8);
                    mb.Cylinder(new Vector3(0, 1.0f, 0), 0.42f, 0.12f, H("#D9D2C3"), 12);
                    mb.Disc(new Vector3(0, 1.13f, 0), 0.34f, H("#6CC0F5"), 12);
                    mb.Sphere(new Vector3(0, 1.32f, 0), 0.16f, H("#BFE8FF"), 3, 7);
                    break;
                case "statue":
                    mb.Box(Vector3.zero, new Vector3(0.9f, 0.5f, 0.9f), H("#B8B2A6"), H("#CFC9BC"));
                    mb.Box(new Vector3(0, 0.5f, 0), new Vector3(0.6f, 0.15f, 0.6f), H("#CFC9BC"));
                    // A golden block hero on top
                    mb.Box(new Vector3(0, 0.65f, 0), new Vector3(0.5f, 0.55f, 0.45f), H("#E0B23A"), H("#FFD66B"));
                    mb.Box(new Vector3(-0.32f, 0.85f, 0), new Vector3(0.12f, 0.45f, 0.12f), H("#E0B23A"));
                    mb.Box(new Vector3(0.32f, 1.0f, 0), new Vector3(0.12f, 0.5f, 0.12f), H("#E0B23A"));
                    mb.Sphere(new Vector3(0.32f, 1.55f, 0), 0.12f, H("#FFE27A"), 3, 6);
                    break;
                case "gazebo":
                    mb.Cylinder(Vector3.zero, 0.85f, 0.15f, H("#D9D2C3"), 8);
                    for (int i = 0; i < 6; i++) { float a = i * Mathf.PI / 3f; mb.Box(new Vector3(Mathf.Cos(a) * 0.68f, 0.15f, Mathf.Sin(a) * 0.68f), new Vector3(0.08f, 1.1f, 0.08f), H("#F2EEE6")); }
                    mb.Cylinder(new Vector3(0, 1.25f, 0), 0.92f, 0.1f, H("#F2EEE6"), 8);
                    mb.Cylinder(new Vector3(0, 1.35f, 0), 0.95f, 0.65f, H("#3FA39A"), 8, 0.05f);
                    mb.Sphere(new Vector3(0, 2.05f, 0), 0.08f, H("#FFC93C"), 3, 6);
                    break;
                default:
                    BuildingModels.Bush(mb, Vector3.zero, 0.4f);
                    break;
            }
        }
    }
}
