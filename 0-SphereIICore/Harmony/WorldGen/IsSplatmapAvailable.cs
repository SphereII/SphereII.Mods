using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Harmony;
using UnityEngine;

class IsSplatmapAvailable
{
    private static string AdvFeatureClass = "AdvancedWorldGen";
    private static string Feature = "DisableSplatMap";

    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("IsSplatMapAvailable")]
    public class SphereII_GameManager_SplatMap
    {
        public static bool Prefix(bool __result)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            AdvLogging.DisplayLog(AdvFeatureClass, "Disabling Splat Map");
            __result = false;
            return false;
        }
    }
}

