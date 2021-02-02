using HarmonyLib;


/**
 * EntityNPCJumpHeight
 *
 * This class includes a Harmony patches to the EntityMoveHelper, to allow entities to jump higher than normal.
 * 
 * XML Usage for entityclasses.xml
 * 
 * <property name="JumpHeight" value="10" />
 */
class EntityNPCJumpHeight
{

    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("StartJump")]
    public class SphereII_EntityNPCJumpHeight_StartJump
    {
        public static bool Prefix(EntityMoveHelper __instance, ref float heightDiff, EntityAlive ___entity)
        {
            float JumpHeight = EntityUtilities.GetFloatValue(___entity.entityId, "JumpHeight");
            if (JumpHeight == -1f)
                return true;

            if (JumpHeight == 0f)
                return true;

            // These are the values set in the EntityMoveHelper's update. They are filtered here so that the EAI Task Swim and Leap will be unaffected.
            //   if ( heightDiff > 1.1f && heightDiff < 1.5f)
            heightDiff = JumpHeight;

            return true;
        }
    }
}

