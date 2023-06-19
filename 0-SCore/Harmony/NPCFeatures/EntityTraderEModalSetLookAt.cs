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

            global::EntityAlive attackTarget = null;
            global::EntityAlive revengeTarget = null;
            
            // If we don't have any attack or revenge then change the distance for the look
            if (!EntityUtilities.GetAttackAndRevengeTarget(___entity.entityId, ref attackTarget, ref revengeTarget))
                return ___entity.GetDistanceSq(_pos) < 10;
            
            if (attackTarget != null && attackTarget.IsAlive())
                return true;
            if (revengeTarget != null && revengeTarget.IsAlive())
                return true;
            
            // var target = EntityUtilities.GetAttackOrRevengeTarget(___entity.entityId);
            // if ( target == null ) return false;
            // if (target.IsDead()) return false;

            return ___entity.GetDistanceSq(_pos) < 10;
        }
    }
}