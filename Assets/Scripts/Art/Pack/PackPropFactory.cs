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

        // Large volume bushes — solid collision.
        static readonly string[] BigBushPrefabs =
        {
            "bush_03", "bush_04", "bush_05"
        };

        // Small bushes / grass only — walk-through (no collider).
        static readonly string[] GrassPrefabs =
        {
            "bush_01", "bush_02"
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
        /// BigRook nature FBXs are Z-up; pack prefabs already bake the upright correction
        /// (root and/or children ≈ -90° X). Only multiply world yaw — never force another -90°.
        /// Wide bushes fail a "tall AABB" test and used to get a second -90°, lying on their side.
        /// </summary>
        static void ApplyYawPreservingUpright(Transform t, float yawDegrees)
        {
            t.rotation = Quaternion.Euler(0f, yawDegrees, 0f) * t.rotation;
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
        static void AddUprightCapsuleCollider(GameObject go, float radius, float height, Vector3 worldBottomCenter)
        {
            ClearColliders(go);

            height = Mathf.Max(0.45f, height);
            radius = Mathf.Max(0.12f, radius);

            var colGo = new GameObject("UprightCollider");
            colGo.transform.SetParent(go.transform, false);
            colGo.transform.position = worldBottomCenter;
            colGo.transform.rotation = Quaternion.identity;
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

        static void AddBushCapsuleCollider(GameObject go, float radiusScale, float maxRadius)
        {
            var bounds = CombinedWorldBounds(go);
            var height = Mathf.Max(0.45f, bounds.size.y * 0.85f);
            var radius = Mathf.Clamp(Mathf.Max(bounds.size.x, bounds.size.z) * 0.5f * radiusScale, 0.2f, maxRadius);
            AddUprightCapsuleCollider(go, radius, height, new Vector3(bounds.center.x, bounds.min.y, bounds.center.z));
        }

        /// <summary>
        /// Trunk radius/height from the lowest bark band only (ignores canopy width).
        /// Radius is padded to match the visual bark so the player cannot walk into the mesh.
        /// </summary>
        static bool TryEstimateTrunk(GameObject go, out Vector3 bottomCenter, out float radius, out float height)
        {
            bottomCenter = go.transform.position;
            radius = 0.4f;
            height = 2.4f;

            var filters = go.GetComponentsInChildren<MeshFilter>();
            if (filters.Length == 0)
            {
                return false;
            }

            var full = CombinedWorldBounds(go);
            // Only the near-ground bark column — mid-height canopy verts inflate XZ and used to
            // force a tiny clamped capsule that left a hollow walkable trunk.
            var bandMaxY = full.min.y + Mathf.Min(1.25f, Mathf.Max(0.55f, full.size.y * 0.18f));
            var bandMinY = full.min.y;

            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            var samples = 0;
            double sumX = 0d, sumZ = 0d;

            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                var verts = mesh.vertices;
                var matrix = filters[i].transform.localToWorldMatrix;
                var step = Mathf.Max(1, verts.Length / 500);
                for (var v = 0; v < verts.Length; v += step)
                {
                    var world = matrix.MultiplyPoint3x4(verts[v]);
                    if (world.y < bandMinY - 0.05f || world.y > bandMaxY)
                    {
                        continue;
                    }

                    minX = Mathf.Min(minX, world.x);
                    maxX = Mathf.Max(maxX, world.x);
                    minZ = Mathf.Min(minZ, world.z);
                    maxZ = Mathf.Max(maxZ, world.z);
                    minY = Mathf.Min(minY, world.y);
                    maxY = Mathf.Max(maxY, world.y);
                    sumX += world.x;
                    sumZ += world.z;
                    samples++;
                }
            }

            if (samples < 8)
            {
                radius = Mathf.Clamp(Mathf.Max(full.size.x, full.size.z) * 0.16f, 0.32f, 1.1f);
                height = Mathf.Clamp(full.size.y * 0.55f, 1.8f, 5f);
                bottomCenter = new Vector3(full.center.x, full.min.y, full.center.z);
                return true;
            }

            var cx = (float)(sumX / samples);
            var cz = (float)(sumZ / samples);
            // Second pass: max horizontal distance from trunk axis (= visual bark radius).
            var maxRadial = 0f;
            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                var verts = mesh.vertices;
                var matrix = filters[i].transform.localToWorldMatrix;
                var step = Mathf.Max(1, verts.Length / 500);
                for (var v = 0; v < verts.Length; v += step)
                {
                    var world = matrix.MultiplyPoint3x4(verts[v]);
                    if (world.y < bandMinY - 0.05f || world.y > bandMaxY)
                    {
                        continue;
                    }

                    var dx = world.x - cx;
                    var dz = world.z - cz;
                    maxRadial = Mathf.Max(maxRadial, Mathf.Sqrt(dx * dx + dz * dz));
                }
            }

            var aabbR = Mathf.Max((maxX - minX) * 0.5f, (maxZ - minZ) * 0.5f);
            // Prefer radial fit; pad so collider meets the visible bark surface.
            radius = Mathf.Clamp(Mathf.Max(maxRadial, aabbR) * 1.08f, 0.32f, 1.45f);
            height = Mathf.Clamp(Mathf.Max(maxY - minY + 0.35f, full.size.y * 0.42f), 1.8f, 5.5f);
            bottomCenter = new Vector3(cx, minY, cz);
            return true;
        }

        static void AddTrunkCollider(GameObject go, out Vector3 bottom, out float radius)
        {
            Physics.SyncTransforms();
            TryEstimateTrunk(go, out bottom, out radius, out var height);
            AddUprightCapsuleCollider(go, radius, height, bottom);
        }

        public static GameObject CreateTree(Vector3 position, int variant)
        {
            var name = TreePrefabs[variant % TreePrefabs.Length];
            var visual = InstantiateNaturePrefab(name, $"TreeVisual_{name}", position, addCollider: false);
            if (visual == null)
            {
                return Freehold.Art.PropFactory.CreateTree(position, variant);
            }

            // World-aligned root so trunk capsule stays upright even if the FBX bake is tilted.
            var root = new GameObject($"Tree_{name}");
            root.transform.position = position;
            visual.transform.SetParent(root.transform, true);
            ApplyYawPreservingUpright(visual.transform, variant * 37f);
            var scale = 0.85f + (variant % 4) * 0.1f;
            visual.transform.localScale = Vector3.one * scale;
            SnapToGround(visual, hover: 0f);
            AddTrunkCollider(root, out var bottom, out var radius);
            var chop = root.AddComponent<ChoppableTree>();
            chop.ConfigureTrunk(bottom, radius);
            return root;
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
            go.transform.localScale = Vector3.one * (0.55f + (variant % 3) * 0.1f);
            ClearColliders(go);
            SnapToGround(go, hover: 0f);
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
            SnapToGround(go, hover: 0f);
            AddBushCapsuleCollider(go, radiusScale: 0.48f, maxRadius: 0.95f);
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
                case ItemType.Wood:
                    go = CreateKenneyObject("log_large", $"Pickup_{def.DisplayName}", position, false)
                         ?? CreateKenneyObject("log_stack", $"Pickup_{def.DisplayName}", position, false);
                    if (go != null) go.transform.localScale = Vector3.one * 0.55f;
                    break;
                case ItemType.Stone:
                    go = CreateKenneyObject("stone_smallA", $"Pickup_{def.DisplayName}", position, false)
                         ?? CreateKenneyObject("rock_smallA", $"Pickup_{def.DisplayName}", position, false);
                    if (go != null) go.transform.localScale = Vector3.one * 0.5f;
                    break;
                case ItemType.Backpack:
                    go = CreateQuaterniusObject("Bag", $"Pickup_{def.DisplayName}", position, 0.5f);
                    break;
            }

            if (go == null)
            {
                return Freehold.Art.PropFactory.CreatePickup(type, position);
            }

            go.transform.rotation = LootGroundRotation(type);
            // Plant after mesh, scale, and rotation so renderer/mesh bounds match the final pose.
            SnapToGround(go, hover: 0.01f);

            var col = go.GetComponent<Collider>();
            if (col == null)
            {
                var sphere = go.AddComponent<SphereCollider>();
                sphere.isTrigger = true;
                sphere.radius = 0.4f;
                var bounds = CombinedWorldBounds(go);
                sphere.center = go.transform.InverseTransformPoint(bounds.center);
            }
            else
            {
                col.isTrigger = true;
            }

            // Re-snap in case collider/bounds settled after first plant.
            SnapToGround(go, hover: 0.01f);

            var pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(type);
            // Ground loot stays static — no bob/spin.
            return pickup;
        }

        /// <summary>
        /// Quaternius tools are authored long-axis = local Y; lay them flat.
        /// Kenney logs/stones already sit with Y as thickness.
        /// </summary>
        static Quaternion LootGroundRotation(ItemType type)
        {
            var yaw = Random.Range(0f, 360f);
            switch (type)
            {
                case ItemType.Axe:
                case ItemType.Pickaxe:
                case ItemType.Knife:
                    return Quaternion.Euler(90f, yaw, 0f);
                case ItemType.Stick:
                case ItemType.Wood:
                    return Quaternion.Euler(0f, yaw, Random.Range(-6f, 6f));
                default:
                    return Quaternion.Euler(0f, yaw, 0f);
            }
        }

        /// <summary>
        /// Raycast to walkable ground, then lift so the mesh AABB bottom sits on that surface.
        /// Uses MeshFilter corners when present (stable right after OBJ assign); renderer bounds fallback.
        /// </summary>
        public static void SnapToGround(GameObject go, float hover = 0.02f)
        {
            if (go == null)
            {
                return;
            }

            Physics.SyncTransforms();

            var pos = go.transform.position;
            var groundY = ResolveGroundY(pos);

            if (!TryWorldMeshBottom(go, out var bottomY))
            {
                bottomY = CombinedWorldBounds(go).min.y;
            }

            var delta = (groundY + hover) - bottomY;
            if (Mathf.Abs(delta) > 0.0001f)
            {
                go.transform.position = pos + Vector3.up * delta;
            }
        }

        static float ResolveGroundY(Vector3 pos)
        {
            // Starter ground cube top is y=0; prefer an explicit Ground hit when present.
            var groundY = 0f;
            var rayOrigin = new Vector3(pos.x, Mathf.Max(pos.y, 0f) + 40f, pos.z);
            var hits = Physics.RaycastAll(rayOrigin, Vector3.down, 80f, ~0, QueryTriggerInteraction.Ignore);
            var bestDist = float.MaxValue;
            var found = false;

            for (var i = 0; i < hits.Length; i++)
            {
                var hit = hits[i];
                if (hit.collider == null)
                {
                    continue;
                }

                var hitGo = hit.collider.gameObject;
                if (hitGo.name == "UprightCollider" || hitGo.name.StartsWith("Pickup_", System.StringComparison.Ordinal))
                {
                    continue;
                }

                if (hit.normal.y < 0.5f)
                {
                    continue;
                }

                if (hitGo.name == "Ground")
                {
                    return hit.point.y;
                }

                if (hit.distance < bestDist)
                {
                    bestDist = hit.distance;
                    groundY = hit.point.y;
                    found = true;
                }
            }

            return found ? groundY : 0f;
        }

        static bool TryWorldMeshBottom(GameObject go, out float bottomY)
        {
            bottomY = float.MaxValue;
            var filters = go.GetComponentsInChildren<MeshFilter>();
            var any = false;
            for (var i = 0; i < filters.Length; i++)
            {
                var mesh = filters[i].sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                var b = mesh.bounds;
                var matrix = filters[i].transform.localToWorldMatrix;
                for (var cx = 0; cx < 2; cx++)
                for (var cy = 0; cy < 2; cy++)
                for (var cz = 0; cz < 2; cz++)
                {
                    var corner = new Vector3(
                        cx == 0 ? b.min.x : b.max.x,
                        cy == 0 ? b.min.y : b.max.y,
                        cz == 0 ? b.min.z : b.max.z);
                    var world = matrix.MultiplyPoint3x4(corner);
                    if (world.y < bottomY)
                    {
                        bottomY = world.y;
                    }
                }

                any = true;
            }

            return any;
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
                AddBushCapsuleCollider(go, radiusScale: 0.35f, maxRadius: 0.6f);
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
