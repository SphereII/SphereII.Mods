using HarmonyLib;

/**
 * EntityNPCMakeVulnerable
 *
 * This class includes a Harmony patches to the the EntityNPC to make them vulnerable to attacks and damage.
 * 
 */
class EntityNPCMakeVulnerable
{
    private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";
    private static readonly string Feature = "MakeTraderVulnerable";

    [HarmonyPatch(typeof(EntityNPC))]
    [HarmonyPatch("PostInit")]
    public class SphereII_RemoveTraderProtection_PostInit
    {
        public static void Postfix(EntityNPC __instance)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            __instance.IsGodMode.Value = false;
        }
    }


    [HarmonyPatch(typeof(EntityNPC))]
    [HarmonyPatch("ProcessDamageResponseLocal")]
    public class SphereII_RemoveTraderProtection_OricessDamageResponse
    {
        public static bool Prefix(EntityNPC __instance, int __state)
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

        public static void Postfix(EntityNPC __instance, int __state)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            if (__instance.NPCInfo == null)
                return;

            __instance.NPCInfo.TraderID = __state;
        }
    }

    [HarmonyPatch(typeof(EntityNPC))]
    [HarmonyPatch("DamageEntity")]
    public class SphereII_RemoveTraderProtection_DamageEntity
    {
        public static bool Prefix(EntityNPC __instance, int __state)
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

        public static void Postfix(EntityNPC __instance, int __state)
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

