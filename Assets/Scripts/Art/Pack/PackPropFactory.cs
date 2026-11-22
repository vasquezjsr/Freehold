using Freehold.Art;
using Freehold.Crafting;
using Freehold.Items;
using Freehold.World;
using UnityEngine;

namespace Freehold.Art.Pack
{
    /// <summary>Spawns environment/loot from vendored CC0 packs (Kenney + Quaternius).</summary>
    public static class PackPropFactory
    {
        static readonly string[] TreeNames =
        {
            "tree_default", "tree_default_dark", "tree_pineDefaultA", "tree_pineDefaultB",
            "tree_oak", "tree_oak_dark", "tree_cone", "tree_detailed", "tree_fat", "tree_pineRoundA"
        };

        static readonly string[] RockNames =
        {
            "rock_largeA", "rock_largeB", "rock_largeC", "rock_largeD", "rock_largeE", "rock_largeF",
            "rock_smallA", "rock_smallB", "rock_smallC", "rock_smallD", "rock_tallA", "rock_tallB"
        };

        public static GameObject CreateTree(Vector3 position, int variant)
        {
            var name = TreeNames[variant % TreeNames.Length];
            var go = CreateKenneyObject(name, $"Tree_{name}", position, addCollider: true);
            if (go == null)
            {
                return Freehold.Art.PropFactory.CreateTree(position, variant);
            }

            go.transform.rotation = Quaternion.Euler(0f, variant * 37f, 0f);
            var scale = 0.9f + (variant % 4) * 0.08f;
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        public static GameObject CreateGrass(Vector3 position, int variant)
        {
            var name = variant % 2 == 0 ? "grass" : "grass_large";
            var go = CreateKenneyObject(name, $"Grass_{variant}", position, false);
            if (go != null)
            {
                go.transform.rotation = Quaternion.Euler(0f, variant * 40f, 0f);
            }

            return go;
        }

        public static GameObject CreateRock(Vector3 position, int variant)
        {
            var name = RockNames[variant % RockNames.Length];
            var go = CreateKenneyObject(name, $"Rock_{name}", position + Vector3.up * 0.02f, addCollider: true);
            if (go == null)
            {
                return Freehold.Art.PropFactory.CreateRock(position, variant);
            }

            go.transform.rotation = Quaternion.Euler(0f, variant * 45f, 0f);
            go.transform.localScale = Vector3.one * (0.85f + (variant % 5) * 0.12f);
            return go;
        }

        public static GameObject CreatePond(Vector3 position)
        {
            // Kenney waterfalls as water feature + grass/stone shore from pack.
            var root = new GameObject("Pond");
            root.transform.position = position;

            var water = CreateKenneyObject("cliff_waterfall_rock", "WaterFeature", position, false);
            if (water != null)
            {
                water.transform.SetParent(root.transform, true);
                water.transform.localScale = new Vector3(1.4f, 0.6f, 1.4f);
            }

            for (var i = 0; i < 8; i++)
            {
                var a = i / 8f * Mathf.PI * 2f;
                var p = position + new Vector3(Mathf.Cos(a) * 3.2f, 0f, Mathf.Sin(a) * 3.2f);
                var stone = CreateRock(p, 20 + i);
                stone.transform.SetParent(root.transform, true);
                stone.transform.localScale *= 0.55f;
            }

            for (var i = 0; i < 10; i++)
            {
                var a = i / 10f * Mathf.PI * 2f + 0.2f;
                var p = position + new Vector3(Mathf.Cos(a) * 2.4f, 0f, Mathf.Sin(a) * 2.4f);
                var grass = CreateKenneyObject(i % 2 == 0 ? "grass" : "grass_large", $"ShoreGrass_{i}", p, false);
                if (grass != null)
                {
                    grass.transform.SetParent(root.transform, true);
                }
            }

            if (water == null)
            {
                Object.Destroy(root);
                return Freehold.Art.PropFactory.CreatePond(position);
            }

            return root;
        }

        public static ItemPickup CreatePickup(ItemType type, Vector3 position)
        {
            var def = ItemDefinition.Get(type);
            GameObject go = null;
            switch (type)
            {
                case ItemType.Axe:
                    go = CreateQuaterniusObject("Axe_small", $"Pickup_{def.DisplayName}", position, 0.55f);
                    break;
                case ItemType.Pickaxe:
                    go = CreateQuaterniusObject("Hammer_Double", $"Pickup_{def.DisplayName}", position, 0.5f);
                    break;
                case ItemType.Knife:
                    go = CreateQuaterniusObject("Dagger", $"Pickup_{def.DisplayName}", position, 0.7f);
                    break;
                case ItemType.PlasticBottle:
                    go = CreateQuaterniusObject("Potion1_Empty", $"Pickup_{def.DisplayName}", position, 0.45f);
                    break;
                case ItemType.DirtyWaterBottle:
                    go = CreateQuaterniusObject("Potion2_Filled", $"Pickup_{def.DisplayName}", position, 0.45f);
                    break;
                case ItemType.CleanWaterBottle:
                    go = CreateQuaterniusObject("Potion1_Filled", $"Pickup_{def.DisplayName}", position, 0.45f);
                    break;
                case ItemType.RawFood:
                    go = CreateQuaterniusObject("ChickenLeg", $"Pickup_{def.DisplayName}", position, 0.7f);
                    break;
                case ItemType.CookedFood:
                    go = CreateQuaterniusObject("Bag", $"Pickup_{def.DisplayName}", position, 0.35f);
                    break;
                case ItemType.Stick:
                    go = CreateKenneyObject("log_large", $"Pickup_{def.DisplayName}", position, false)
                         ?? CreateKenneyObject("log_stack", $"Pickup_{def.DisplayName}", position, false);
                    if (go != null) go.transform.localScale = Vector3.one * 0.35f;
                    break;
                case ItemType.Stone:
                    go = CreateKenneyObject("stone_smallA", $"Pickup_{def.DisplayName}", position, false)
                         ?? CreateKenneyObject("rock_smallA", $"Pickup_{def.DisplayName}", position, false);
                    if (go != null) go.transform.localScale = Vector3.one * 0.5f;
                    break;
            }

            if (go == null)
            {
                return Freehold.Art.PropFactory.CreatePickup(type, position);
            }

            var col = go.GetComponent<Collider>();
            if (col == null)
            {
                var sphere = go.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 0.4f;
                sphere.center = new Vector3(0f, 0.15f, 0f);
            }
            else
            {
                col.isTrigger = true;
            }

            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), type == ItemType.Axe || type == ItemType.Pickaxe ? 15f : 0f);
            var pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(type);
            go.AddComponent<PickupBob>();
            return pickup;
        }

