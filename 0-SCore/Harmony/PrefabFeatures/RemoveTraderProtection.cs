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

        // keeps the doors open / unlocked.
        [HarmonyPatch(typeof(TraderArea))]
        [HarmonyPatch("SetClosed")]
        public class TraderAreaSetClosed
        {
            public static bool Prefix(bool _bClosed)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                return _bClosed != true;
            }
        }

        // Mutes the speakers
        [HarmonyPatch(typeof(TraderArea))]
        [HarmonyPatch("HandleWarning")]
        public class TraderAreaHandleWarning
        {
            public static bool Prefix()
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                return false;
            }
        }


        // This disables the check most calls will be making to the trader system that determines if you are inside it or not.
        // This includes checking for block damages, picking up items, etc.
        [HarmonyPatch(typeof(World))]
        [HarmonyPatch("IsWithinTraderArea")]
        [HarmonyPatch(new Type[]
        {
            typeof(Vector3i)
        })]
        public class WorldIsWithinTraderArea
        {
            public static bool Prefix(ref bool __result)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                __result = false;
                return false;
            }
        }

        // Allows placing of blocks in the trader area
        [HarmonyPatch(typeof(World))]
        [HarmonyPatch("IsWithinTraderPlacingProtection")]
        public class WorldIsWithinTraderPlacingProtection
        {
            public static bool Prefix(ref bool __result)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;
                
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "AllowBuildingInTraderArea"))
                    return true;

                __result = false;
                return false;
            }
        }

      
    }
}