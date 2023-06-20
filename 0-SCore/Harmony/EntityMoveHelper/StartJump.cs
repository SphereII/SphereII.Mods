using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityMoveHelper
{
    [HarmonyPatch(typeof(global::EntityMoveHelper))]
    [HarmonyPatch("StartJump")]
    public class StartJump
    {
        public static bool Prefix(global::EntityMoveHelper __instance, ref global::EntityAlive ___entity, bool calcYaw, float distance = 0f, float heightDiff = 0)
        {
            if (!EntityUtilities.IsHuman(___entity.entityId)) return true;

            if (__instance.IsBlocked) return true;
            if (__instance.BlockedTime > 0.5) return true;

            EntityUtilities.Stop(___entity.entityId);
            return false;
        }
    }
}