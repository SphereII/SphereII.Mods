using HarmonyLib;

namespace Harmony.NPCFeatures
{
    /**
     * EntityNPCJumpHeight
     * 
     * This class includes a Harmony patches to the EntityMoveHelper, to allow entities to jump higher than normal.
     * 
     * XML Usage for entityclasses.xml
     * <property name="JumpHeight" value="10" />
     */
    internal class EntityNPCJumpHeight
    {
        [HarmonyPatch(typeof(global::EntityMoveHelper))]
        [HarmonyPatch("StartJump")]
        public class EntityNPCJumpHeightStartJump
        {
            public static bool Prefix(global::EntityMoveHelper __instance, ref float heightDiff, global::EntityAlive ___entity)
            {
                var jumpHeight = EntityUtilities.GetFloatValue(___entity.entityId, "JumpHeight");
                if (jumpHeight <= 0f)
                    return true;

                // These are the values set in the EntityMoveHelper's update. They are filtered here so that the EAI Task Swim and Leap will be unaffected.
                //   if ( heightDiff > 1.1f && heightDiff < 1.5f)
                heightDiff = jumpHeight;

                return true;
            }
        }
    }
}