using HarmonyLib;
using UAI;
using UnityEngine;

namespace Harmony.UtilityAI
{
    class UAITaskBasePatches
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingTask";

        public static void AddBuff( global::EntityAlive entityAlive, string buffs, global::EntityAlive other = null)
        {
            foreach (var buff in buffs.Split(','))
                entityAlive.Buffs.AddBuff(buff);
        }

        public static void RemoveBuff(global::EntityAlive entityAlive, string buffs, global::EntityAlive other = null)
        {
            foreach (var buff in buffs.Split(','))
                entityAlive.Buffs.RemoveBuff(buff);
        }

        [HarmonyPatch(typeof(UAITaskBase))]
        [HarmonyPatch("Start")]
        public class UAITaskBaseStart
        {
            public static void Postfix(UAITaskBase __instance, Context _context)
            {
                SCoreUtils.SetWeapon(_context);

                // if other is passed in, set their minevent context to other for buffs to work right.
                var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
                if ( entityAlive != null )
                        _context.Self.MinEventContext.Other = entityAlive;

                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action: {_context.ActionData.CurrentTask?.Name} {__instance.GetType()} Start: {_context.Self.EntityName} ( {_context.Self.entityId} )");
                if (__instance.Parameters.ContainsKey("OnStartAddBuffs"))
                    AddBuff(_context.Self, __instance.Parameters["OnStartAddBuffs"], entityAlive);

                if (__instance.Parameters.ContainsKey("OnStartRemoveBuffs"))
                    RemoveBuff(_context.Self, __instance.Parameters["OnStartRemoveBuffs"], entityAlive);

                if (entityAlive != null)
                {
                    entityAlive.MinEventContext.Other = _context.Self;

                    if (__instance.Parameters.ContainsKey("OnStartAddBuffsTarget"))
                        AddBuff(entityAlive, __instance.Parameters["OnStartAddBuffsTarget"], _context.Self);

                    if (__instance.Parameters.ContainsKey("OnStartRemoveBuffsTarget"))
                        RemoveBuff(entityAlive, __instance.Parameters["OnStartRemoveBuffsTarget"], _context.Self);
                }
            }
        }

        [HarmonyPatch(typeof(UAITaskBase))]
        [HarmonyPatch("Stop")]
        public class UAITaskBaseStop
        {
            public static void Postfix(UAITaskBase __instance, Context _context)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action: {_context.ActionData.CurrentTask?.Name} {__instance.GetType()} Stop: {_context.Self.EntityName} ( {_context.Self.entityId} )");

                SCoreUtils.SetWeapon(_context);

                var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);


                if (__instance.Parameters.ContainsKey("OnStopAddBuffs"))
                    AddBuff(_context.Self, __instance.Parameters["OnStopAddBuffs"], entityAlive);

                if (__instance.Parameters.ContainsKey("OnStopRemoveBuffs"))
                    RemoveBuff(_context.Self, __instance.Parameters["OnStopRemoveBuffs"], entityAlive);

                if (entityAlive != null)
                {
                    if (__instance.Parameters.ContainsKey("OnStopAddBuffsTarget"))
                        AddBuff(entityAlive, __instance.Parameters["OnStopAddBuffsTarget"], _context.Self);

                    if (__instance.Parameters.ContainsKey("OnStopRemoveBuffsTarget"))
                        RemoveBuff(entityAlive, __instance.Parameters["OnStopRemoveBuffsTarget"], _context.Self);
                }

            }
        }
    }
}