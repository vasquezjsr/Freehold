using Freehold.Art;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Freehold.Art.Pack
{
    /// <summary>
    /// Instantiates the Vuena CC0 blocky hero. Prefers Resources, then Editor AssetDatabase.
    /// Falls back to procedural humanoid only if the pack asset is unavailable.
    /// </summary>
    public static class PackPlayerVisual
    {
        const string ResourcesPath = "Characters/Hero";
        const string EditorFbxPath = "Assets/ThirdParty/VuenaBlockyCharacters/Unity/blocky_01_hero.fbx";

        public static bool TryAttach(Transform playerRoot)
        {
            var source = LoadHeroSource();
            if (source == null)
            {
                Debug.LogWarning("Freehold: Vuena hero FBX not available yet — using procedural humanoid fallback. After Unity imports ThirdParty assets, Play again or use Freehold/Refresh Pack Player.");
                PlayerVisualBuilder.Build(playerRoot);
                return false;
            }

            var instance = Object.Instantiate(source, playerRoot);
            instance.name = "HeroVisual";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            // Prefer pack Animation clips (idle/walk/run/jump) when present.
            var anim = instance.GetComponentInChildren<Animation>();
            var animator = instance.GetComponentInChildren<Animator>();
            var bridge = playerRoot.gameObject.AddComponent<PackPlayerAnimationBridge>();
            bridge.Configure(playerRoot.GetComponent<CharacterController>(), anim, animator);
            return true;
        }

        static GameObject LoadHeroSource()
        {
            var fromResources = Resources.Load<GameObject>(ResourcesPath);
            if (fromResources != null)
            {
                return fromResources;
            }

#if UNITY_EDITOR
            var fromAsset = AssetDatabase.LoadAssetAtPath<GameObject>(EditorFbxPath);
            if (fromAsset != null)
            {
                return fromAsset;
            }
#endif
            return null;
        }
    }

    /// <summary>Drives pack Animation/Animator from CharacterController speed.</summary>
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

                // Try first clip containing name fragment
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
                // Without a custom controller, Animator may still expose parameters later.
            }
        }
    }
}
