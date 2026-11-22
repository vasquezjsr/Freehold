using Freehold.Crafting;
using Freehold.Items;
using Freehold.Needs;
using Freehold.Player;
using Freehold.UI;
using UnityEngine;

namespace Freehold.World
{
    /// <summary>
    /// Builds the first survival scavenging slice at runtime so the project
    /// is playable without hand-authored meshes or complex scene wiring.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        static bool _booted;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (_booted || Object.FindFirstObjectByType<GameBootstrap>() != null)
            {
                return;
            }

            var host = new GameObject("GameBootstrap");
            host.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            if (_booted)
            {
                Destroy(gameObject);
                return;
            }

            _booted = true;
            DontDestroyOnLoad(gameObject);
            Boot();
        }

        void Boot()
        {
            Random.InitState(42);
            StarterAreaBuilder.Build();

            var player = CreatePlayer(new Vector3(0f, 1f, 0f));
            var camera = CreateCamera(player.transform);

            var inventory = player.GetComponent<Inventory>();
            var needs = player.GetComponent<PlayerNeeds>();
            HudBuilder.Build(needs, inventory, out var prompt);

            var placer = player.AddComponent<CampfirePlacer>();
            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.Configure(inventory, needs, prompt, placer);

            var controller = player.GetComponent<PlayerController>();
            controller.BindCameraPivot(camera.Pivot);

            Debug.Log("Freehold survival slice ready. Explore, scavenge, build a campfire, cook & boil.");
        }

        static GameObject CreatePlayer(Vector3 position)
        {
            var player = PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Capsule,
                position,
                new Vector3(0.9f, 1f, 0.9f),
                new Color(0.2f, 0.45f, 0.55f),
                "Player",
                false);

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.4f;
            controller.center = Vector3.up * 0.1f;
            controller.minMoveDistance = 0f;

            player.AddComponent<PlayerNeeds>();
            player.AddComponent<Inventory>();
            player.AddComponent<PlayerController>();

            // Simple head marker for silhouette
            PrimitiveFactory.CreateColoredPrimitive(
                PrimitiveType.Sphere,
                position + Vector3.up * 0.85f,
                Vector3.one * 0.45f,
                new Color(0.9f, 0.75f, 0.6f),
                "Head",
                false).transform.SetParent(player.transform, true);

            return player;
        }

        static ThirdPersonCamera CreateCamera(Transform target)
        {
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            cam.fieldOfView = 60f;
            camGo.AddComponent<AudioListener>();

            var follow = camGo.AddComponent<ThirdPersonCamera>();
            follow.Configure(target);
            camGo.transform.position = target.position + new Vector3(0f, 2.1f, -5.5f);
            return follow;
        }
    }
}
