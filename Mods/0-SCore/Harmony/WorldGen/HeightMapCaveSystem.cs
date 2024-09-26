using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SCore.Features.Caves.Scripts;
using UnityEngine;
using WorldGenerationEngineFinal;

namespace Harmony.WorldGen
{
    public class SCoreheightMapCaveSystem
    {
        // Make the world darker underground
        [HarmonyPatch(typeof(WorldBiomeProviderFromImage))]
        [HarmonyPatch("InitData")]
        public class SCoreHeightMapCaveSystemGameManager
        {
            private static readonly string AdvFeatureClass = "CaveConfiguration";
            private static readonly string CavePath = "CavePath";
            private static readonly string Feature = "CaveEnabled";
            public static void Postfix()
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                var configurationType = Configuration.GetPropertyValue(AdvFeatureClass, "GenerationType");
                if (configurationType != "Texture2D")
                    return;

                SCoreCavesUtils.SetTexture();
            }
        }
    }
}
