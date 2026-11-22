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
                var shader = Shader.Find("Standard") ?? Shader.Find("Diffuse") ?? Shader.Find("Unlit/Color");
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
            var def = ItemDefinition.Get(type);
            var go = CreateColoredPrimitive(def.Mesh, position, def.Scale, def.Color, $"Pickup_{def.DisplayName}");
            var col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            var pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(type);

            // Gentle bob
            go.AddComponent<PickupBob>();
            return pickup;
        }

        public static Campfire CreateCampfire(Vector3 position)
        {
            var root = new GameObject("Campfire");
            root.transform.position = position;

            CreateColoredPrimitive(PrimitiveType.Cylinder, position, new Vector3(1.1f, 0.12f, 1.1f), new Color(0.25f, 0.2f, 0.15f), "Ring", true)
                .transform.SetParent(root.transform, true);

            // Log pile
            for (var i = 0; i < 3; i++)
            {
                var angle = i * 60f;
                var log = CreateColoredPrimitive(
                    PrimitiveType.Cylinder,
                    position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * 0.15f, 0.18f, Mathf.Sin(angle * Mathf.Deg2Rad) * 0.15f),
                    new Vector3(0.18f, 0.45f, 0.18f),
                    new Color(0.4f, 0.25f, 0.12f),
                    $"Log_{i}",
                    false);
                log.transform.rotation = Quaternion.Euler(0f, angle, 70f);
                log.transform.SetParent(root.transform, true);
            }

            var flame = CreateColoredPrimitive(
                PrimitiveType.Sphere,
                position + Vector3.up * 0.55f,
                new Vector3(0.45f, 0.7f, 0.45f),
                new Color(1f, 0.45f, 0.1f),
                "Flame",
                false);
            flame.transform.SetParent(root.transform, true);

            var lightGo = new GameObject("FireLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = Vector3.up * 0.8f;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.2f);
            light.range = 8f;
            light.intensity = 2.2f;

            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.4f;

            var campfire = root.AddComponent<Campfire>();
            campfire.ConfigureVisuals(flame.transform, light);
            root.tag = "Campfire";
            return campfire;
        }
    }

    public class PickupBob : MonoBehaviour
    {
        Vector3 _origin;
        float _seed;

        void Start()
        {
            _origin = transform.position;
            _seed = Random.value * 10f;
        }

        void Update()
        {
            transform.position = _origin + Vector3.up * (Mathf.Sin(Time.time * 2f + _seed) * 0.08f);
            transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);
        }
    }
}
