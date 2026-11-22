using Freehold.Art.Pack;
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
    /// is playable without hand-authored FBX dependencies.
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

            var player = CreatePlayer(new Vector3(0f, 0.05f, 0f));
            var camera = CreateCamera(player.transform);

            var inventory = player.GetComponent<Inventory>();
            var needs = player.GetComponent<PlayerNeeds>();
            HudBuilder.Build(needs, inventory, out var prompt);

            var placer = player.AddComponent<CampfirePlacer>();
            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.Configure(inventory, needs, prompt, placer);

            var controller = player.GetComponent<PlayerController>();
            controller.BindCameraPivot(camera.Pivot);

            Debug.Log("Freehold survival slice ready (low-poly art pass). Explore, scavenge, build a campfire, cook & boil.");
        }

        static GameObject CreatePlayer(Vector3 position)
        {
            var player = new GameObject("Player");
            player.transform.position = position;

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.minMoveDistance = 0f;
            controller.skinWidth = 0.08f;

            player.AddComponent<PlayerNeeds>();
            player.AddComponent<Inventory>();
            player.AddComponent<PlayerController>();

            PackPlayerVisual.TryAttach(player.transform);
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
