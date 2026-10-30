using UnityEngine;
using UnityEngine.UI;

namespace JellyDrop.Interface
{
    /// <summary>
    /// Vertical colour gradient for any UI graphic (images, sliced panels, text). The colours multiply the
    /// graphic's own colour, so a white-to-grey gradient makes a lit-from-above face in any tint.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public sealed class UIGradient : BaseMeshEffect
    {
        public Color Top = Color.white;
        public Color Bottom = Color.white;

        public static UIGradient Add(Graphic graphic, Color top, Color bottom)
        {
            var g = graphic.gameObject.GetComponent<UIGradient>();
            if (g == null) g = graphic.gameObject.AddComponent<UIGradient>();
            g.Set(top, bottom);
            return g;
        }

        public void Set(Color top, Color bottom)
        {
            Top = top;
            Bottom = bottom;
            if (graphic != null) graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;
            var v = new UIVertex();
            // Use the real extent of the vertices (text glyphs rarely fill their rect).
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                if (v.position.y < min) min = v.position.y;
                if (v.position.y > max) max = v.position.y;
            }
            float h = Mathf.Max(0.0001f, max - min);
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                var c = Color.Lerp(Bottom, Top, (v.position.y - min) / h);
                Color original = v.color;
                v.color = original * c;
                vh.SetUIVertex(v, i);
            }
        }
    }
}