        public static Campfire CreateCampfire(Vector3 position)
        {
            var root = new GameObject("Campfire");
            root.transform.position = position;

            var stones = CreateKenneyObject("campfire_stones", "Ring", position, true);
            var logs = CreateKenneyObject("campfire_logs", "Logs", position, false);
            if (stones != null) stones.transform.SetParent(root.transform, true);
            if (logs != null) logs.transform.SetParent(root.transform, true);

            // Flame visual kept as a pack-colored mesh child (same point-light values as before).
            var flame = new GameObject("Flame");
            flame.transform.SetParent(root.transform, false);
            flame.transform.localPosition = Vector3.up * 0.35f;
            var flameMesh = LowPolyMeshes.Flame();
            flame.AddComponent<MeshFilter>().sharedMesh = flameMesh;
            flame.AddComponent<MeshRenderer>().sharedMaterial = LowPolyMeshes.Mat(ArtPalette.FlameOuter);
            flame.AddComponent<FlameFlicker>();

            var lightGo = new GameObject("FireLight");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = Vector3.up * 0.8f;
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.55f, 0.2f);
            light.range = 8f;
            light.intensity = 2.2f;

            if (stones == null && logs == null)
            {
                Object.Destroy(root);
                return Freehold.Art.PropFactory.CreateCampfire(position);
            }

            var trigger = root.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1.4f;
            var campfire = root.AddComponent<Campfire>();
            campfire.ConfigureVisuals(flame.transform, light);
            root.tag = "Campfire";
            return campfire;
        }

        static GameObject CreateKenneyObject(string assetName, string goName, Vector3 position, bool addCollider)
        {
            if (!ObjMeshLoader.TryLoad(PackPaths.Kenney(assetName), out var mesh, out var color))
            {
                return null;
            }

            return BuildMeshObject(goName, mesh, color, position, addCollider);
        }

        static GameObject CreateQuaterniusObject(string assetName, string goName, Vector3 position, float scale)
        {
            if (!ObjMeshLoader.TryLoad(PackPaths.Quaternius(assetName), out var mesh, out var color))
            {
                return null;
            }

            var go = BuildMeshObject(goName, mesh, color, position, false);
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        static GameObject BuildMeshObject(string name, Mesh mesh, Color color, Vector3 position, bool addCollider)
        {
            var go = new GameObject(name);
            go.transform.position = position;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = PrimitiveFactory.ColorMaterial(color);
            if (addCollider)
            {
                var col = go.AddComponent<MeshCollider>();
                col.sharedMesh = mesh;
            }

            return go;
        }
    }
}
