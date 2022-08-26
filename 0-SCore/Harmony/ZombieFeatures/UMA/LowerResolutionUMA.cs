using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class LowerResolutionUMA
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
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (___umaData.ArchetypeIsPlayer)
                return true;

            ___umaData.AtlasSize = 512;

            // Changing the Atlas size down  for all UMAs
            if (SkyManager.IsBloodMoonVisible() || SkyManager.IsDark())
                ___umaData.AtlasSize = 128;

            AdvLogging.DisplayLog(AdvFeatureClass, Feature + " - Atlas Size: " + ___umaData.AtlasSize);
            return true;
        }
    }
}

