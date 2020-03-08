using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
class EntityNPCJumpHeight
{
    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("StartJump")]
    public class SphereII_EntityNPCJumpHeight_StartJump
    {
        public static bool Prefix(EntityMoveHelper __instance, ref float heightDiff)
        {
            float JumpHeight = EntityUtilities.GetFloatValue(__instance.entity.entityId, "JumpHeight");
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

