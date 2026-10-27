using UnityEngine;
using UnityEngine.Rendering;

namespace JellyDrop.Visuals
{
    /// <summary>
    /// Makes sprites render unlit (full colour) under URP's 2D renderer even when the scene has no
    /// 2D lights. With the built-in render pipeline nothing changes.
    /// </summary>
    public static class SpriteMaterial
    {
        private static Material _unlit;
        private static bool _resolved;

        /// <summary>Adds a SpriteRenderer with the right material.</summary>
        public static SpriteRenderer Add(GameObject go)
        {
            var r = go.AddComponent<SpriteRenderer>();
            Apply(r);
            return r;
        }

        public static void Apply(SpriteRenderer renderer)
        {
            if (!_resolved)
            {
                _resolved = true;
                if (GraphicsSettings.currentRenderPipeline != null)
                {
                    var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                    if (shader != null) _unlit = new Material(shader) { name = "JellyDrop Sprite Unlit" };
                }
            }
            if (_unlit != null) renderer.sharedMaterial = _unlit;
        }
    }
}
