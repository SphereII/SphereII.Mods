using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
class EntityNPCGreetingFix
{
    private static string AdvFeatureClass = "AdvancedPrefabFeatures";
    private static string Feature = "DisableTraderProtection";

    [HarmonyPatch(typeof(EntityNPC))]
    [HarmonyPatch("OnUpdateLive")]
    public class SphereII_RemoveTraderProtection_OnUpdateLive
    {
        public static bool Prefix(EntityNPC __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // Create a new on-the-fly trader area
            if (__instance.traderArea == null  )
            {
                Vector3i size = new Vector3i(2f, 2f, 2f);
                __instance.traderArea = new TraderArea(new Vector3i(__instance.position), size, size, size, size);
            }
            return true;
        }
    }
}

