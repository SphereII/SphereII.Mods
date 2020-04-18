using DMT;
using Harmony;
using System;
using UnityEngine;

namespace SphereIICore_EntityMoveHelper
{
    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("Stop")]
    public class SphereII_EntityMoveHelper_Stop
    {
        public static void Postfix(ref EntityMoveHelper __instance)
        {
                __instance.entity.speedForward = 0f;
        }
    }

    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("DigStart")]
    public class SphereII_EntityMoveHelper_DigStart
    {
        public static bool Prefix(EntityMoveHelper __instance)
        {
            if (EntityUtilities.IsHuman(__instance.entity.entityId))
                return false;

            return true;

        }
    }

    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("StartJump")]
    public class SphereII_EntityMoveHelper_StartJump
    {
        public static bool Prefix(ref EntityMoveHelper __instance, bool calcYaw, float distance = 0f, float heightDiff = 0)
        {
            //Debug.Log("Start Jump");
            if (heightDiff == 1.4f)
            {
                EntityUtilities.Stop(__instance.entity.entityId);
                return false;
            }
            
            return true;
            
        }
    }
    //[HarmonyPatch(typeof(EntityMoveHelper))]
    //[HarmonyPatch("CheckBlockedUp")]
    //public class SphereII_EntityMoveHelper_CheckBlockedUp
    //{
    //    public static void Postfix(ref EntityMoveHelper __instance, Vector3 pos)
    //    {
    //        if (__instance.entity is EntityNPC && __instance.IsBlocked == false)
    //        {
    //            Vector3 headPosition = __instance.entity.getHeadPosition();
    //            headPosition.x = pos.x;
    //            headPosition.z = pos.z;
    //            headPosition.y -= 0.625f;
    //            Ray ray = new Ray(headPosition, Vector3.up);
    //            if (Voxel.Raycast(__instance.entity.world, ray, 1f, 1082195968, 128, 0.125f))
    //            {
    //                __instance.HitInfo = Voxel.voxelRayHitInfo.Clone();
    //                __instance.IsBlocked = true;
    //                float sqrMagnitude = (pos - __instance.HitInfo.hit.pos).sqrMagnitude;
    //                if (sqrMagnitude < __instance.blockedDistSq)
    //                {
    //                    __instance.blockedDistSq = sqrMagnitude;
    //                    __instance.obstacleCheckTickDelay = 12;
    //                    __instance.ResetStuckCheck();
    //                }
    //            }
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(EntityMoveHelper))]
    //[HarmonyPatch("CheckBlocked")]
    //public class SphereII_EntityMoveHelper_CheckBlocked
    //{
    //    public static void Postfix(ref EntityMoveHelper __instance, Vector3 pos, Vector3 endPos, int baseY)
    //    {
    //        if (__instance.entity is EntityNPC && __instance.IsBlocked == false)
    //        {
    //            __instance.IsBlocked = false;
    //            endPos.y -= 0.01f;
    //            Vector3 vector = endPos - pos;
    //            float num = vector.magnitude + 0.001f;
    //            vector *= 1f / num;
    //            Ray ray = new Ray(pos - vector * 0.375f, vector);
    //            if (num > __instance.ccRadius + 0.25f)
    //            {
    //                num = __instance.ccRadius + 0.25f;
    //            }
    //            if (baseY >= 2)
    //            {
    //                num += 0.21f;
    //            }
    //            if (Voxel.Raycast(__instance.entity.world, ray, num - 0.125f + 0.375f, 1082195968, 128, 0.125f))
    //            {
    //                if (baseY == 0 && Voxel.phyxRaycastHit.normal.y > 0.643f)
    //                {
    //                    Vector2 vector2;
    //                    vector2.x = Voxel.phyxRaycastHit.normal.x;
    //                    vector2.y = Voxel.phyxRaycastHit.normal.z;
    //                    vector2.Normalize();
    //                    Vector2 vector3;
    //                    vector3.x = vector.x;
    //                    vector3.y = vector.z;
    //                    vector3.Normalize();
    //                    Debug.Log("Vector: " + vector3.x * vector2.x + vector3.y * vector2.y + " < " + -0.7f);
    //                    if (vector3.x * vector2.x + vector3.y * vector2.y < -0.7f)
    //                    {
    //                        return;
    //                    }
    //                }

    //                __instance.HitInfo = Voxel.voxelRayHitInfo.Clone();
    //                __instance.IsBlocked = true;
    //                Vector3 a = pos - __instance.HitInfo.hit.pos;
    //                float sqrMagnitude = a.sqrMagnitude;
    //                Debug.Log(" Magnitude: " + sqrMagnitude + " Blocked distance: " + __instance.blockedDistSq);
    //                if (sqrMagnitude < __instance.blockedDistSq)
    //                {
    //                    __instance.blockedDistSq = sqrMagnitude;
    //                    float num2 = 1f / Mathf.Sqrt(sqrMagnitude);
    //                    float num3 = __instance.ccRadius + 1.1f;
    //                    __instance.tempMoveToPos = a * (num2 * num3) + __instance.HitInfo.hit.pos;
    //                    Debug.Log("Temp Move To Pos: " + __instance.tempMoveToPos);
    //                    __instance.tempMoveToPos.y = __instance.moveToPos.y;
    //                    __instance.isTempMove = true;
    //                    __instance.obstacleCheckTickDelay = 12;
    //                    __instance.ResetStuckCheck();
    //                }
    //            }
    //        }
    //    }
    //}
}
