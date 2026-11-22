using System.IO;
using UnityEngine;

namespace Freehold.Art.Pack
{
    public static class PackPaths
    {
        public static string ThirdPartyRoot => Path.Combine(Application.dataPath, "ThirdParty");
        public static string KenneyModels => Path.Combine(ThirdPartyRoot, "KenneyNatureKit", "Models");
        public static string QuaterniusModels => Path.Combine(ThirdPartyRoot, "QuaterniusRpgItems", "Models");

        // Purchased Asset Store packs (imported under Assets/).
        public const string FloreswaHeroPrefab = "Assets/Floreswa/Prefabs/male01_1.prefab";
        public const string FloreswaHeroFbx = "Assets/Floreswa/Models/male01_1.fbx";

        public const string NaturePrefabRoot = "Assets/BigRookGames/Nature Pack/Prefabs";
        public const string NatureMaterialRoot = "Assets/BigRookGames/Nature Pack/Materials";

        public static string Kenney(string fileNameWithoutExt) => Path.Combine(KenneyModels, fileNameWithoutExt + ".obj");
        public static string Quaternius(string fileNameWithoutExt) => Path.Combine(QuaterniusModels, fileNameWithoutExt + ".obj");

        public static string NaturePrefab(string prefabName) => $"{NaturePrefabRoot}/{prefabName}.prefab";
        public static string NatureMaterial(string materialName) => $"{NatureMaterialRoot}/{materialName}.mat";
    }
}
