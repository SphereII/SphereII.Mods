//using DMT;
//using GamePath;
//using Harmony;
//using System;
//using System.Collections.Generic;
//using System.Reflection;
//using UAI;
//using UnityEngine;


//       <property name = "PathingBlocks" value="PathingCube" />

//class SphereII_EAIApproachSpot
//{


//    If the entity is dead, don't hover over it.
//    [HarmonyPatch(typeof(EAIWander))]
//    [HarmonyPatch("CanExecute")]
//    public class SphereII_EAIApproachSpot_CanExecute
//    {

//        public static bool Postfix(bool __result, EAIWander __instance, ref Vector3 ___position)
//        {
//            If it's a zombie, don't do anything extra
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

//                    For testing, change the target to this block, so we can see where the NPC intends to go.
//                    GameManager.Instance.World.SetBlock(0, new Vector3i(___position), new BlockValue((uint)Block.GetBlockByName("PathingCube2", false).blockID), true, false);

//                    if (SphereCache.LastBlock.ContainsKey(__instance.theEntity.entityId))
//                    {
//                        GameManager.Instance.World.SetBlock(0, new Vector3i(SphereCache.LastBlock[__instance.theEntity.entityId]), new BlockValue((uint)Block.GetBlockByName("PathingCube", false).blockID), true, false);
//                        SphereCache.LastBlock[__instance.theEntity.entityId] = ___position;
//                    }
//                    else
//                    {
//                        Store the LastBlock position here, so we know what we can remove next time.
//                       SphereCache.LastBlock.Add(__instance.theEntity.entityId, ___position);
//                    }

//                }

//            }

//            return __result;
//        }
//    }

//    [HarmonyPatch(typeof(EAIWander))]
//    [HarmonyPatch("Update")]
//    public class SphereII_EAIWander_Update
//    {
//        public static void Postfix(EAIWander __instance, ref EntityAlive ___theEntity, ref float ___time, Vector3 ___position)
//        {
//            If it's a zombie, don't do anything extra
//            if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
//                return;

//            If there's no positions, don't both processing the special tasks.
//            Vector3 newposition = SphereCache.GetRandomPath(___theEntity.entityId);
//            if (newposition == Vector3.zero)
//                return;

//            Check if we are blocked, which may indicate that we are at a door that we want to open.
//           EntityMoveHelper moveHelper = ___theEntity.moveHelper;

//            If we are close, be done with it.
//            float dist = Vector3.Distance(___position, ___theEntity.position);
//            if (dist < 2f)
//            {
//                moveHelper.Stop();
//                ___theEntity.navigator.clearPath();
//                ___time = 40f;
//                return;
//            }
//            If blocked, check to see if its a door.
//            if (moveHelper.IsBlocked)
//            {
//                Vector3i blockPos = ___theEntity.moveHelper.HitInfo.hit.blockPos;
//                BlockValue block = ___theEntity.world.GetBlock(blockPos);
//                if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
//                {
//                    SphereCache.AddDoor(___theEntity.entityId, blockPos);
//                    EntityUtilities.OpenDoor(__instance.theEntity.entityId, blockPos);
//                    We were blocked, so let's clear it.
//                    moveHelper.ClearBlocked();
//                }

//                if we are still blocked, try to re-position and clear the block flag
//                if (moveHelper.IsBlocked && moveHelper.BlockedTime > 2)
//                {
//                    Debug.Log("I am blocked. Trying to rotate");
//                    ___theEntity.RotateTo(Vector3.back.x, Vector3.back.y, Vector3.zero.z, 180f, 180f);
//                    moveHelper.SetMoveTo(___theEntity.position + (Vector3.forward * 3), true);
//                    ___time = 40f;
//                    PathFinderThread.Instance.FindPath(___theEntity, ___position, ___theEntity.GetMoveSpeed(), false, __instance);
//                }

//                }

//             Check to see if we've opened a door, and close it behind you.
//            Vector3i doorPos = SphereCache.GetDoor(___theEntity.entityId);
//                if (doorPos != Vector3i.zero)
//                {
//                    BlockValue block = ___theEntity.world.GetBlock(doorPos);
//                    if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
//                    {
//                        if ((___theEntity.GetDistanceSq(doorPos.ToVector3()) > 4f))
//                        {
//                            EntityUtilities.CloseDoor(___theEntity.entityId, doorPos);
//                            SphereCache.RemoveDoor(___theEntity.entityId, doorPos);
//                        }
//                    }
//                }

//            }
//        }


//        NPCs shouldn't be jumping when patrolling.
//    [HarmonyPatch(typeof(EntityAlive))]
//        [HarmonyPatch("Jumping", MethodType.Getter)]
//        class MapRoomFunctionality_getMapScale_Patch
//        {
//            public static bool Prefix(EntityAlive __instance, ref bool __result)
//            {
//                If it's a zombie, don't do anything extra
//                if (!EntityUtilities.IsHuman(__instance.entityId))
//                    return true;

//                __result = false;
//                return false;
//            }
//        }


//        It looks weird seeing NPCs start digging.


//       [HarmonyPatch(typeof(EntityMoveHelper))]
//       [HarmonyPatch("DigStart")]
//    public class SphereII_EAIWander_DigStart
//        {
//            public static bool Prefix(EntityMoveHelper __instance, EntityAlive ___entity)
//            {
//                If it's a zombie, don't do anything extra
//                if (!EntityUtilities.IsHuman(___entity.entityId))
//                    return true;

//                return false;
//            }
//        }

//        Modify push,as it seems that the NPCs will try to fight each other.


//       [HarmonyPatch(typeof(EntityMoveHelper))]
//    [HarmonyPatch("Push")]
//    public class SphereII_EntityMoveHelper
//        {
//            public static bool Prefix(EntityMoveHelper __instance, EntityAlive blockerEntity, EntityAlive ___entity)
//            {
//                If it's a zombie, don't do anything extra
//                if (!EntityUtilities.IsHuman(___entity.entityId))
//                    return true;

//                return false;
//            }
//        }

//        Take over the Start so we can find the path with Breaking blocks; this will let us open doors.


//       [HarmonyPatch(typeof(EAIWander))]
//       [HarmonyPatch("Start")]
//    public class SphereII_EAIWander_Start
//        {
//            public static bool Prefix(EAIWander __instance, ref EntityAlive ___theEntity, ref float ___time, ref Vector3 ___position)
//            {
//                If it's a zombie, don't do anything extra
//                if (!EntityUtilities.IsHuman(__instance.theEntity.entityId))
//                    return true;

//                If there's no positions, don't both processing the special tasks.
//            Vector3 newposition = SphereCache.GetRandomPath(___theEntity.entityId);
//                if (newposition == Vector3.zero)
//                    return true;

//                Give them more time to path find.The CanContinue() stops at 30f, so we'll set it at -90, rather than 0.
//                ___time = -90f;

//                __instance.manager.pathCostScale = __instance.manager.random.RandomRange(0, 100);
//                Debug.Log("New AI Path Cost: " + ___theEntity.aiManager.pathCostScale);

//                PathFinderThread.Instance.FindPath(___theEntity, ___position, ___theEntity.GetMoveSpeed(), false, __instance);
//                return false;
//            }
//        }
//    }