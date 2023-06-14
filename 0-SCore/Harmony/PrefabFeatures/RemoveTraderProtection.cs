using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Harmony.PrefabFeatures
{
    /**
     * SCoreRemoveTraderProtection
     * 
     * This class includes a Harmony patches to disable the landclaim block on the trader, making them vulnerable.
     */
    public class RemoveTraderProtection
    {
        private static readonly string AdvFeatureClass = "AdvancedPrefabFeatures";
        private static readonly string Feature = "DisableTraderProtection";

        // Patch by Khaine.
        [HarmonyPatch(typeof(TraderArea), MethodType.Constructor)]
        [HarmonyPatch(new Type[]
     {
            typeof(Vector3i),
            typeof(Vector3i),
            typeof(Vector3i),
            typeof(List<Prefab.PrefabTeleportVolume>)
     })]
        public class TraderArea_Patch
        {
            public static void Postfix(TraderArea __instance)
            {
                //Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;
                Vector3i vector3I = new Vector3i(1, 1, 1);
                __instance.ProtectSize = vector3I;
            }
        }
        //public class RemoveTraderProtectionStart : IModApi
        //{
        //    public void InitMod(Mod _modInstance)
        //    {
        //        Debug.Log(" Loading Patch: " + GetType());
        //        var harmony = new HarmonyLib.Harmony(GetType().ToString());

        //        // Navezgane only - Since it's pre generated, it uses a different prefabs loading, with preset locations. This will adjust the prefabs for only navezgane.
        //        var original = typeof(PrefabInstance).GetConstructor(new[] { typeof(int), typeof(PathAbstractions.AbstractedLocation), typeof(Vector3i), typeof(byte), typeof(Prefab), typeof(int) });
        //        var prefix = typeof(RemoveTraderProtectionPrefabInstance).GetMethod("PrefabInstance_Prefix");
        //        harmony.Patch(original, new HarmonyMethod(prefix));
        //    }
        //}


        //// Navezgane only - Since it's pre generated, it uses a different prefabs loading, with preset locations. This will adjust the prefabs for only navezgane.
        //public class RemoveTraderProtectionPrefabInstance
        //{
        //    public static bool PrefabInstance_Prefix(ref Vector3i _position, ref Prefab _bad)
        //    {
        //        // Check if this feature is enabled.
        //        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
        //            return true;

        //        // Only apply these changes to navezgane world
        //        if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Navezgane")
        //            if (_bad != null)
        //                _bad.bTraderArea = false;
        //        return true;
        //    }
        //}

        //[HarmonyPatch(typeof(Prefab))]
        //[HarmonyPatch("LoadXMLData")]
        //public class RemoveTraderProtectionLoadXMLData
        //{
        //    public static void Postfix(Prefab __instance)
        //    {
        //        // Check if this feature is enabled.
        //        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
        //            return;

        //        __instance.bTraderArea = false;
        //    }
        //}
    }
}