using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityMoveHelper
{
    [HarmonyPatch(typeof(global::EntityMoveHelper))]
    [HarmonyPatch("StartJump")]
    public class StartJump
    {
        public static bool Prefix(ref global::EntityAlive ___entity, bool calcYaw, float distance = 0f, float heightDiff = 0)
        {
            if (!EntityUtilities.IsHuman(___entity.entityId)) return true;

            if (heightDiff > 1f) return true;

            EntityUtilities.Stop(___entity.entityId);
            return false;
        }
    }
}