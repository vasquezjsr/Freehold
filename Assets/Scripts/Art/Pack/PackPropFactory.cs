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

        static readonly string[] BigBushPrefabs =
        {
            "bush_03", "bush_04", "bush_border"
        };

        static readonly string[] GrassPrefabs =
        {
            "bush_01", "bush_02", "bush_05"
        };

        static Bounds CombinedWorldBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(go.transform.position, Vector3.one);
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        /// <summary>
        /// BigRook nature FBXs are Z-up; pack prefabs bake ~-90° X so they stand in Unity.
        /// Spawn must multiply yaw onto that upright base — never replace with yaw-only.
        /// </summary>
        static void ApplyYawPreservingUpright(Transform t, float yawDegrees)
        {
            var prefabRot = t.rotation;
            var size = CombinedWorldBounds(t.gameObject).size;

            // Prefab already upright (tall world AABB) — keep its tilt, add yaw.
            if (size.y >= Mathf.Max(size.x, size.z) * 0.65f)
            {
                t.rotation = Quaternion.Euler(0f, yawDegrees, 0f) * prefabRot;
                return;
            }

            // Still flat: force classic Z-up → Y-up, then yaw.
            t.rotation = Quaternion.Euler(-90f, yawDegrees, 0f);
            size = CombinedWorldBounds(t.gameObject).size;
            if (size.y < Mathf.Max(size.x, size.z) * 0.65f)
            {
                t.rotation = Quaternion.Euler(0f, yawDegrees, -90f);
            }
        }

        static void ClearColliders(GameObject go)
        {
            var cols = go.GetComponentsInChildren<Collider>(true);
            for (var i = 0; i < cols.Length; i++)
            {
                Object.DestroyImmediate(cols[i]);
            }
        }

        /// <summary>
        /// World-upright capsule on an unrotated child. Pack prefabs keep ~-90° X on the root;
        /// a CapsuleCollider on that root with direction=Y becomes a sideways invisible wall.
        /// </summary>
        static void AddUprightCapsuleCollider(GameObject go, float radiusScale, float maxRadius)
        {
            ClearColliders(go);

            var bounds = CombinedWorldBounds(go);
            var height = Mathf.Max(0.45f, bounds.size.y);
            var radius = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f * radiusScale, 0.1f, maxRadius);

            var colGo = new GameObject("UprightCollider");
            colGo.transform.SetParent(go.transform, false);
            // World-aligned so capsule Y == world up regardless of FBX upright tilt on the parent.
            colGo.transform.position = new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
            colGo.transform.rotation = Quaternion.identity;
            // Cancel parent lossy scale so height/radius stay in world units.
            var lossy = colGo.transform.lossyScale;
            colGo.transform.localScale = new Vector3(
                1f / Mathf.Max(0.001f, Mathf.Abs(lossy.x)),
                1f / Mathf.Max(0.001f, Mathf.Abs(lossy.y)),
                1f / Mathf.Max(0.001f, Mathf.Abs(lossy.z)));

            var col = colGo.AddComponent<CapsuleCollider>();
            col.direction = 1; // Y
            col.height = height;
            col.radius = radius;
            col.center = new Vector3(0f, height * 0.5f, 0f);
        }

        public static GameObject CreateTree(Vector3 position, int variant)
        {
            var name = TreePrefabs[variant % TreePrefabs.Length];
            var go = InstantiateNaturePrefab(name, $"Tree_{name}", position, addCollider: false);
            if (go == null)
            {
                return Freehold.Art.PropFactory.CreateTree(position, variant);
            }

            ApplyYawPreservingUpright(go.transform, variant * 37f);
            var scale = 0.85f + (variant % 4) * 0.1f;
            go.transform.localScale = Vector3.one * scale;
            // Narrow trunk capsule — canopy must not form a wall.
            AddUprightCapsuleCollider(go, radiusScale: 0.18f, maxRadius: 0.45f);
            return go;
        }

        /// <summary>Small bushes / grass dressing — walk-through (no collider).</summary>
        public static GameObject CreateGrass(Vector3 position, int variant)
        {
            var name = GrassPrefabs[variant % GrassPrefabs.Length];
            var go = InstantiateNaturePrefab(name, $"Grass_{variant}", position, false);
            if (go == null)
            {
                return null;
            }

            ApplyYawPreservingUpright(go.transform, variant * 40f);
            go.transform.localScale = Vector3.one * (0.35f + (variant % 3) * 0.08f);
            ClearColliders(go);
            return go;
        }

        /// <summary>
        /// Big bushes (nature pack has no rocks) — solid upright capsule.
        /// </summary>
        public static GameObject CreateRock(Vector3 position, int variant)
        {
            var name = BigBushPrefabs[variant % BigBushPrefabs.Length];
            var go = InstantiateNaturePrefab(name, $"BigBush_{name}", position, addCollider: false);
            if (go == null)
            {
                return Freehold.Art.PropFactory.CreateRock(position, variant);
            }

            ApplyYawPreservingUpright(go.transform, variant * 45f);
            go.transform.localScale = Vector3.one * (0.85f + (variant % 5) * 0.12f);
            AddUprightCapsuleCollider(go, radiusScale: 0.42f, maxRadius: 0.85f);
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
            if (ground == null)
            {
                return false;
            }

            var renderer = ground.GetComponent<MeshRenderer>();
            if (renderer == null)
            {
                return false;
            }

#if UNITY_EDITOR
            // Prefer textured grass mats; grass_tile_01 often ships with a missing MainTex (white look).
            Material mat = null;
            foreach (var matName in new[] { "grass_tile_01d", "grass_tile_02", "grass_tile_01", "bush_tile" })
            {
                var candidate = AssetDatabase.LoadAssetAtPath<Material>(PackPaths.NatureMaterial(matName));
                if (candidate == null)
                {
                    continue;
                }

                if (candidate.HasProperty("_MainTex") && candidate.mainTexture != null)
                {
                    mat = candidate;
                    break;
                }

                if (mat == null)
                {
                    mat = candidate;
                }
            }

            if (mat != null && mat.HasProperty("_MainTex") && mat.mainTexture != null)
            {
                renderer.sharedMaterial = mat;
                return true;
            }

            // Runtime-tinted copy so we don't mutate the pack asset, and ground isn't pink/white.
            if (mat != null)
            {
                var copy = new Material(mat);
                copy.color = ArtPalette.Grass;
                if (copy.HasProperty("_MainTex"))
                {
                    var tex = AssetDatabase.LoadAssetAtPath<Texture>(PackPaths.NatureTexture("grass_tile_01d.tga"))
                              ?? AssetDatabase.LoadAssetAtPath<Texture>(PackPaths.NatureTexture("Grass_Plane_01c.tif"));
                    if (tex != null)
                    {
                        copy.mainTexture = tex;
                        copy.mainTextureScale = new Vector2(8f, 8f);
                    }
                }

                renderer.sharedMaterial = copy;
                return true;
            }
#endif
            renderer.sharedMaterial = PrimitiveFactory.ColorMaterial(ArtPalette.Grass);
            return true;
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
            SnapToGround(go);
            var pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(type);
            go.AddComponent<PickupBob>();
            return pickup;
        }

        /// <summary>Raycast down and plant mesh bottom on the hit surface (or y=0 ground top).</summary>
        public static void SnapToGround(GameObject go, float hover = 0.02f)
        {
            if (go == null)
            {
                return;
            }

            var pos = go.transform.position;
            var rayOrigin = pos + Vector3.up * 8f;
            var groundY = 0f;
            var hits = Physics.RaycastAll(rayOrigin, Vector3.down, 24f, ~0, QueryTriggerInteraction.Ignore);
            var bestDist = float.MaxValue;
            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                // Skip vegetation capsules / self — only plant on the walkable ground plane.
                if (hit.collider == null || hit.collider.transform.IsChildOf(go.transform))
                {
                    continue;
                }

                if (hit.collider.gameObject.name == "UprightCollider")
                {
                    continue;
                }

                if (hit.normal.y < 0.5f)
                {
                    continue;
                }

                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    groundY = hit.point.y;
                }
            }

            go.transform.position = new Vector3(pos.x, groundY + hover, pos.z);

            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            var lift = (groundY + hover) - bounds.min.y;
            go.transform.position += Vector3.up * lift;
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
            // Keep prefab localRotation (Z-up correction). Callers apply yaw via ApplyYawPreservingUpright.

            if (addCollider && go.GetComponentInChildren<Collider>() == null)
            {
                AddUprightCapsuleCollider(go, radiusScale: 0.25f, maxRadius: 0.5f);
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
