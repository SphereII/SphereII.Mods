using HarmonyLib;

namespace Harmony.NPCFeatures
{
    /**
     * EntityNPCMakeVulnerable
     * 
     * This class includes a Harmony patches to the the EntityNPC to make them vulnerable to attacks and damage.
     */
    internal class EntityNPCMakeVulnerable
    {
        private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";
        private static readonly string Feature = "MakeTraderVulnerable";

        [HarmonyPatch(typeof(EntityTrader))]
        [HarmonyPatch("PostInit")]
        public class SCoreRemoveTraderProtectionPostInit
        {
            public static void Postfix(EntityTrader __instance)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                __instance.IsGodMode.Value = false;
            }
        }


        [HarmonyPatch(typeof(EntityTrader))]
        [HarmonyPatch("ProcessDamageResponseLocal")]
        public class SCoreRemoveTraderProtectionProcessDamageResponse
        {
            public static bool Prefix(EntityTrader __instance, int __state)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (__instance.NPCInfo == null)
                    return true;

                __state = __instance.NPCInfo.TraderID;
                __instance.NPCInfo.TraderID = 0;
                return true;
            }

            public static void Postfix(EntityTrader __instance, int __state)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                if (__instance.NPCInfo == null)
                    return;

                __instance.NPCInfo.TraderID = __state;
                // EntityTraders turn off their physics transforms, but we want it on,
                // Otherwise NPCs won't collider with each other.
                __instance.PhysicsTransform.gameObject.SetActive(true);
            }
        }

        [HarmonyPatch(typeof(EntityTrader))]
        [HarmonyPatch("DamageEntity")]
        public class SCoreRemoveTraderProtectionDamageEntity
        {
            public static bool Prefix(EntityTrader __instance, int __state)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                if (__instance.NPCInfo == null)
                    return true;

                __state = __instance.NPCInfo.TraderID;
                __instance.NPCInfo.TraderID = 0;
                return true;
            }

            public static void Postfix(EntityTrader __instance, int __state)
            {
                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                if (__instance.NPCInfo == null)
                    return;

                __instance.NPCInfo.TraderID = __state;
            }
        }
    }
}