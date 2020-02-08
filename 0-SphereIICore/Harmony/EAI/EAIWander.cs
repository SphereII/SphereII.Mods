using DMT;
using GamePath;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


//       <property name = "PathingBlocks" value="PathingCube" />

class SphereII_EAIApproachSpot
{
    // If the entity is dead, don't hover over it.
    [HarmonyPatch(typeof(EAIWander))]
    [HarmonyPatch("CanExecute")]
    public class SphereII_EAIApproachSpot_CanExecute
    {
        public static bool blDisplayLog = true;
        public static void DisplayLog(String strMessage, EntityAlive theEntity)
        {
            if (blDisplayLog)
                Debug.Log(theEntity.EntityName + ": " + strMessage);
        }
        public static bool Postfix(bool __result, EAIWander __instance, ref Vector3 ___position)
        {
            // If it's a zombie, don't do anything extra
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return __result;

            if (__result)
            {
                List<Vector3> Paths = SphereCache.GetPaths(__instance.theEntity.entityId);
                if (Paths == null || Paths.Count == 0)
                {
                    DisplayLog("No Existing Paths. Configuring...", __instance.theEntity);
                    //  Grab a list of blocks that are configured for this class.
                    //    <property name = "PathingBlocks" value="" />
                    List<string> Blocks = EntityUtilities.ConfigureEntityClass(__instance.theEntity.entityId, "PathingBlocks");
                    if (Blocks.Count == 0)
                    {
                       // DisplayLog("No Blocks configured. Setting Default", __instance.theEntity);
                     //   Blocks.Add("PathingCube");
                        return __result;
                    }

                    //Scan for the blocks in the area
                    List<Vector3> PathingVectors = ModGeneralUtilities.ScanForBlockInListHelper(__instance.theEntity.position, Blocks, 30);
                    if (PathingVectors.Count == 0)
                    {
                        DisplayLog("No Pathing Targets in this area.", __instance.theEntity);
                        return __result;
                    }
                    //Add to the cache
                    DisplayLog("Adding Paths to Cache.", __instance.theEntity);
                    SphereCache.AddPaths(__instance.theEntity.entityId, PathingVectors);
                }

                Vector3 newposition = SphereCache.GetRandomPath(__instance.theEntity.entityId);
                if (newposition == Vector3.zero)
                    return __result;

                SphereCache.RemovePath(__instance.theEntity.entityId, newposition);
                ___position = __instance.theEntity.world.FindSupportingBlockPos(newposition);
            }

            return __result;
        }
    }

    [HarmonyPatch(typeof(EAIWander))]
    [HarmonyPatch("Update")]
    public class SphereII_EAIWander_Update
    {
        public static void Postfix(EAIWander __instance, ref EntityAlive ___theEntity)
        {
            // If it's a zombie, don't do anything extra
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return;

            // Check if we are blocked, which may indicate that we are at a door that we want to open.
            EntityMoveHelper moveHelper = ___theEntity.moveHelper;

            // If blocked, check to see if its a door.
            if ( moveHelper.IsBlocked)
            {
                Vector3i blockPos = ___theEntity.moveHelper.HitInfo.hit.blockPos;
                BlockValue block = ___theEntity.world.GetBlock(blockPos);
                if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
                {
                    SphereCache.AddDoor(___theEntity.entityId, blockPos);
                    EntityUtilities.OpenDoor(__instance.theEntity.entityId, blockPos);
                    // We were blocked, so let's clear it.
                    moveHelper.ClearBlocked();
                }
            }

            // Check to see if we've opened a door, and close it behind you.
            Vector3i doorPos = SphereCache.GetDoor(___theEntity.entityId);
            if (doorPos != Vector3i.zero)
            {
                BlockValue block = ___theEntity.world.GetBlock(doorPos);
                if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
                {
                    if ((___theEntity.GetDistanceSq(doorPos.ToVector3()) > 4f))
                    {
                        EntityUtilities.CloseDoor(___theEntity.entityId, doorPos);
                        SphereCache.RemoveDoor(___theEntity.entityId, doorPos);
                    }
                }
            }

        }
    }

  
    // Take over the Start so we can find the path with Breaking blocks; this will let us open doors.
    [HarmonyPatch(typeof(EAIWander))]
    [HarmonyPatch("Start")]
    public class SphereII_EAIWander_Start
    {
        public static bool Prefix(EAIWander __instance, ref EntityAlive ___theEntity, ref float ___time, ref Vector3 ___position)
        {
            // If it's a zombie, don't do anything extra
            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
                return true;

            ___time = 0f;
            
            PathFinderThread.Instance.FindPath(___theEntity, ___position, ___theEntity.GetMoveSpeed(), true, __instance);
            return false;
        }
    }
}