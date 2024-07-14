using HarmonyLib;

/**
 * SphereIICore_EntityMoveHelper
 * 
 * This class includes a Harmony patches to modify the EntityMoveHelper that attempts to allow the human NPCs to behave differently than zombies,
 * such as giving them to ability to stop without drifting (due to root motion accumulated speed, etc), prevent them from jumping needlessly, or digging.
 * 
 */
namespace Harmony.EntityMoveHelper
{
    // Includes resetting the speedForward to 0 when the entity is told to stop; this stops it from sliding or taking a few extra steps
    [HarmonyPatch(typeof(global::EntityMoveHelper))]
    [HarmonyPatch("Stop")]
    public class Stop
    {
        public static void Postfix(ref global::EntityMoveHelper __instance, ref global::EntityAlive ___entity)
        {
             ___entity.speedForward = 0f;
        }
    }
}