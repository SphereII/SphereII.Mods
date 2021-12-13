using HarmonyLib;
using System.Collections.Generic;
using UAI;
using UnityEngine;
namespace Harmony.UtilityAI
{
    public class Debugging
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILogging";

        //[HarmonyPatch(typeof(UAI.UAIAction))]
        //[HarmonyPatch("GetScore")]
        //public class UAIAction_GetScore
        //{
        //    private static bool Prefix(UAIAction __instance, Context _context, object _target, float min = 0f)
        //    {
        //        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
        //            return true;

        //        var considerations = __instance.GetConsiderations();
        //        var tasks = __instance.GetTasks();

        //        float num = 1f;
        //        if (considerations.Count == 0)
        //            return true;
        //        if (tasks.Count == 0)
        //            return true;

        //        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Start Evaluation {__instance.Name} for {_context.Self.EntityName} ( {_context.Self.entityId} ) :: Action {__instance.Name} Weight: {__instance.Weight} ");

        //        global::EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_target);
        //        if (entityAlive != null)
        //            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"\tTarget Entity: {entityAlive.EntityName} {entityAlive.entityId}");

        //        for (int i = 0; i < considerations.Count; i++)
        //        {
        //            if (0f > num || num < min)
        //            {
        //                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"\tConsiderations num {num} falls below minimum or 0: {min}");
        //                break;
        //            }

        //            num *= considerations[i].ComputeResponseCurve(considerations[i].GetScore(_context, _target));
        //            AdvLogging.DisplayLog(AdvFeatureClass, Feature, string.Format("\t{0} {1} {2} : Consideration Score {3},  Cumulative Score: {4}", new object[]
        //            {
        //                __instance.Name,
        //                __instance.GetTasks()[0].Name,
        //                considerations[i].Name,
        //                considerations[i].GetScore(_context, _target),
        //                num
        //            }));
        //        }

        //        float result = (num + (1f - num) * (float)(1 - 1 / considerations.Count) * num) * __instance.Weight;
        //        AdvLogging.DisplayLog(AdvFeatureClass, Feature, string.Format("\tFinal Score for Action: {0}", result));

        //        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"End Evaluation {__instance.Name} ");
        //        return true;
        //    }
        //}


        [HarmonyPatch(typeof(UAI.UAIPackage))]
        [HarmonyPatch("DecideAction")]
        public class UAIPackage_DecideAction
        {
            // for out parameters, use ref instead of out. ref the __result otherwise the default of 0 is returned.
            private static bool Prefix(UAI.UAIPackage __instance, ref float __result, Context _context, ref  UAIAction _chosenAction, ref object _chosenTarget, List<UAIAction> ___actionList)
            {
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, "\n**** START ************************ ");
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"UAIPackage: {_context.Self.entityId} {_context.Self.EntityName}");
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"EntityTargets: {_context.ConsiderationData.EntityTargets.Count} WayPoint: {_context.ConsiderationData.WaypointTargets.Count}");
                float num = 0f;
                _chosenAction = null;
                _chosenTarget = null;
                for (int i = 0; i < ___actionList.Count; i++)
                {
                    int num2 = 0;
                    int num3 = 0;
                    while (num3 < _context.ConsiderationData.EntityTargets.Count && num2 <= UAIBase.MaxEntitiesToConsider)
                    {
                        float score = ___actionList[i].GetScore(_context, _context.ConsiderationData.EntityTargets[num3], 0f);
                        if (score > num)
                        {
                            num = score;
                            _chosenAction = ___actionList[i];
                            _chosenTarget = _context.ConsiderationData.EntityTargets[num3];
                        }
                        num2++;
                        num3++;
                    }
                    int num4 = 0;
                    while (num4 < _context.ConsiderationData.WaypointTargets.Count && num4 <= UAIBase.MaxWaypointsToConsider)
                    {
                        float score2 = ___actionList[i].GetScore(_context, _context.ConsiderationData.WaypointTargets[num4], 0f);
                        if (score2 > num)
                        {
                            num = score2;
                            _chosenAction = ___actionList[i];
                            _chosenTarget = _context.ConsiderationData.WaypointTargets[num4];
                        }
                        num4++;
                    }
                }


                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"************* Final Decision: {_context.Self.EntityName} ( {_context.Self.entityId} ) ********************* ");
                if (_chosenAction != null)
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Chosen Action: {_chosenAction.Name} Score {num}");
                else
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, " No Chosen action!");
                if (_chosenTarget != null)
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Chosen Target: {_chosenTarget} Score: {num}");
                else
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, " No Chosen target!");
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, "********************************** ");

                __result = num;
                return false;
            }

            //private static void Postfix(UAI.UAIPackage __instance, Context _context, UAIAction _chosenAction, object _chosenTarget)
            //{
            //    if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            //        return;

            //    if (_chosenAction != null)
            //        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Chosen Action: {_chosenAction.Name}");
            //    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"UAIPackage: {_context.Self.entityId} {_context.Self.EntityName}");
            //    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"EntityTargets: {_context.ConsiderationData.EntityTargets.Count} WayPoint: {_context.ConsiderationData.WaypointTargets.Count}");
            //    AdvLogging.DisplayLog(AdvFeatureClass, Feature, "**** END ************************ \n");
            //}
        }
    }
}