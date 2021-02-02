using HarmonyLib;
using UnityEngine;


/**
 * EntityNPCSpeedFix
 *
 * This class includes a Harmony patches to EntityNPC to adjust its update Speed Forward, which hasn't gotten the same updates as EntityAlive.
 */
class EntityNPCSpeedFix
{

    private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";
    private static readonly string Feature = "NPCSpeedFix";

    [HarmonyPatch(typeof(EntityNPC))]
    [HarmonyPatch("updateSpeedForwardAndStrafe")]
    public class SphereII_UpdateSpeedForward
    {
        public static void SetMovementState(EntityNPC __instance)
        {
            float num = __instance.speedStrafe;
            if (num >= 1234f)
            {
                num = 0f;
            }
            float num2 = __instance.speedForward * __instance.speedForward + num * num;
            __instance.MovementState = ((num2 > __instance.moveSpeedAggro * __instance.moveSpeedAggro) ? 3 : ((num2 > __instance.moveSpeed * __instance.moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)));
        }

        public static bool Prefix(EntityNPC __instance, ref Vector3 _dist, float _partialTicks)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (__instance.isEntityRemote && _partialTicks > 1f)
            {
                _dist /= _partialTicks;
            }
            __instance.speedForward *= 0.5f;
            __instance.speedStrafe *= 0.5f;
            __instance.speedVertical *= 0.5f;
            if (Mathf.Abs(_dist.x) > 0.001f || Mathf.Abs(_dist.z) > 0.001f)
            {
                float num = Mathf.Sin(-__instance.rotation.y * 3.14159274f / 180f);
                float num2 = Mathf.Cos(-__instance.rotation.y * 3.14159274f / 180f);
                __instance.speedForward += num2 * _dist.z - num * _dist.x;
                __instance.speedStrafe += num2 * _dist.x + num * _dist.z;
            }
            if (Mathf.Abs(_dist.y) > 0.001f)
            {
                __instance.speedVertical += _dist.y;
            }
            SetMovementState(__instance);
            return false;
        }
    }
}

