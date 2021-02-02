using DMT;
using HarmonyLib;
using System;
using UnityEngine;


/**
 * SphereII_RemoveTraderProtection
 *
 * This class includes a Harmony patches to disable the landclaim block on the trader, making them vulnerable.
 * 
 */
public class SphereII_RemoveTraderProtection
{
    private static readonly string AdvFeatureClass = "AdvancedPrefabFeatures";
    private static readonly string Feature = "DisableTraderProtection";


    public class SphereII_RemoveTraderProtection_Start : IHarmony
    {
        // Special patch here, since we need to explicity target a constructor
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = new Harmony(GetType().ToString());

            // Navezgane only - Since it's pregenerated, it uses a different prefabs loading, with preset locations. This will adjust the prefabs for only navezgane.
            var original = typeof(PrefabInstance).GetConstructor(new Type[] { typeof(int), typeof(PathAbstractions.AbstractedLocation), typeof(Vector3i), typeof(byte), typeof(Prefab), typeof(int) });
            var prefix = typeof(SphereII_RemoveTraderProtection_PrefabInstance).GetMethod("PrefabInstance_Prefix");
            harmony.Patch(original, new HarmonyMethod(prefix));
        }
    }


    // Navezgane only - Since it's pregenerated, it uses a different prefabs loading, with preset locations. This will adjust the prefabs for only navezgane.
    public class SphereII_RemoveTraderProtection_PrefabInstance
    {
        public static bool PrefabInstance_Prefix(ref Vector3i _position, ref Prefab _bad)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // Only apply these changes to navezgane world
            if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
            {
                if (_bad != null)
                    _bad.bTraderArea = false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("LoadXMLData")]
    public class SphereII_RemoveTraderProtection_LoadXMLData
    {
        public static void Postfix(Prefab __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            __instance.bTraderArea = false;


        }

    }
}
