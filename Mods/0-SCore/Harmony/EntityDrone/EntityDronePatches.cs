using HarmonyLib;
using System;

namespace Harmony.EntityDrone
{
    /// <summary>
    /// Harmony patches of <see cref="global::EntityDrone"/>.
    /// </summary>
    public class EntityDronePatches
    {
        /// <summary>
        /// Harmony patch of <see cref="global::EntityDrone.isAlly(global::EntityAlive)"/>
        /// so that it will not produce an NRE if the target is null.
        /// </summary>
        [HarmonyPatch(typeof(global::EntityDrone))]
        [HarmonyPatch("isAlly")]
        public class EntityDrone_isAlly
        {
            /// <summary>
            /// The _target argument was cast to an <see cref="EntityPlayer"/> before this method
            /// was called, so it will be null for ranged weapon attacks from any NPC.
            /// This will cause a <see cref="NullReferenceException"/> in the original method,
            /// so we need to skip it entirely in this case.
            /// </summary>
            /// <param name="__result"></param>
            /// <param name="_target"></param>
            /// <returns></returns>
            public static bool Prefix(ref bool __result, global::EntityAlive _target)
            {
                if (_target == null)
                {
                    // If it's not a player, consider it an ally so it can't damage the drone.
                    // This is consistent with vanilla zombies not being able to damage it.
                    __result = true;
                    return false;
                }

                return true;
            }
        }
    }
}
