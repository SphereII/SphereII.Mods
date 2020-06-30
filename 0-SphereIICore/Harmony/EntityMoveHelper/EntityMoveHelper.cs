using DMT;
using HarmonyLib;
using System;
using UnityEngine;

/**
 * SphereIICore_EntityMoveHelper
 * 
 * This class includes a Harmony patches to modify the EntityMoveHelper that attempts to allow the human NPCs to behave differently than zombies,
 * such as giving them to ability to stop without drifting (due to root motion accumulated speed, etc), prevent them from jumping needlessly, or digging.
 * 
 */
namespace SphereIICore_EntityMoveHelper
{

    // Includes resetting the speedForward to 0 when the entity is told to stop; this stops it from sliding or taking a few extra steps
    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("Stop")]
    public class SphereII_EntityMoveHelper_Stop
    {
        public static void Postfix(ref EntityMoveHelper __instance, ref EntityAlive ___entity)
        {
                ___entity.speedForward = 0f;
        }
    }

    // Disables digging for Humans
    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("DigStart")]
    public class SphereII_EntityMoveHelper_DigStart
    {
        public static bool Prefix(EntityAlive ___entity)
        {
            if (EntityUtilities.IsHuman(___entity.entityId))
                return false;

            return true;

        }
    }

    // Disables random jumping for Humans
    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("StartJump")]
    public class SphereII_EntityMoveHelper_StartJump
    {
        public static bool Prefix(ref EntityAlive ___entity, bool calcYaw, float distance = 0f, float heightDiff = 0)
        {
            if (EntityUtilities.IsHuman(___entity.entityId))
            {
                if (heightDiff == 1.4f)
                {
                    EntityUtilities.Stop(___entity.entityId);
                    return false;
                }
            }
            return true;
            
        }
    }
}
