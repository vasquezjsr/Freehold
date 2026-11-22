using Freehold.Art;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Freehold.Art.Pack
{
    /// <summary>
    /// Instantiates a Floreswa Low Poly Character Pack hero.
    /// Falls back to procedural humanoid only if the pack asset is unavailable.
    /// </summary>
    public static class PackPlayerVisual
    {
        const string ResourcesPath = "Characters/StoreHero";

        public static bool TryAttach(Transform playerRoot)
        {
            var source = LoadHeroSource();
            if (source == null)
            {
                Debug.LogWarning("Freehold: Floreswa character not available yet — using procedural humanoid fallback. After Unity imports Assets/Floreswa, Press Play again.");
                PlayerVisualBuilder.Build(playerRoot);
                return false;
            }

            var instance = Object.Instantiate(source, playerRoot);
            instance.name = "HeroVisual";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            FitHeroToController(instance, playerRoot);

            // Pack ships A-pose meshes with no locomotion clips — drive bones procedurally.
            // Disable any Animator so it cannot overwrite LateUpdate bone deltas.
            foreach (var animator in instance.GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = false;
                animator.runtimeAnimatorController = null;
            }

            var loco = playerRoot.gameObject.AddComponent<PackRigLocomotionVisual>();
            if (!loco.TryBind(instance.transform, playerRoot.GetComponent<CharacterController>()))
            {
                Object.Destroy(loco);
                var anim = instance.GetComponentInChildren<Animation>();
                var animator = instance.GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    animator.enabled = true;
                }

                var bridge = playerRoot.gameObject.AddComponent<PackPlayerAnimationBridge>();
                bridge.Configure(playerRoot.GetComponent<CharacterController>(), anim, animator);
            }

            return true;
        }

        static void FitHeroToController(GameObject instance, Transform playerRoot)
        {
            var controller = playerRoot != null ? playerRoot.GetComponent<CharacterController>() : null;
            if (controller == null)
            {
                return;
            }

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return;
            }

            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            if (bounds.size.y < 0.01f)
            {
                return;
            }

            var scale = controller.height / bounds.size.y;
            instance.transform.localScale = Vector3.one * scale;

            bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            // Plant feet on the CharacterController pivot (player root).
            var lift = playerRoot.position.y - bounds.min.y;
            instance.transform.localPosition = new Vector3(0f, lift, 0f);
        }

        static GameObject LoadHeroSource()
        {
            var fromResources = Resources.Load<GameObject>(ResourcesPath);
            if (fromResources != null)
            {
                return fromResources;
            }

#if UNITY_EDITOR
            var fromPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PackPaths.FloreswaHeroPrefab);
            if (fromPrefab != null)
            {
                return fromPrefab;
            }

            var fromFbx = AssetDatabase.LoadAssetAtPath<GameObject>(PackPaths.FloreswaHeroFbx);
            if (fromFbx != null)
            {
                return fromFbx;
            }
