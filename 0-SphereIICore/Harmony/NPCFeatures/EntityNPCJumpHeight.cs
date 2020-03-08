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
            heightDiff = JumpHeight;

            return true;
        }
    }
}

