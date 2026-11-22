using Freehold.Items;
using UnityEngine;

namespace Freehold.World
{
    public static class StarterAreaBuilder
    {
        public static void Build()
        {
            BuildGround();
            BuildSky();
            ScatterTreesAndRocks();
            ScatterLoot();
            CreateLandmarkPond();
        }

        static void BuildGround()
        {
            var ground = PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Cube,
                Vector3.zero,
                new Vector3(80f, 0.2f, 80f),
                new Color(0.45f, 0.7f, 0.38f),
                "Ground");
            ground.transform.position = new Vector3(0f, -0.1f, 0f);

            // Soft dirt patches
            for (var i = 0; i < 8; i++)
            {
                var pos = new Vector3(Random.Range(-30f, 30f), 0.01f, Random.Range(-30f, 30f));
                PrimitiveFactory.CreateColoredPrimitive(
                    PrimitiveType.Cylinder,
                    pos,
                    new Vector3(3f + Random.value * 3f, 0.02f, 3f + Random.value * 3f),
                    new Color(0.55f, 0.45f, 0.28f),
                    $"Dirt_{i}",
                    false);
            }
        }

        static void BuildSky()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.7f, 0.9f);
            RenderSettings.ambientEquatorColor = new Color(0.55f, 0.62f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.25f, 0.22f, 0.18f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.012f;
            RenderSettings.fogColor = new Color(0.7f, 0.8f, 0.85f);

            if (Camera.main != null)
            {
                Camera.main.clearFlags = CameraClearFlags.SolidColor;
                Camera.main.backgroundColor = new Color(0.55f, 0.75f, 0.92f);
            }

            var sun = new GameObject("Sun");
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.85f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42f, -30f, 0f);
        }

        static void ScatterTreesAndRocks()
        {
            for (var i = 0; i < 28; i++)
            {
                var pos = RandomRingPosition(6f, 36f);
                CreateTree(pos);
            }

            for (var i = 0; i < 18; i++)
            {
                var pos = RandomRingPosition(4f, 34f);
                var scale = 0.4f + Random.value * 0.7f;
                PrimitiveFactory.CreateColoredPrimitive(
                    PrimitiveType.Sphere,
                    pos + Vector3.up * (scale * 0.35f),
                    Vector3.one * scale,
                    new Color(0.5f, 0.5f, 0.48f),
                    $"Rock_{i}");
            }
        }

        static void CreateTree(Vector3 pos)
        {
            var root = new GameObject("Tree");
            root.transform.position = pos;

            var trunk = PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Cylinder,
                pos + Vector3.up * 0.7f,
                new Vector3(0.35f, 0.7f, 0.35f),
                new Color(0.4f, 0.25f, 0.12f),
                "Trunk");
            trunk.transform.SetParent(root.transform, true);

            var canopyColor = Color.Lerp(new Color(0.25f, 0.55f, 0.28f), new Color(0.35f, 0.7f, 0.3f), Random.value);
            var canopy = PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Sphere,
                pos + Vector3.up * 2.1f,
                Vector3.one * (1.6f + Random.value * 0.6f),
                canopyColor,
                "Canopy");
            canopy.transform.SetParent(root.transform, true);
        }

        static void CreateLandmarkPond()
        {
            PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Cylinder,
                new Vector3(12f, 0.02f, -8f),
                new Vector3(6f, 0.05f, 6f),
                new Color(0.25f, 0.55f, 0.75f),
                "Pond",
                false);
        }

        static void ScatterLoot()
        {
            Spawn(ItemType.Axe, new Vector3(-6f, 0.35f, 4f));
            Spawn(ItemType.Pickaxe, new Vector3(8f, 0.35f, 10f));
            Spawn(ItemType.Knife, new Vector3(-10f, 0.25f, -3f));
            Spawn(ItemType.PlasticBottle, new Vector3(11f, 0.25f, -6f));
            Spawn(ItemType.DirtyWaterBottle, new Vector3(13f, 0.25f, -9f));
            Spawn(ItemType.RawFood, new Vector3(3f, 0.2f, -5f));
            Spawn(ItemType.RawFood, new Vector3(-14f, 0.2f, 8f));
            Spawn(ItemType.Stick, new Vector3(2f, 0.3f, 2f));
            Spawn(ItemType.Stick, new Vector3(-3f, 0.3f, -4f));
            Spawn(ItemType.Stick, new Vector3(5f, 0.3f, 7f));
            Spawn(ItemType.Stick, new Vector3(-8f, 0.3f, 6f));
            Spawn(ItemType.Stick, new Vector3(9f, 0.3f, -2f));
            Spawn(ItemType.Stone, new Vector3(-2f, 0.2f, 9f));
            Spawn(ItemType.Stone, new Vector3(6f, 0.2f, -11f));

            // Extra scattered sticks/stones for campfire crafting
            for (var i = 0; i < 10; i++)
            {
                var pos = RandomRingPosition(3f, 22f) + Vector3.up * 0.25f;
                Spawn(Random.value > 0.4f ? ItemType.Stick : ItemType.Stone, pos);
            }
        }

        static void Spawn(ItemType type, Vector3 position)
        {
            PrimitiveFactory.CreatePickup(type, position);
        }

        static Vector3 RandomRingPosition(float minRadius, float maxRadius)
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var radius = Random.Range(minRadius, maxRadius);
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
