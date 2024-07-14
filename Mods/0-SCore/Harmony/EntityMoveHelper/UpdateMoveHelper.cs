using HarmonyLib;

namespace Harmony.EntityMoveHelper
{
    [HarmonyPatch(typeof(global::EntityMoveHelper))]
    [HarmonyPatch("UpdateMoveHelper")]
    public class MoveHelperUpdate
    {
        public static bool Prefix(global::EntityAlive ___entity, ref int ___moveToFailCnt, ref int ___moveToTicks)
        {
            if (!EntityUtilities.IsHuman(___entity.entityId))
                return true;

             // This patch is for the NPCs who seem to get stuck, then start jumping, moving back to position, and looping again.
             if (___moveToTicks <= 5 || ___moveToFailCnt < 2) return true;
             ___moveToFailCnt = 0;
            ___moveToTicks = 0;
            EntityUtilities.Stop(___entity.entityId);
            return false;

        }
    }
}