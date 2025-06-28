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

            // var position = ___entity.position;
            // var vector3I = new Vector3i(Utils.Fastfloor(position.x), Utils.Fastfloor(position.y + 2.35f), Utils.Fastfloor(position.z));
            // var block = ___entity.world.GetBlock(vector3I);
            // if (block.isair && !__instance.IsBlocked) return false;
            if (__instance.BlockedTime > SCoreConstants.BlockTimeToJump) return true;


            EntityUtilities.Stop(___entity.entityId);
            return false;
        }
    }
}