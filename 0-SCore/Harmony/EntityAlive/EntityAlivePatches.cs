using HarmonyLib;
using System;
using System.IO;

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

        //[HarmonyPatch(typeof(global::EntityAlive))]
        //[HarmonyPatch("IsAttackValid")]
        //public class EntityAliveIsAttackValid
        //{
        //    public static void Postfix(bool __result, ref global::EntityAlive __instance)
        //    {
        //        var entityTarget = EntityUtilities.GetAttackOrRevengeTarget(__instance.entityId);
        //        if (entityTarget == null)
        //            return;

        //        // Face and rotate the target.
        //        __instance.SetLookPosition(entityTarget.getHeadPosition());
        //        __instance.RotateTo(entityTarget, 30f, 30f);

        //    }
        //}
    }
}