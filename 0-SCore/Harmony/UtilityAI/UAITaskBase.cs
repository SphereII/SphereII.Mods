using HarmonyLib;
using UAI;
using UnityEngine;

namespace Harmony.UtilityAI
{
    class UAITaskBasePatches
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingTask";

        [HarmonyPatch(typeof(UAITaskBase))]
        [HarmonyPatch("Start")]
        public class UAITaskBaseStart
        {
            public static void Postfix(UAITaskBase __instance, Context _context)
            {
                SCoreUtils.SetWeapon(_context);

                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action: {_context.ActionData.CurrentTask?.Name} {__instance.GetType()} Start: {_context.Self.EntityName} ( {_context.Self.entityId} )");
                if (__instance.Parameters.ContainsKey("OnStartAddBuffs"))
                {
                    foreach (var buff in __instance.Parameters["OnStartAddBuffs"].Split(','))
                    {
                        Debug.Log($"OnStartAddBuffs() Adding: {buff}");
                        _context.Self.Buffs.AddBuff(buff);
                    }
                }

                if (__instance.Parameters.ContainsKey("OnStartRemoveBuffs"))
                {
                    foreach (var buff in __instance.Parameters["OnStartRemoveBuffs"].Split(','))
                        _context.Self.Buffs.RemoveBuff(buff);
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
                if (__instance.Parameters.ContainsKey("OnStopAddBuffs"))
                {
                    foreach (var buff in __instance.Parameters["OnStopAddBuffs"].Split(','))
                    {
                        Debug.Log($"OnStopAddBuffs() Adding: {buff}");
                        _context.Self.Buffs.AddBuff(buff);
                    }
                }

                if (__instance.Parameters.ContainsKey("OnStopRemoveBuffs"))
                {
                    foreach (var buff in __instance.Parameters["OnStopRemoveBuffs"].Split(','))
                    {
                        Debug.Log($"OnStopRemoveBuffs() Adding: {buff}");
                        _context.Self.Buffs.RemoveBuff(buff);
                    }
                }
            }
        }
    }
}