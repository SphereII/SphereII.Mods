using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace Harmony.EntityAlive
{
    /**
     * SCoreEntityAlive_Patches
     * 
     * This class includes a Harmony patch to allow Entity Alive's to save and read their faction ID, allowing persistence across game loads.
     */
    internal class EntityAlivePatches
    {
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("Write")]
        public class Write
        {
            public static void Postfix(global::EntityAlive __instance, BinaryWriter _bw)
            {
                _bw.Write(__instance.factionId);
            }
        }

        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("Read")]
        public class Read
        {
            public static void Postfix(ref global::EntityAlive __instance, BinaryReader _br)
            {
                try
                {
                    __instance.factionId = _br.ReadByte();
                }
                catch (Exception)
                {
                    // Fail safe for first load up
                }
            }
        }

        /// <summary>
        /// This is a patch to SetSleeperActive that will wake NPCs in "active" sleeper volumes.
        /// </summary>
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("SetSleeperActive")]
        public class SetSleeperActive
        {
            public static void Postfix(ref global::EntityAlive __instance)
            {
                // If we want to also verify the entity is using UAI, check this value:
                // EntityClass.list[__instance.entityClass].UseAIPackages
                if (__instance is EntityAliveSDX || __instance is EntityEnemySDX)
                {
                    __instance.ConditionalTriggerSleeperWakeUp();
                }
            }
        }

        /// <summary>
        /// Postfix for <see cref="global::EntityAlive.PostInit"/> that adds a read-only cvar
        /// containing the entity ID.
        /// </summary>
        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.PostInit))]
        public static class EntityAlivePostInit
        {
            [HarmonyPostfix]
            public static void Postfix(global::EntityAlive __instance) {
                __instance.SetCVar("_entityId", __instance.entityId);

            }
        }

        /// <summary>
        /// Postfix for <see cref="global::EntityAlive.SetAttackTarget"/> that adds a read-only
        /// cvar containing the attack target's entity ID.
        /// </summary>
        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.SetAttackTarget))]
        public static class EntityAliveSetAttackTarget
        {
            [HarmonyPostfix]
            public static void Postfix(global::EntityAlive __instance)
            {
                __instance.SetCVar("_attackTargetId", __instance.GetAttackTarget()?.entityId ?? 0);
            }
        }

        /// <summary>
        /// Postfix for <see cref="global::EntityAlive.SetRevengeTarget"/> that adds a read-only
        /// cvar containing the revenge target's entity ID.
        /// </summary>
        [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.SetRevengeTarget))]
        public static class EntityAliveSetRevengeTarget
        {
            [HarmonyPostfix]
            public static void Postfix(global::EntityAlive __instance)
            {
                __instance.SetCVar("_revengeTargetId", __instance.GetRevengeTarget()?.entityId ?? 0);
            }
        }
    }
}