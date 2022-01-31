using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace SCore.Harmony.MiniTurret
{
    [HarmonyPatch(typeof(AutoTurretFireController))]
    [HarmonyPatch("shouldIgnoreTarget")]
    public class AutoTurretFireControllerShouldIgnoreTarget
    {
        private static void Postfix(AutoTurretFireController __instance, ref bool __result, Entity _target)
        {
            // The only time we need to override the result is if the target is an EntityNPC.
            // We need to decide if they're a "stranger" and treat them accordingly.
            if (_target is EntityNPC)
            {
                // Don't target dead things, even NPCs.
                if (_target.IsDead())
                    return;

                var owner = GameManager.Instance.World.GetEntity(__instance.TileEntity.OwnerEntityID)
                    as EntityAlive;
                if (owner == null)
                    return;

                var targetAsStranger = __instance.TileEntity.TargetStrangers &&
                                       EntityTargetingUtilities.IsEnemy(owner, _target);
                __result = !targetAsStranger;

                /*
                 * Note from Karl: Targeting your followers as allies doesn't work properly.
                 * Followers won't take damage, so the turret will just keep wasting ammo on them. 
                 * I'm leaving in commented-out code in case people, for some weird reason, want
                 * that as a feature. If so, we will need to fix it so allies can take damage.
                 */
                //// Need to see if the target is an ally of the owner, not the other way around (IsAlly is not reciprocal)
                //var targetAsAlly = __instance.TileEntity.TargetAllies && EntityTargetingUtilities.IsAlly(_target, owner);
                //__result = !(targetAsStranger || targetAsAlly);

                // Even if they can be targeted, ignore them if they're not in our field of view.
                if (!__result)
                {
                    // The dot product of two vectors: the vector representing the direction to
                    // the target, and the vector representing the view cone's forward direction.
                    var dot = Vector3.Dot(
                        _target.position - __instance.TileEntity.ToWorldPos().ToVector3(),
                        __instance.Cone.transform.forward);
                    
                    // If the dot product is greater than zero, the angle between the vectors is
                    // greater than 90 degrees, so it's outside the field of view.
                    if (dot > 0f)
                    {
                        __result = true;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(MiniTurretFireController))]
    [HarmonyPatch("shouldIgnoreTarget")]
    public class MiniTurretFireControllerShouldIgnoreTarget
    {
        private static void Postfix(MiniTurretFireController __instance, ref bool __result, Entity _target)
        {
            // The only time we need to override the result is if the target is an EntityNPC.
            // We need to decide if they're a "stranger" and treat them accordingly.
            if (_target is EntityNPC)
            {
                // Don't target dead things, even NPCs.
                if (_target.IsDead())
                    return;

                var owner = GameManager.Instance.World.GetEntity(__instance.entityTurret.belongsPlayerId)
                    as EntityAlive;
                if (owner == null)
                    return;

                __result = !(__instance.entityTurret.TargetStrangers &&
                             EntityTargetingUtilities.IsEnemy(owner, _target));

                if (!__result)
                {
                    // Even if they can be targeted, ignore them if they're not in our field of view.
                    Vector3 forward = __instance.transform.forward;
                    if (__instance.Cone != null)
                    {
                        forward = __instance.Cone.transform.forward;
                    }
                    if (Vector3.Dot(_target.position - __instance.entityTurret.position, forward) > 0f)
                    {
                        __result = true;
                    }

                    // Even if they are in our field of view, ignore them if we can't hit them.
                    if (!__result)
                    {
                        float _yaw = 0f;
                        float _pitch = 0f;
                        Vector3 targetPos = Vector3.zero;
                        MethodInfo canHitEntity = typeof(MiniTurretFireController).GetMethod("canHitEntity", BindingFlags.NonPublic | BindingFlags.Instance);
                        if (canHitEntity != null)
                        {
                            var canHit = (bool)canHitEntity.Invoke(__instance, new object[] { _target, _yaw, _pitch, targetPos });
                            __result = !canHit;
                        }
                    }
                }
            }
        }
    }
}
