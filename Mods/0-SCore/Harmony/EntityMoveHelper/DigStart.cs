using HarmonyLib;

namespace Harmony.EntityMoveHelper
{
    // Disables NPCs and humans from digging.
    [HarmonyPatch(typeof(global::EntityMoveHelper))]
    [HarmonyPatch("DigStart")]
    public class DigStart
    {
        public static bool Prefix(global::EntityAlive ___entity)
        {
            return !EntityUtilities.IsHuman(___entity.entityId);
        }
    }
}