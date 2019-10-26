using DMT;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

public class SphereII_Progression
{
    private static string AdvFeatureClass = "AdvancedProgression";
    private static string Feature = "ZeroXP";

    [HarmonyPatch(typeof(Progression))]
    [HarmonyPatch("AddLevelExpRecursive")]
    public class SphereII_Progression_AddLevelExpRecursive
    {
        static bool Prefix()
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;
            return false;
        }
    }


    [HarmonyPatch(typeof(Progression))]
    [HarmonyPatch("AddLevelExp")]
    public class SphereII_Progression_AddLevelExp
    {
        static bool Prefix()
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            return false;
        }
    }
}