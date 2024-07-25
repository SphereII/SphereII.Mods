using System.Collections.Generic;
using HarmonyLib;
using UAI;
using UnityEngine;

namespace Harmony.UtilityAI
{
    class UAITaskBasePatches
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingTask";

        public static Dictionary<int, SCoreQueue<UAITaskBase>> TaskHistory =
            new Dictionary<int, SCoreQueue<UAITaskBase>>();

        private static void AddBuff( global::EntityAlive entityAlive, string buffs)
        {
            foreach (var buff in buffs.Split(','))
                entityAlive.Buffs.AddBuff(buff);
        }

        private static void RemoveBuff(global::EntityAlive entityAlive, string buffs)
        {
            if (string.IsNullOrEmpty(buffs)) return;
            
            foreach (var buff in buffs.Split(','))
            {
                entityAlive.Buffs.RemoveBuff(buff);
            }
        }

        [HarmonyPatch(typeof(UAITaskBase))]
        [HarmonyPatch("Start")]
        public class UAITaskBaseStart
        {
            public static void Postfix(UAITaskBase __instance, Context _context)
            {
                // if (!TaskHistory.ContainsKey(_context.Self.entityId))
                // {
                //     TaskHistory.Add(_context.Self.entityId, new SCoreQueue<UAITaskBase>());
                // }
                // TaskHistory[_context.Self.entityId].Add(__instance);
                
                SCoreUtils.SetWeapon(_context);
                SCoreUtils.DisplayDebugInformation(_context, "Starting");

                // if other is passed in, set their minevent context to other for buffs to work right.
                var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
                if ( entityAlive != null )
                        _context.Self.MinEventContext.Other = entityAlive;

                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action: {_context.ActionData.CurrentTask?.Name} {__instance.GetType()} Start: {_context.Self.EntityName} ( {_context.Self.entityId} )");
                if (__instance.Parameters.ContainsKey("OnStartAddBuffs"))
                    AddBuff(_context.Self, __instance.Parameters["OnStartAddBuffs"]);

                if (__instance.Parameters.ContainsKey("OnStartRemoveBuffs"))
                    RemoveBuff(_context.Self, __instance.Parameters["OnStartRemoveBuffs"]);

                if (entityAlive == null) return;
                entityAlive.MinEventContext.Other = _context.Self;

                if (__instance.Parameters.ContainsKey("OnStartAddBuffsTarget"))
                    AddBuff(entityAlive, __instance.Parameters["OnStartAddBuffsTarget"]);

                if (__instance.Parameters.ContainsKey("OnStartRemoveBuffsTarget"))
                    RemoveBuff(entityAlive, __instance.Parameters["OnStartRemoveBuffsTarget"]);
            }
        }
        [HarmonyPatch(typeof(UAIBase))]
        [HarmonyPatch("updateAction")]
        public class UAITaskBaseUpdateAction
        {
            public static void Postfix(UAITaskBase __instance, Context _context)
            {
                var message = "";
                if (!_context.ActionData.Initialized)
                {
                    message = "Intializing";
                }
                if (!_context.ActionData.Started)
                {
                    message = "Starting";
                }
                if (_context.ActionData.Executing)
                {
                    message = "Executing";
                    
                }
                SCoreUtils.DisplayDebugInformation(_context, message);
            }
        }


        [HarmonyPatch(typeof(UAITaskBase))]
        [HarmonyPatch("Stop")]
        public class UAITaskBaseStop
        {
            public static void Postfix(UAITaskBase __instance, Context _context)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action: {_context.ActionData.CurrentTask?.Name} {__instance.GetType()} Stop: {_context.Self.EntityName} ( {_context.Self.entityId} )");

                SCoreUtils.DisplayDebugInformation(_context, "Stopping");

                SCoreUtils.SetWeapon(_context);

                var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);


                if (__instance.Parameters.ContainsKey("OnStopAddBuffs"))
                    AddBuff(_context.Self, __instance.Parameters["OnStopAddBuffs"]);

                if (__instance.Parameters.ContainsKey("OnStopRemoveBuffs"))
                    RemoveBuff(_context.Self, __instance.Parameters["OnStopRemoveBuffs"]);

                if (entityAlive == null) return;
                if (__instance.Parameters.ContainsKey("OnStopAddBuffsTarget"))
                    AddBuff(entityAlive, __instance.Parameters["OnStopAddBuffsTarget"]);

                if (__instance.Parameters.ContainsKey("OnStopRemoveBuffsTarget"))
                    RemoveBuff(entityAlive, __instance.Parameters["OnStopRemoveBuffsTarget"]);

            }
        }
    }
}