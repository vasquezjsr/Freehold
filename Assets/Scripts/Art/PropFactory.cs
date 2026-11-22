using Freehold.Crafting;
using Freehold.Items;
using Freehold.World;
using UnityEngine;

namespace Freehold.Art
{
    /// <summary>Spawns detailed low-poly environment props and loot (Built-in RP).</summary>
    public static class PropFactory
    {
        public static GameObject CreateTree(Vector3 position, int variant)
        {
            var root = new GameObject($"Tree_{variant}");
            root.transform.position = position;

            var trunkMesh = LowPolyMeshes.TreeTrunk(1.4f + (variant % 3) * 0.15f);
            var trunk = MeshBuilder.CreateMeshObject("Trunk", trunkMesh, LowPolyMeshes.Mat(ArtPalette.Bark), position, false);
            trunk.transform.SetParent(root.transform, true);
            trunk.transform.rotation = Quaternion.Euler(0f, variant * 37f, 0f);

            var canopyY = 1.45f + (variant % 3) * 0.12f;
            var canopy = MeshBuilder.CreateMeshObject(
                "Canopy",
                LowPolyMeshes.TreeCanopy(variant),
                LowPolyMeshes.Mat(variant % 3 == 0 ? ArtPalette.LeafA : variant % 3 == 1 ? ArtPalette.LeafB : ArtPalette.LeafC),
                position + Vector3.up * canopyY,
                false);
            canopy.transform.SetParent(root.transform, true);
            canopy.transform.localScale = Vector3.one * (0.95f + (variant % 4) * 0.08f);
            canopy.transform.rotation = Quaternion.Euler(0f, variant * 21f, 0f);

            // Trunk-only capsule matching procedural trunk radius/height.
            var trunkHeight = 1.5f + (variant % 3) * 0.15f;
            var trunkRadius = 0.22f;
            var colGo = new GameObject("UprightCollider");
            colGo.transform.SetParent(root.transform, false);
            colGo.transform.localPosition = Vector3.zero;
            var col = colGo.AddComponent<CapsuleCollider>();
            col.direction = 1;
            col.height = trunkHeight;
            col.radius = trunkRadius;
            col.center = new Vector3(0f, trunkHeight * 0.5f, 0f);

            root.AddComponent<ChoppableTree>();
            return root;
        }

        public static GameObject CreateRock(Vector3 position, int variant)
        {
            var mesh = LowPolyMeshes.Rock(variant);
            var go = MeshBuilder.CreateMeshObject($"Rock_{variant}", mesh, LowPolyMeshes.Mat(ArtPalette.RockA), position + Vector3.up * 0.05f, true);
            go.transform.rotation = Quaternion.Euler(0f, variant * 45f, (variant % 5) * 4f);
            var scale = 0.85f + (variant % 5) * 0.18f;
            go.transform.localScale = Vector3.one * scale;
            return go;
        }

        public static GameObject CreatePond(Vector3 position)
        {
            var root = new GameObject("Pond");
            root.transform.position = position;

            var water = MeshBuilder.CreateMeshObject("Water", LowPolyMeshes.PondWater(), LowPolyMeshes.Mat(ArtPalette.Pond), position, false);
            water.transform.SetParent(root.transform, true);

            var bank = MeshBuilder.CreateMeshObject("Bank", LowPolyMeshes.PondBank(), LowPolyMeshes.Mat(ArtPalette.PondEdge), position, false);
            bank.transform.SetParent(root.transform, true);

            for (var i = 0; i < 6; i++)
            {
                var a = i / 6f * Mathf.PI * 2f;
                var p = position + new Vector3(Mathf.Cos(a) * 3.4f, 0.02f, Mathf.Sin(a) * 3.4f);
                var stone = CreateRock(p, 10 + i);
                stone.transform.SetParent(root.transform, true);
                stone.transform.localScale *= 0.45f;
            }

            return root;
        }

        public static ItemPickup CreatePickup(ItemType type, Vector3 position)
        {
            var def = ItemDefinition.Get(type);
            var mesh = LowPolyMeshes.ForItem(type);
            var go = MeshBuilder.CreateMeshObject($"Pickup_{def.DisplayName}", mesh, LowPolyMeshes.Mat(def.Color), position, false);
            var col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = 0.35f;
            col.center = new Vector3(0f, 0.15f, 0f);

            var yaw = Random.Range(0f, 360f);
            if (type == ItemType.Axe || type == ItemType.Pickaxe || type == ItemType.Knife)
            {
                go.transform.rotation = Quaternion.Euler(90f, yaw, 0f);
            }
            else if (type == ItemType.Stick || type == ItemType.Wood)
            {
                go.transform.rotation = Quaternion.Euler(0f, yaw, 12f);
            }
            else
            {
                go.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            Freehold.Art.Pack.PackPropFactory.SnapToGround(go, hover: 0.01f);
            var pickup = go.AddComponent<ItemPickup>();
            pickup.Configure(type);
            return pickup;
        }

        public static Campfire CreateCampfire(Vector3 position)
        {
            var root = new GameObject("Campfire");
            root.transform.position = position;

            var ring = MeshBuilder.CreateMeshObject("Ring", LowPolyMeshes.CampfireRing(), LowPolyMeshes.Mat(ArtPalette.Ash), position, true);
            ring.transform.SetParent(root.transform, true);

            var logMesh = LowPolyMeshes.CampfireLog();
            for (var i = 0; i < 3; i++)
            {
                var angle = i * 60f;
                var log = MeshBuilder.CreateMeshObject($"Log_{i}", logMesh, LowPolyMeshes.Mat(ArtPalette.Wood), position + Vector3.up * 0.12f, false);
                log.transform.SetParent(root.transform, true);
                log.transform.rotation = Quaternion.Euler(0f, angle, 18f);
            }

            var flameRoot = new GameObject("Flame");
            flameRoot.transform.SetParent(root.transform, false);
            flameRoot.transform.position = position + Vector3.up * 0.2f;
            var outer = MeshBuilder.CreateMeshObject("FlameOuter", LowPolyMeshes.Flame(), LowPolyMeshes.Mat(ArtPalette.FlameOuter), flameRoot.transform.position, false);
            outer.transform.SetParent(flameRoot.transform, true);
            var coreMesh = LowPolyMeshes.Box("FlameCore", new Vector3(0.12f, 0.28f, 0.12f), ArtPalette.FlameCore);
            var core = MeshBuilder.CreateMeshObject("FlameCore", coreMesh, LowPolyMeshes.Mat(ArtPalette.FlameCore), flameRoot.transform.position + Vector3.up * 0.12f, false);
            core.transform.SetParent(flameRoot.transform, true);
            flameRoot.AddComponent<FlameFlicker>();

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
            campfire.ConfigureVisuals(flameRoot.transform, light);
            root.tag = "Campfire";
            return campfire;
        }
    }

    public class FlameFlicker : MonoBehaviour
    {
        Vector3 _baseScale;

        void Start()
        {
            _baseScale = transform.localScale;
        }

        void Update()
        {
            var pulse = 1f + Mathf.Sin(Time.time * 9f) * 0.08f + Mathf.Sin(Time.time * 17f) * 0.04f;
            transform.localScale = _baseScale * pulse;
            transform.Rotate(Vector3.up, 40f * Time.deltaTime, Space.World);
        }
    }
}
