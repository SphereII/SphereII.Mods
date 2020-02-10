//using DMT;
//using GamePath;
//using Harmony;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UAI;
//using UnityEngine;


////       <property name = "PathingBlocks" value="PathingCube" />

//class SphereII_EAIApproachSpot
//{
//    public static bool blDisplayLog = false;
//    public static void DisplayLog(String strMessage, EntityAlive theEntity)
//    {
//        if (blDisplayLog)
//            Debug.Log(theEntity.EntityName + ": " + strMessage);
//    }
//    public static Vector3 GetNewPositon( EntityAlive theEntity)
//    {
//        Vector3 result = Vector3.zero;
//        List<Vector3> Paths = SphereCache.GetPaths(theEntity.entityId);
//        if (Paths == null || Paths.Count == 0)
//        {
//            DisplayLog("No Existing Paths. Configuring...", theEntity);
//            //  Grab a list of blocks that are configured for this class.
//            //    <property name = "PathingBlocks" value="" />
//            List<string> Blocks = EntityUtilities.ConfigureEntityClass(theEntity.entityId, "PathingBlocks");
//            if (Blocks.Count == 0)
//            {
//                // DisplayLog("No Blocks configured. Setting Default", __instance.theEntity);
//               // Blocks.Add("PathingCube");
//                  return result;
//            }



//            //Scan for the blocks in the area
//            List<Vector3> PathingVectors = ModGeneralUtilities.ScanForBlockInListHelper(theEntity.position, Blocks, 50);
//            if (PathingVectors.Count == 0)
//            {
//                DisplayLog("No Pathing Targets in this area.", theEntity);
//                return result;
//            }
//            //Add to the cache
//            DisplayLog("Adding Paths to Cache.", theEntity);
//            SphereCache.AddPaths(theEntity.entityId, PathingVectors);
//        }

//        Vector3 newposition = SphereCache.GetRandomPath(theEntity.entityId, theEntity.position);
//        if (newposition == Vector3.zero)
//            return result;

        
        
//        SphereCache.RemovePath(theEntity.entityId, newposition);
//        result =  theEntity.world.FindSupportingBlockPos(newposition);
//        //Debug.Log("Position: " + result);
//        result.x = (float)Utils.Fastfloor(result.x) + 0.5f;
//        result.y = (float)Utils.Fastfloor(result.y) + 0.5f;
//        result.z = (float)Utils.Fastfloor(result.z) + 0.5f;
//        return result;
//    }
//    // If the entity is dead, don't hover over it.
//    [HarmonyPatch(typeof(EAIWander))]
//    [HarmonyPatch("CanExecute")]
//    public class SphereII_EAIApproachSpot_CanExecute
//    {
    
//        public static bool Postfix(bool __result, EAIWander __instance, ref Vector3 ___position)
//        {
//            // If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
//                return __result;

//            if (__result)
//            {
//                Vector3 result = SphereII_EAIApproachSpot.GetNewPositon(__instance.theEntity);
//                if (result == Vector3.zero)
//                    return false;
//                else
//                {
//                    ___position = result;

//                    GameManager.Instance.World.SetBlock(0, new Vector3i(___position), new BlockValue((uint)Block.GetBlockByName("PathingCube2", false).blockID), true, false);

//                    if (SphereCache.LastBlock.ContainsKey(__instance.theEntity.entityId))
//                    {
//                        Debug.Log("Changing old position back to regular pathing cube.");
//                        GameManager.Instance.World.SetBlock(0, new Vector3i(SphereCache.LastBlock[__instance.theEntity.entityId]), new BlockValue((uint)Block.GetBlockByName("PathingCube", false).blockID), true, false);
//                        SphereCache.LastBlock[__instance.theEntity.entityId] = ___position;
//                    }
//                    else
//                        SphereCache.LastBlock.Add(__instance.theEntity.entityId, ___position);

//                }

//                }

//            return __result;
//        }
//    }

//    [HarmonyPatch(typeof(EAIWander))]
//    [HarmonyPatch("Update")]
//    public class SphereII_EAIWander_Update
//    {
//        public static void Postfix(EAIWander __instance, ref EntityAlive ___theEntity, ref float ___time, Vector3 ___position)
//        {
//            // If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
//                return;
           
//            // Check if we are blocked, which may indicate that we are at a door that we want to open.
//            EntityMoveHelper moveHelper = ___theEntity.moveHelper;

