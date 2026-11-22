using Freehold.Crafting;
using Freehold.Items;
using UnityEngine;

namespace Freehold.World
{
    public static class PrimitiveFactory
    {
        static Material _sharedLit;

        public static Material SharedLit()
        {
            if (_sharedLit == null)
            {
                // Built-in RP (Unity 6): Standard is the lit default. Keep simple fallbacks.
                var shader = Shader.Find("Standard")
                             ?? Shader.Find("Legacy Shaders/Diffuse")
                             ?? Shader.Find("Unlit/Color");
                if (shader == null)
                {
                    Debug.LogError("Freehold: no suitable Built-in shader found. Stay on Built-in RP (not URP/HDRP).");
                    shader = Shader.Find("Hidden/InternalErrorShader");
                }

                _sharedLit = new Material(shader);
            }

            return _sharedLit;
        }

        public static Material ColorMaterial(Color color)
        {
            var mat = new Material(SharedLit());
            mat.color = color;
            return mat;
        }

        public static GameObject CreateColoredPrimitive(PrimitiveType type, Vector3 position, Vector3 scale, Color color, string name, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.position = position;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().sharedMaterial = ColorMaterial(color);

            if (!collider)
            {
                var col = go.GetComponent<Collider>();
                if (col != null)
                {
                    Object.Destroy(col);
                }
            }

            return go;
        }

        public static ItemPickup CreatePickup(ItemType type, Vector3 position)
        {
            return Freehold.Art.Pack.PackPropFactory.CreatePickup(type, position);
        }

        public static Campfire CreateCampfire(Vector3 position)
        {
            return Freehold.Art.Pack.PackPropFactory.CreateCampfire(position);
        }
    }

    /// <summary>Deprecated — ground loot is static. Kept so old scenes don't break if referenced.</summary>
    public class PickupBob : MonoBehaviour
    {
        void Start()
        {
            enabled = false;
        }
    }
}
