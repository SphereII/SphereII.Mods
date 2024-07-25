using HarmonyLib;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using UAI;
namespace Harmony.UtilityAI
{
    public class Debugging
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILogging";

        private static readonly bool LoggingEnabled = AdvLogging.LogEnabled(AdvFeatureClass, Feature);
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

        /// <summary>
        /// This patch adds the package to the dictionary of stored attributes in SCoreUtils.
        /// We will need those attributes later to see if the user specified package filters.
        /// </summary>
        [HarmonyPatch(typeof(UAIFromXml))]
        [HarmonyPatch("parseAIPackageNode")]
        public static class UAIFromXml_parseAIPackageNode
        {
            public static void Postfix(XElement _element)
            {
                if (_element == null)
                    return;

                if (!_element.HasAttribute("name"))
                    return;

                var name = _element.GetAttribute("name");

                if (UAIBase.AIPackages.TryGetValue(name, out var package))
                {
                    SCoreUtils.StoreAttributes(package, _element);
                }
            }
        }

        /// <summary>
        /// This patch adds the action to the dictionary of stored attributes in SCoreUtils.
        /// We will need those attributes later to see if the user specified action filters.
        /// </summary>
        [HarmonyPatch(typeof(UAIFromXml))]
        [HarmonyPatch("parseActionNode")]
        public static class UAIFromXml_parseActionNode
        {
            public static void Postfix(UAIPackage _package, XElement _element)
            {
                if (_element == null || _package == null)
                    return;

                if (!_element.HasAttribute("name"))
                    return;

                var name = _element.GetAttribute("name");

                var actions = _package.GetActions();
                if (actions == null)
                    return;

                for (int i = 0; i < actions.Count; i++)
                {
                    var action = actions[i];
                    if (action.Name == name)
                    {
                        SCoreUtils.StoreAttributes(_package, action, _element);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// <para>
        /// This patch replaces the DecideAction method with one that will skip targets which are
        /// not passed through the package and/or action filters.
        /// </para>
        /// <para>
        /// Note that filters are inclusive, not exclusive; and that targets may be filtered by
        /// packages and actions, and must pass through both filters.
        /// </para>
        /// </summary>
        [HarmonyPatch(typeof(UAI.UAIPackage))]
        [HarmonyPatch("DecideAction")]
        public class UAIPackage_DecideAction
        {

            // for out parameters, use ref instead of out. ref the __result otherwise the default of 0 is returned.
            private static bool Prefix(UAI.UAIPackage __instance, ref float __result, Context _context, ref UAIAction _chosenAction, ref object _chosenTarget, List<UAIAction> ___actionList)
            {
                if (LoggingEnabled)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, "\n**** START ************************ ");
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"UAIPackage: {_context.Self.entityId} {_context.Self.EntityName}");
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"EntityTargets: {_context.ConsiderationData.EntityTargets.Count} WayPoint: {_context.ConsiderationData.WaypointTargets.Count}");
                }

                float highScore = 0f;
                _chosenAction = null;
                _chosenTarget = null;

                int availableActions = 0;
                int actionRans = 0;


                var packageEntityFilter = SCoreUtils.GetEntityFilter(__instance, null, _context.Self);
                var packageWaypointFilter = SCoreUtils.GetWaypointFilter(__instance, null, _context.Self);
                if (LoggingEnabled)
                {
                    if (packageEntityFilter != null)
                        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Package {__instance.Name} entity filter: {packageEntityFilter}");
                    if (packageWaypointFilter != null)
                        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Package {__instance.Name} waypoint filter: {packageWaypointFilter}");
                }
                for (int i = 0; i < ___actionList.Count; i++)
                {
                    var actionEntityFilter = SCoreUtils.GetEntityFilter(__instance, ___actionList[i], _context.Self);
                    var actionWaypointFilter = SCoreUtils.GetWaypointFilter(__instance, ___actionList[i], _context.Self);
                    if (LoggingEnabled)
                    {
                        if (actionEntityFilter != null)
                            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action {___actionList[i].Name} entity filter: {actionEntityFilter}");
                        if (actionWaypointFilter != null)
                            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Action {___actionList[i].Name} waypoint filter: {actionWaypointFilter}");
                    }

                    int entitiesConsidered = 0;
                    int targetIndex = 0;
                    while (targetIndex < _context.ConsiderationData.EntityTargets.Count && entitiesConsidered <= UAIBase.MaxEntitiesToConsider)
                    {
                        var target = _context.ConsiderationData.EntityTargets[targetIndex];
                        availableActions++;

                        if (packageEntityFilter != null && !packageEntityFilter.Test(target))
                        {
                            if (LoggingEnabled)
                                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Target entity excluded by package filter: {target}");
                            targetIndex++;
                            continue;
                        }

                        if (actionEntityFilter != null && !actionEntityFilter.Test(target))
                        {
                            if (LoggingEnabled)
                                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Target entity excluded by action filter: {target}");
                            targetIndex++;
                            continue;
                        }

                        float score = ___actionList[i].GetScore(_context, target, 0f);
                        if (score > highScore)
                        {
                            highScore = score;
                            _chosenAction = ___actionList[i];
                            _chosenTarget = _context.ConsiderationData.EntityTargets[targetIndex];
                        }
                        entitiesConsidered++;
                        targetIndex++;
                        actionRans++;
                    }

                    int waypointsConsidered = 0;
                    int waypointIndex = 0;
                    while (waypointIndex < _context.ConsiderationData.WaypointTargets.Count && waypointsConsidered <= UAIBase.MaxWaypointsToConsider)
                    {
                        var target = _context.ConsiderationData.WaypointTargets[waypointIndex];
                        availableActions++;
                        if (packageWaypointFilter != null && !packageWaypointFilter.Test(target))
                        {
                            if (LoggingEnabled)
                                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Target waypoint excluded by package filter: {target}");
                            waypointIndex++;
                            continue;
                        }

                        if (actionWaypointFilter != null && !actionWaypointFilter.Test(target))
                        {
                            if (LoggingEnabled)
                                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Target waypoint excluded by action filter: {target}");
                            waypointIndex++;
                            continue;
                        }

                        float score2 = ___actionList[i].GetScore(_context, target, 0f);
                        if (score2 > highScore)
                        {
                            highScore = score2;
                            _chosenAction = ___actionList[i];
                            _chosenTarget = target;
                        }
                        waypointsConsidered++;
                        waypointIndex++;
                        actionRans++;
                    }
                }

                if (LoggingEnabled)
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{_context.Self.EntityName} : I had a total of {availableActions} actions available, but I only evaluated {actionRans}");

                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"************* Final Decision: {_context.Self.EntityName} ( {_context.Self.entityId} ) ********************* ");
                    if (_chosenAction != null)
                        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Chosen Action: {_chosenAction.Name} Score {highScore}");
                    else
                        AdvLogging.DisplayLog(AdvFeatureClass, Feature, " No Chosen action!");
                    if (_chosenTarget != null)
                        AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Chosen Target: {_chosenTarget} Score: {highScore}");
                    else
                        AdvLogging.DisplayLog(AdvFeatureClass, Feature, " No Chosen target!");
                    AdvLogging.DisplayLog(AdvFeatureClass, Feature, "********************************** ");
                }
                __result = highScore;
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