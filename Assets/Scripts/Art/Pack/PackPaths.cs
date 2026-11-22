using System.IO;
using UnityEngine;

namespace Freehold.Art.Pack
{
    public static class PackPaths
    {
        public static string ThirdPartyRoot => Path.Combine(Application.dataPath, "ThirdParty");
        public static string KenneyModels => Path.Combine(ThirdPartyRoot, "KenneyNatureKit", "Models");
        public static string QuaterniusModels => Path.Combine(ThirdPartyRoot, "QuaterniusRpgItems", "Models");
        public static string VuenaHeroFbx => Path.Combine(ThirdPartyRoot, "VuenaBlockyCharacters", "Unity", "blocky_01_hero.fbx");

        public static string Kenney(string fileNameWithoutExt) => Path.Combine(KenneyModels, fileNameWithoutExt + ".obj");
        public static string Quaternius(string fileNameWithoutExt) => Path.Combine(QuaterniusModels, fileNameWithoutExt + ".obj");
    }
}
