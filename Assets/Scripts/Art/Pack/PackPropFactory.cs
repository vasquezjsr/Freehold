using Freehold.Art;
using Freehold.Crafting;
using Freehold.Items;
using Freehold.World;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Freehold.Art.Pack
{
    /// <summary>
    /// Spawns StarterArea dressing from the purchased BigRookGames nature pack.
    /// Loot/campfire keep Quaternius + Kenney meshes where the store pack has no equivalents.
    /// </summary>
    public static class PackPropFactory
    {
        static readonly string[] TreePrefabs =
        {
            "tree_common_01", "tree_common_02", "tree_common_03",
            "tree_common_01", "tree_common_02", "tree_common_03"
        };

        static readonly string[] BushPrefabs =
        {
            "bush_01", "bush_02", "bush_03", "bush_04", "bush_05", "bush_border"
        };

        static readonly string[] GrassPrefabs =
        {
            "bush_01", "bush_02", "bush_05"
        };

        public static GameObject CreateTree(Vector3 position, int variant)
        {
            var name = TreePrefabs[variant % TreePrefabs.Length];
            var go = InstantiateNaturePrefab(name, $"Tree_{name}", position, addCollider: true);
            if (go == null)
            {
                return Freehold.Art.PropFactory.CreateTree(position, variant);
            }

            go.transform.rotation = Quaternion.Euler(0f, variant * 37f, 0f);
            var scale = 0.85f + (variant % 4) * 0.1f;
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        public static GameObject CreateGrass(Vector3 position, int variant)
        {
            var name = GrassPrefabs[variant % GrassPrefabs.Length];
            var go = InstantiateNaturePrefab(name, $"Grass_{variant}", position, false);
            if (go == null)
            {
                return null;
            }

            go.transform.rotation = Quaternion.Euler(0f, variant * 40f, 0f);
            go.transform.localScale = Vector3.one * (0.35f + (variant % 3) * 0.08f);
            return go;
        }

        public static GameObject CreateRock(Vector3 position, int variant)
        {
            // Nature pack has no rock meshes — use denser bushes / borders as ground dressing.
            var name = BushPrefabs[variant % BushPrefabs.Length];
            var go = InstantiateNaturePrefab(name, $"RockDressing_{name}", position, addCollider: true);
            if (go == null)
            {
                return Freehold.Art.PropFactory.CreateRock(position, variant);
            }

            go.transform.rotation = Quaternion.Euler(0f, variant * 45f, 0f);
            go.transform.localScale = Vector3.one * (0.55f + (variant % 5) * 0.08f);
            return go;
        }

        public static GameObject CreatePond(Vector3 position)
        {
            var root = new GameObject("Pond");
            root.transform.position = position;

            var water = PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Cylinder,
                position,
                new Vector3(5.5f, 0.08f, 5.5f),
                new Color(0.25f, 0.45f, 0.55f, 0.85f),
                "Water",
                false);
            water.transform.SetParent(root.transform, true);

            for (var i = 0; i < 10; i++)
            {
                var a = i / 10f * Mathf.PI * 2f;
                var p = position + new Vector3(Mathf.Cos(a) * 3.4f, 0f, Mathf.Sin(a) * 3.4f);
                var bush = CreateRock(p, 20 + i);
                if (bush != null)
                {
                    bush.transform.SetParent(root.transform, true);
                    bush.transform.localScale *= 0.7f;
                }
            }

            for (var i = 0; i < 12; i++)
            {
                var a = i / 12f * Mathf.PI * 2f + 0.2f;
                var p = position + new Vector3(Mathf.Cos(a) * 2.5f, 0f, Mathf.Sin(a) * 2.5f);
                var grass = CreateGrass(p, 40 + i);
                if (grass != null)
                {
                    grass.transform.SetParent(root.transform, true);
                }
            }

            return root;
        }

        public static bool TryApplyGroundMaterial(GameObject ground)
        {
#if UNITY_EDITOR
            var mat = AssetDatabase.LoadAssetAtPath<Material>(PackPaths.NatureMaterial("grass_tile_01"));
            if (mat == null)
            {
                mat = AssetDatabase.LoadAssetAtPath<Material>(PackPaths.NatureMaterial("grass_tile_02"));
            }

            if (mat == null || ground == null)
            {
                return false;
            }

            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return false;
            }

            renderer.sharedMaterial = mat;
            return true;
#else
            return false;
#endif
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

        static GameObject InstantiateNaturePrefab(string prefabName, string goName, Vector3 position, bool addCollider)
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PackPaths.NaturePrefab(prefabName));
            if (prefab == null)
            {
                return null;
            }

            var go = Object.Instantiate(prefab);
            go.name = goName;
            go.transform.position = position;

            if (addCollider && go.GetComponentInChildren<Collider>() == null)
            {
                var filter = go.GetComponentInChildren<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    var col = go.AddComponent<MeshCollider>();
                    col.sharedMesh = filter.sharedMesh;
                }
                else
                {
                    var capsule = go.AddComponent<CapsuleCollider>();
                    capsule.height = 2f;
                    capsule.radius = 0.4f;
                    capsule.center = new Vector3(0f, 1f, 0f);
                }
            }

            return go;
#else
            return null;
#endif
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
