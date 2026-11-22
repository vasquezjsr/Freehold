#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Freehold.EditorTools
{
    /// <summary>
    /// Convenience menu to open the starter scene and confirm bootstrap is present.
    /// </summary>
    public static class EnsurePlayableScene
    {
        const string ScenePath = "Assets/Scenes/StarterArea.unity";

        [MenuItem("Freehold/Open Starter Area Scene")]
        public static void OpenStarterScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("Opened StarterArea. Press Play — GameBootstrap auto-spawns the survival slice.");
        }

        [MenuItem("Freehold/Validate Survival Slice")]
        public static void Validate()
        {
            var scriptsOk =
                AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/World/GameBootstrap.cs") != null &&
                AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Player/PlayerController.cs") != null &&
                AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Crafting/Campfire.cs") != null &&
                AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Art/Pack/PackPropFactory.cs") != null &&
                AssetDatabase.LoadAssetAtPath<MonoScript>("Assets/Scripts/Art/Pack/PackPlayerVisual.cs") != null &&
                AssetDatabase.IsValidFolder("Assets/Floreswa") &&
                AssetDatabase.IsValidFolder("Assets/BigRookGames/Nature Pack") &&
                AssetDatabase.LoadAssetAtPath<GameObject>(PackPathsProxy.Hero) != null &&
                AssetDatabase.LoadAssetAtPath<GameObject>(PackPathsProxy.Tree) != null;

            if (!scriptsOk)
            {
                Debug.LogError("Freehold: missing core scripts or purchased Asset Store packs (Floreswa / BigRookGames).");
                return;
            }

            var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (scene == null)
            {
                Debug.LogError("Freehold: StarterArea scene missing.");
                return;
            }

            Debug.Log("Freehold survival slice looks complete with store packs. Open StarterArea and press Play.");
        }

        static class PackPathsProxy
        {
            public const string Hero = "Assets/Floreswa/Prefabs/male01_1.prefab";
            public const string Tree = "Assets/BigRookGames/Nature Pack/Prefabs/tree_common_01.prefab";
        }
    }
}
#endif
