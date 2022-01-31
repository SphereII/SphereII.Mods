using HarmonyLib;
using UnityEngine;

namespace Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(EModelBase))]
    [HarmonyPatch("SetLookAt")]
    public class EmodelBaseSetLookAt
    {
        private static bool Prefix(EModelBase __instance, Entity ___entity, Vector3 _pos)
        {
            // Check if it's a trader. This controls whether the NPC will look at you if you are within a particular radius.
            if (___entity.HasAnyTags(FastTags.Parse("trader")))
                return true;

            if ( EntityUtilities.GetAttackOrRevengeTarget(___entity.entityId) != null )
                    return false;

            if (___entity.GetDistanceSq(_pos) < 10)
                return true;

            return false;
        }
    }
}