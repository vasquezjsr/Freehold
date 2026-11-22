using Freehold.Art;
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
                ArtPalette.Grass,
                "Ground");
            ground.transform.position = new Vector3(0f, -0.1f, 0f);

            // Soft dirt patches (flat discs — ground plane only, not lighting)
            for (var i = 0; i < 8; i++)
            {
                var pos = new Vector3(Random.Range(-30f, 30f), 0.01f, Random.Range(-30f, 30f));
                PrimitiveFactory.CreateColoredPrimitive(
                    PrimitiveType.Cylinder,
                    pos,
                    new Vector3(3f + Random.value * 3f, 0.02f, 3f + Random.value * 3f),
                    ArtPalette.Dirt,
                    $"Dirt_{i}",
                    false);
            }
        }

        // Intentionally unchanged lighting / fog / sun setup (art pass does not touch lighting).
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
                PropFactory.CreateTree(pos, i);
            }

            for (var i = 0; i < 18; i++)
            {
                var pos = RandomRingPosition(4f, 34f);
                PropFactory.CreateRock(pos, i);
            }
        }

        static void CreateLandmarkPond()
        {
            PropFactory.CreatePond(new Vector3(12f, 0.02f, -8f));
        }

        static void ScatterLoot()
        {
            Spawn(ItemType.Axe, new Vector3(-6f, 0.15f, 4f));
            Spawn(ItemType.Pickaxe, new Vector3(8f, 0.15f, 10f));
            Spawn(ItemType.Knife, new Vector3(-10f, 0.1f, -3f));
            Spawn(ItemType.PlasticBottle, new Vector3(11f, 0.05f, -6f));
            Spawn(ItemType.DirtyWaterBottle, new Vector3(13f, 0.05f, -9f));
            Spawn(ItemType.RawFood, new Vector3(3f, 0.08f, -5f));
            Spawn(ItemType.RawFood, new Vector3(-14f, 0.08f, 8f));
            Spawn(ItemType.Stick, new Vector3(2f, 0.05f, 2f));
            Spawn(ItemType.Stick, new Vector3(-3f, 0.05f, -4f));
            Spawn(ItemType.Stick, new Vector3(5f, 0.05f, 7f));
            Spawn(ItemType.Stick, new Vector3(-8f, 0.05f, 6f));
            Spawn(ItemType.Stick, new Vector3(9f, 0.05f, -2f));
            Spawn(ItemType.Stone, new Vector3(-2f, 0.05f, 9f));
            Spawn(ItemType.Stone, new Vector3(6f, 0.05f, -11f));

            for (var i = 0; i < 10; i++)
            {
                var pos = RandomRingPosition(3f, 22f) + Vector3.up * 0.05f;
                Spawn(Random.value > 0.4f ? ItemType.Stick : ItemType.Stone, pos);
            }
        }

        static void Spawn(ItemType type, Vector3 position)
        {
            PropFactory.CreatePickup(type, position);
        }

        static Vector3 RandomRingPosition(float minRadius, float maxRadius)
        {
            var angle = Random.Range(0f, Mathf.PI * 2f);
            var radius = Random.Range(minRadius, maxRadius);
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
