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
    /// Rest pose is A-pose. Walk uses one gait phase: arms swing opposite legs in character space
    /// so Rigify local axes don't desync the contralateral gait.
    /// </summary>
    public class PackRigLocomotionVisual : MonoBehaviour
    {
        CharacterController _controller;
        Transform _visualRoot;
        float _phase;
        float _chopUntil;
        float _chopStart;

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

        public static void TriggerChop(GameObject player)
        {
            var loco = player != null ? player.GetComponent<PackRigLocomotionVisual>() : null;
            loco?.PlayChop();
        }

        public void PlayChop()
        {
            _chopStart = Time.time;
            _chopUntil = Time.time + 0.48f;
        }

        public bool TryBind(Transform visualRoot, CharacterController controller)
        {
            _controller = controller;
            _visualRoot = visualRoot;
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

            if (Time.time < _chopUntil)
            {
                ApplyChop();
                return;
            }

            var horizontal = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z);
            var speed = horizontal.magnitude;
            var grounded = _controller.isGrounded;
            var sprinting = speed > 5.5f;
            var moving = speed > 0.2f;

            var stepRate = sprinting ? 10.2f : moving ? 7.6f : 1.5f;
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

        void ApplyChop()
        {
            var u = Mathf.Clamp01((Time.time - _chopStart) / 0.48f);
            // Wind-up → strike → recover.
            float swing;
            if (u < 0.28f)
            {
                swing = Mathf.Lerp(20f, -25f, u / 0.28f);
            }
            else if (u < 0.55f)
            {
                swing = Mathf.Lerp(-25f, 70f, (u - 0.28f) / 0.27f);
            }
            else
            {
                swing = Mathf.Lerp(70f, 8f, (u - 0.55f) / 0.45f);
            }

            var rightAxis = CharacterRight();
            var upAxis = Vector3.up;
            SetLocal(_hips, _hipsRest, Quaternion.Euler(6f, 12f, 0f));
            SetLocal(_spine, _spineRest, Quaternion.Euler(8f, 18f, 0f));
            SetLocal(_chest, _chestRest, Quaternion.Euler(10f, 22f, 0f));
            SetLocal(_head, _headRest, Quaternion.Euler(0f, -10f, 0f));
            SwingBone(_thighL, _thighLRest, 8f, rightAxis);
            SwingBone(_thighR, _thighRRest, -6f, rightAxis);
            SwingBone(_shinL, _shinLRest, 10f, rightAxis);
            SwingBone(_shinR, _shinRRest, 8f, rightAxis);
            // Dominant right-hand axe swing (still adducted out of A-pose).
            var carryR = Quaternion.AngleAxis(-38f, CharacterForward()) * Quaternion.AngleAxis(25f, upAxis);
            SwingBone(_armR, _armRRest, -swing, rightAxis, carryR);
            SwingBone(_foreR, _foreRRest, 25f + Mathf.Abs(swing) * 0.35f, rightAxis);
            SwingBone(_armL, _armLRest, 12f, rightAxis, Quaternion.AngleAxis(40f, CharacterForward()));
            SwingBone(_foreL, _foreLRest, 20f, rightAxis);
        }

        void ApplyIdle()
        {
            // Rest is A-pose — use the same character-space SwingBone path as walk so
            // Rigify local Euler axes cannot leave arms stuck ~45° out.
            var breathe = Mathf.Sin(Time.time * 1.7f) * 2.2f;
            var weight = Mathf.Sin(Time.time * 0.85f);
            var rightAxis = CharacterRight();
            var fwdAxis = CharacterForward();

            SetLocal(_hips, _hipsRest, Quaternion.Euler(2f, weight * 2.5f, weight * 1.4f));
            SetLocal(_spine, _spineRest, Quaternion.Euler(breathe * 0.4f, 0f, weight * 0.6f));
            SetLocal(_chest, _chestRest, Quaternion.Euler(breathe * 0.25f, 0f, 0f));
            SetLocal(_head, _headRest, Quaternion.Euler(-breathe * 0.3f, weight * 1.5f, 0f));

            // Soft standing weight: slight knee bend, one hip a touch forward.
            SwingBone(_thighL, _thighLRest, 6f + weight * 1.5f, rightAxis);
            SwingBone(_thighR, _thighRRest, 4f - weight * 1.5f, rightAxis);
            SwingBone(_shinL, _shinLRest, 10f, rightAxis);
            SwingBone(_shinR, _shinRRest, 9f, rightAxis);

            // Arms down at sides: full A-pose adduction (~45°) + slight hang/elbow bend.
            const float armHang = 8f;
            const float adduct = 46f;
            var armCarryL = Quaternion.AngleAxis(adduct, fwdAxis);
            var armCarryR = Quaternion.AngleAxis(-adduct, fwdAxis);
            SwingBone(_armL, _armLRest, armHang + breathe * 0.15f, rightAxis, armCarryL);
            SwingBone(_armR, _armRRest, armHang + breathe * 0.15f, rightAxis, armCarryR);
            SwingBone(_foreL, _foreLRest, 14f, rightAxis);
            SwingBone(_foreR, _foreRRest, 14f, rightAxis);
        }

        void ApplyWalkRun(bool sprinting)
        {
            // One shared gait phase. Positive = left leg forward / right arm forward (natural contralateral).
            var legAmp = sprinting ? 42f : 32f;
            var armAmp = sprinting ? 40f : 30f;
            var gait = Mathf.Sin(_phase);
            var gaitCos = Mathf.Cos(_phase);
            var leftLeg = gait * legAmp;
            var rightLeg = -leftLeg;
            var leftArm = -gait * armAmp;   // opposite left leg
            var rightArm = gait * armAmp;   // opposite right leg
            var shinL = Mathf.Max(0f, -gait) * (sprinting ? 55f : 40f);
            var shinR = Mathf.Max(0f, gait) * (sprinting ? 55f : 40f);
            var lean = sprinting ? 12f : 5f;
            var sway = gait * 4.5f;
            var hipYaw = gait * 6f;
            var shoulderCounter = -gait * 7f;

            var rightAxis = CharacterRight();

            SetLocal(_hips, _hipsRest, Quaternion.Euler(lean * 0.45f, hipYaw, sway * 0.9f));
            SetLocal(_spine, _spineRest, Quaternion.Euler(lean * 0.55f, shoulderCounter * 0.6f, sway * 0.35f));
            SetLocal(_chest, _chestRest, Quaternion.Euler(lean * 0.4f, shoulderCounter, sway * 0.25f));
            SetLocal(_head, _headRest, Quaternion.Euler(sprinting ? -5f : -1f, -shoulderCounter * 0.4f, 0f));

            SwingBone(_thighL, _thighLRest, leftLeg, rightAxis);
            SwingBone(_thighR, _thighRRest, rightLeg, rightAxis);
            SwingBone(_shinL, _shinLRest, shinL, rightAxis);
            SwingBone(_shinR, _shinRRest, shinR, rightAxis);

            // Keep arms near the body while swinging (same adduction base as idle).
            var armCarryL = Quaternion.AngleAxis(42f, CharacterForward()) *
                            Quaternion.AngleAxis(gaitCos * 4f, Vector3.up);
            var armCarryR = Quaternion.AngleAxis(-42f, CharacterForward()) *
                            Quaternion.AngleAxis(-gaitCos * 4f, Vector3.up);
            SwingBone(_armL, _armLRest, leftArm, rightAxis, armCarryL);
            SwingBone(_armR, _armRRest, rightArm, rightAxis, armCarryR);
            SwingBone(_foreL, _foreLRest, 16f + Mathf.Abs(leftArm) * 0.28f, rightAxis);
            SwingBone(_foreR, _foreRRest, 16f + Mathf.Abs(rightArm) * 0.28f, rightAxis);
        }

        void ApplyJump()
        {
            var rightAxis = CharacterRight();
            SetLocal(_hips, _hipsRest, Quaternion.identity);
            SetLocal(_spine, _spineRest, Quaternion.Euler(-6f, 0f, 0f));
            SetLocal(_chest, _chestRest, Quaternion.Euler(-4f, 0f, 0f));
            SetLocal(_head, _headRest, Quaternion.Euler(6f, 0f, 0f));
            SwingBone(_thighL, _thighLRest, -16f, rightAxis);
            SwingBone(_thighR, _thighRRest, -10f, rightAxis);
            SwingBone(_shinL, _shinLRest, 32f, rightAxis);
            SwingBone(_shinR, _shinRRest, 26f, rightAxis);
            SwingBone(_armL, _armLRest, -35f, rightAxis, Quaternion.AngleAxis(40f, CharacterForward()));
            SwingBone(_armR, _armRRest, -30f, rightAxis, Quaternion.AngleAxis(-40f, CharacterForward()));
            SwingBone(_foreL, _foreLRest, 10f, rightAxis);
            SwingBone(_foreR, _foreRRest, 10f, rightAxis);
        }

        Vector3 CharacterRight()
        {
            var t = _visualRoot != null ? _visualRoot : transform;
            var right = t.right;
            right.y = 0f;
            return right.sqrMagnitude > 0.0001f ? right.normalized : Vector3.right;
        }

        Vector3 CharacterForward()
        {
            var t = _visualRoot != null ? _visualRoot : transform;
            var fwd = t.forward;
            fwd.y = 0f;
            return fwd.sqrMagnitude > 0.0001f ? fwd.normalized : Vector3.forward;
        }

        static void SetLocal(Transform bone, Quaternion rest, Quaternion delta)
        {
            if (bone == null)
            {
                return;
            }

            bone.localRotation = rest * delta;
        }

        /// <summary>
        /// Apply a world-axis swing on top of rest (+ optional carry), then bake to localRotation.
        /// Keeps left/right arm and leg swings opposite in the same character frame.
        /// </summary>
        static void SwingBone(Transform bone, Quaternion rest, float degrees, Vector3 axisWorld, Quaternion carryWorld = default)
        {
            if (bone == null)
            {
                return;
            }

            if (carryWorld.Equals(default))
            {
                carryWorld = Quaternion.identity;
            }

            var parentRot = bone.parent != null ? bone.parent.rotation : Quaternion.identity;
            var restWorld = parentRot * rest;
            var swungWorld = Quaternion.AngleAxis(degrees, axisWorld) * carryWorld * restWorld;
            bone.localRotation = Quaternion.Inverse(parentRot) * swungWorld;
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