//            // If we are close, be done with it.
//            float dist = Vector3.Distance(___position, ___theEntity.position);
//            if ( dist < 2f)
//            {
//                moveHelper.Stop();
//                ___theEntity.navigator.clearPath();
//                ___time = 40f;
//                return;
//            }
//            // If blocked, check to see if its a door.
//            if ( moveHelper.IsBlocked)
//            {
//                Vector3i blockPos = ___theEntity.moveHelper.HitInfo.hit.blockPos;
//                BlockValue block = ___theEntity.world.GetBlock(blockPos);
//                if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
//                {
//                    SphereCache.AddDoor(___theEntity.entityId, blockPos);
//                    EntityUtilities.OpenDoor(__instance.theEntity.entityId, blockPos);
//                    // We were blocked, so let's clear it.
//                    moveHelper.ClearBlocked();
//                }

//                // if we are still blocked, try to re-position and clear the block flag
//                if (moveHelper.IsBlocked && moveHelper.BlockedTime > 2)
//                {
//                    //Debug.Log("I am blocked. Trying to rotate");
//                    ___theEntity.RotateTo(Vector3.back.x, Vector3.back.y, Vector3.zero.z, 180f, 180f);
//                    moveHelper.SetMoveTo( ___theEntity.position + ( Vector3.forward * 3 ), true);
//                    ___time = 40f;
//                    //PathFinderThread.Instance.FindPath(___theEntity, ___position, ___theEntity.GetMoveSpeed(), false, __instance);
//                }

//            }

//            // Check to see if we've opened a door, and close it behind you.
//            Vector3i doorPos = SphereCache.GetDoor(___theEntity.entityId);
//            if (doorPos != Vector3i.zero)
//            {
//                BlockValue block = ___theEntity.world.GetBlock(doorPos);
//                if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
//                {
//                    if ((___theEntity.GetDistanceSq(doorPos.ToVector3()) > 4f))
//                    {
//                        EntityUtilities.CloseDoor(___theEntity.entityId, doorPos);
//                        SphereCache.RemoveDoor(___theEntity.entityId, doorPos);
//                    }
//                }
//            }

//        }
//    }


//    // NPCs shouldn't be jumping when patrolling.
//    [HarmonyPatch(typeof(EntityAlive))]
//    [HarmonyPatch("Jumping", MethodType.Getter)]
//    class MapRoomFunctionality_getMapScale_Patch
//    {
//        public static bool Prefix(EntityAlive __instance, ref bool __result)
//        {
//            // If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(__instance.entityId))
//                return true;

//            __result = false;
//            return false;
//        }
//    }


//    // It looks weird seeing NPCs start digging.
//    [HarmonyPatch(typeof(EntityMoveHelper))]
//    [HarmonyPatch("DigStart")]
//    public class SphereII_EAIWander_DigStart
//    {
//        public static bool Prefix(EntityMoveHelper __instance, EntityAlive ___entity)
//        {
//            // If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(___entity.entityId))
//                return true;

//            return false;
//        }
//    }

//    // Modify push,as it seems that the NPCs will try to fight each other.
//    [HarmonyPatch(typeof(EntityMoveHelper))]
//    [HarmonyPatch("Push")]
//    public class SphereII_EntityMoveHelper
//    {
//        public static bool Prefix(EntityMoveHelper __instance, EntityAlive blockerEntity, EntityAlive ___entity)
//        {
//            // If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(___entity.entityId))
//                return true;

//            return false;
//        }
//    }
       
//    // Take over the Start so we can find the path with Breaking blocks; this will let us open doors.
//    [HarmonyPatch(typeof(EAIWander))]
//    [HarmonyPatch("Start")]
//    public class SphereII_EAIWander_Start
//    {
//        public static bool Prefix(EAIWander __instance, ref EntityAlive ___theEntity, ref float ___time, ref Vector3 ___position)
//        {
//            // If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
//                return true;

//            Debug.Log("AI Path Cost: " + ___theEntity.aiManager.pathCostScale);
//            ___time = -90f;

//            //__instance.manager.pathCostScale = __instance.manager.random.RandomRange(0, 100);
//            //Debug.Log("New AI Path Cost: " + ___theEntity.aiManager.pathCostScale);

//            PathFinderThread.Instance.FindPath(___theEntity, ___position, ___theEntity.GetMoveSpeed(), false, __instance);
//            return false;
//        }
//    }
//}