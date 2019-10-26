using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

// Add Reference to Assembly-CSharp-firstpass for UMA references to resolve in Visual Studio
public class SphereII_LowerResolutionUMA
{
    private static string AdvFeatureClass = "AdvancedZombieFeatures";
    private static string Feature = "LowerResolutionUMA";

    // Drastically reduce the texture size of the generated UMAs, making them spawn in faster and generate smaller files.
    [HarmonyPatch(typeof(UMA.UMAGeneratorCoroutine))]
    [HarmonyPatch("workerMethod")]
    public class SphereII_ArchetypeTextureCache
    {
        public static bool Prefix(ref UMA.UMAData ___umaData)
        {
            // Check if this feature is enabled.
            if(!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            ___umaData.AtlasSize = 512;

            // Changing the Atlas size down  for all UMAs
            if (SkyManager.BloodMoon() || SkyManager.IsDark())
                ___umaData.AtlasSize = 128;

            AdvLogging.DisplayLog(AdvFeatureClass, Feature + " - Atlas Size: " + ___umaData.AtlasSize);
            return true;
        }
    }

}