#endif
            return null;
        }
    }

    /// <summary>
    /// Procedural idle / walk / run / jump on Floreswa Blender Rigify bones.
    /// Rest pose is A-pose; deltas are applied on top of captured rest locals.
    /// </summary>
    public class PackRigLocomotionVisual : MonoBehaviour
    {
        CharacterController _controller;
        float _phase;

        Transform _hips;
        Transform _spine;
        Transform _chest;
        Transform _head;
        Transform _thighL;
        Transform _shinL;
        Transform _thighR;
        Transform _shinR;
        Transform _armL;
        Transform _foreL;
        Transform _armR;
        Transform _foreR;

        Quaternion _hipsRest, _spineRest, _chestRest, _headRest;
        Quaternion _thighLRest, _shinLRest, _thighRRest, _shinRRest;
        Quaternion _armLRest, _foreLRest, _armRRest, _foreRRest;

        public bool TryBind(Transform visualRoot, CharacterController controller)
        {
            _controller = controller;
            if (visualRoot == null)
            {
                return false;
            }

            _hips = FindBone(visualRoot, "spine");
            _spine = FindBone(visualRoot, "spine.001") ?? FindBone(visualRoot, "spine.002");
            _chest = FindBone(visualRoot, "spine.003") ?? FindBone(visualRoot, "spine.002");
            _head = FindBone(visualRoot, "spine.005") ?? FindBone(visualRoot, "spine.006") ?? FindBone(visualRoot, "spine.004");
            _thighL = FindBone(visualRoot, "thigh.L");
            _shinL = FindBone(visualRoot, "shin.L");
            _thighR = FindBone(visualRoot, "thigh.R");
            _shinR = FindBone(visualRoot, "shin.R");
            _armL = FindBone(visualRoot, "upper_arm.L");
            _foreL = FindBone(visualRoot, "forearm.L");
            _armR = FindBone(visualRoot, "upper_arm.R");
            _foreR = FindBone(visualRoot, "forearm.R");

            if (_thighL == null || _thighR == null || _armL == null || _armR == null)
            {
                Debug.LogWarning("Freehold: Floreswa rig bones not found — locomotion visual skipped.");
                return false;
            }

            CaptureRest(_hips, out _hipsRest);
            CaptureRest(_spine, out _spineRest);
            CaptureRest(_chest, out _chestRest);
            CaptureRest(_head, out _headRest);
            CaptureRest(_thighL, out _thighLRest);
            CaptureRest(_shinL, out _shinLRest);
            CaptureRest(_thighR, out _thighRRest);
            CaptureRest(_shinR, out _shinRRest);
            CaptureRest(_armL, out _armLRest);
            CaptureRest(_foreL, out _foreLRest);
            CaptureRest(_armR, out _armRRest);
            CaptureRest(_foreR, out _foreRRest);
            return true;
        }

        static void CaptureRest(Transform bone, out Quaternion rest)
        {
            rest = bone != null ? bone.localRotation : Quaternion.identity;
        }

        static Transform FindBone(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var found = FindBone(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        void LateUpdate()
        {
            if (_controller == null || _thighL == null)
            {
                return;
            }

            var horizontal = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            var speed = horizontal.magnitude;
            var grounded = _controller.isGrounded;
            var sprinting = speed > 5.5f;
            var moving = speed > 0.2f;

            var stepRate = sprinting ? 9.5f : moving ? 7.2f : 1.5f;
            _phase += Time.deltaTime * stepRate;

            if (!grounded)
            {
                ApplyJump();
                return;
            }

            if (!moving)
            {
                ApplyIdle();
                return;
            }

            ApplyWalkRun(sprinting);
        }

        void ApplyIdle()
        {
            var breathe = Mathf.Sin(Time.time * 1.7f) * 2.5f;
            Set(_hips, _hipsRest, Quaternion.identity);
            Set(_spine, _spineRest, Quaternion.Euler(breathe * 0.35f, 0f, 0f));
            Set(_chest, _chestRest, Quaternion.Euler(breathe * 0.2f, 0f, 0f));
            Set(_head, _headRest, Quaternion.Euler(-breathe * 0.25f, 0f, 0f));
            Set(_thighL, _thighLRest, Quaternion.Euler(3f, 0f, 0f));
            Set(_thighR, _thighRRest, Quaternion.Euler(-2f, 0f, 0f));
            Set(_shinL, _shinLRest, Quaternion.Euler(4f, 0f, 0f));
            Set(_shinR, _shinRRest, Quaternion.Euler(3f, 0f, 0f));
            // Soften A-pose: pull arms slightly toward body + down.
            Set(_armL, _armLRest, Quaternion.Euler(8f, 0f, 18f));
            Set(_armR, _armRRest, Quaternion.Euler(8f, 0f, -18f));
            Set(_foreL, _foreLRest, Quaternion.Euler(6f, 0f, 0f));
            Set(_foreR, _foreRRest, Quaternion.Euler(6f, 0f, 0f));
        }

        void ApplyWalkRun(bool sprinting)
        {
            var amp = sprinting ? 36f : 26f;
            var swing = Mathf.Sin(_phase) * amp;
            var swingOpp = -swing;
            var shinBend = Mathf.Max(0f, -Mathf.Sin(_phase)) * (sprinting ? 48f : 34f);
            var shinBendOpp = Mathf.Max(0f, Mathf.Sin(_phase)) * (sprinting ? 48f : 34f);
            var lean = sprinting ? 10f : 4f;

            Set(_hips, _hipsRest, Quaternion.Euler(lean * 0.4f, 0f, Mathf.Sin(_phase) * 2f));
            Set(_spine, _spineRest, Quaternion.Euler(lean * 0.5f, Mathf.Sin(_phase) * 3f, 0f));
            Set(_chest, _chestRest, Quaternion.Euler(lean * 0.35f, Mathf.Sin(_phase) * 2f, 0f));
            Set(_head, _headRest, Quaternion.Euler(sprinting ? -4f : 0f, 0f, 0f));

            Set(_thighL, _thighLRest, Quaternion.Euler(swing, 0f, 0f));
            Set(_thighR, _thighRRest, Quaternion.Euler(swingOpp, 0f, 0f));
            Set(_shinL, _shinLRest, Quaternion.Euler(shinBend, 0f, 0f));
            Set(_shinR, _shinRRest, Quaternion.Euler(shinBendOpp, 0f, 0f));

            Set(_armL, _armLRest, Quaternion.Euler(swingOpp * 0.75f, 0f, 12f));
            Set(_armR, _armRRest, Quaternion.Euler(swing * 0.75f, 0f, -12f));
            Set(_foreL, _foreLRest, Quaternion.Euler(12f + Mathf.Abs(swingOpp) * 0.15f, 0f, 0f));
            Set(_foreR, _foreRRest, Quaternion.Euler(12f + Mathf.Abs(swing) * 0.15f, 0f, 0f));
        }

        void ApplyJump()
        {
            Set(_hips, _hipsRest, Quaternion.identity);
            Set(_spine, _spineRest, Quaternion.Euler(-6f, 0f, 0f));
            Set(_chest, _chestRest, Quaternion.Euler(-4f, 0f, 0f));
            Set(_head, _headRest, Quaternion.Euler(6f, 0f, 0f));
            Set(_thighL, _thighLRest, Quaternion.Euler(-16f, 0f, 6f));
            Set(_thighR, _thighRRest, Quaternion.Euler(-10f, 0f, -6f));
            Set(_shinL, _shinLRest, Quaternion.Euler(32f, 0f, 0f));
            Set(_shinR, _shinRRest, Quaternion.Euler(26f, 0f, 0f));
            Set(_armL, _armLRest, Quaternion.Euler(-35f, 0f, 22f));
            Set(_armR, _armRRest, Quaternion.Euler(-30f, 0f, -22f));
            Set(_foreL, _foreLRest, Quaternion.Euler(10f, 0f, 0f));
            Set(_foreR, _foreRRest, Quaternion.Euler(10f, 0f, 0f));
        }

        static void Set(Transform bone, Quaternion rest, Quaternion delta)
        {
            if (bone == null)
            {
                return;
            }

            bone.localRotation = rest * delta;
        }
    }

    /// <summary>Fallback: drives pack Animation/Animator from CharacterController speed when clips exist.</summary>
    public class PackPlayerAnimationBridge : MonoBehaviour
    {
        CharacterController _controller;
        Animation _legacy;
        Animator _animator;
        string _current;

        public void Configure(CharacterController controller, Animation legacy, Animator animator)
        {
            _controller = controller;
            _legacy = legacy;
            _animator = animator;
            PlayPreferred("idle", "Idle", "Idle_Loop");
        }

        void Update()
        {
            if (_controller == null)
            {
                return;
            }

            var horizontal = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            var grounded = _controller.isGrounded;

            if (!grounded)
            {
                PlayPreferred("jump", "Jump", "jump");
            }
            else if (horizontal > 5.5f)
            {
                PlayPreferred("run", "Run", "run");
            }
            else if (horizontal > 0.2f)
            {
                PlayPreferred("walk", "Walk", "walk");
            }
            else
            {
                PlayPreferred("idle", "Idle", "idle");
            }
        }

        void PlayPreferred(params string[] names)
        {
            if (_legacy != null)
            {
                foreach (var n in names)
                {
                    if (_legacy.GetClip(n) != null)
                    {
                        if (_current == n)
                        {
                            return;
                        }

                        _legacy.CrossFade(n, 0.1f);
                        _current = n;
                        return;
                    }
                }

                foreach (AnimationState state in _legacy)
                {
                    foreach (var n in names)
                    {
                        if (state.name.ToLowerInvariant().Contains(n.ToLowerInvariant()))
                        {
                            if (_current == state.name) return;
                            _legacy.CrossFade(state.name, 0.1f);
                            _current = state.name;
                            return;
                        }
                    }
                }
            }

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                // Parameter-driven controllers can be wired later without changing gameplay.
            }
        }
    }
}
