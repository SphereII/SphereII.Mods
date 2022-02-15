using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UAI;
using UnityEngine;

namespace Harmony.UtilityAI
{
    public class UAIBasePatches
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILogging";

        [HarmonyPatch(typeof(UAIBase))]
        [HarmonyPatch("updateAction")]
        public static class UAIBasePatch
        {
            private static bool Prefix(Context _context)
            {
                if (_context.ActionData.CurrentTask == null)
                {
                    return false;
                }
                if (!_context.ActionData.Initialized)
                {
                    _context.ActionData.CurrentTask.Init(_context);
                }
                if (!_context.ActionData.Started)
                {
                    _context.ActionData.CurrentTask.Start(_context);
                }
                if (_context.ActionData.Executing)
                {
                    _context.ActionData.CurrentTask.Update(_context);
                    return false;
                }
                _context.ActionData.CurrentTask.Reset(_context);
                if (_context.ActionData.TaskIndex + 1 < _context.ActionData.Action.GetTasks().Count)
                {
                    _context.ActionData.TaskIndex = _context.ActionData.TaskIndex + 1;
                    return false;
                }
                // Reset the task index to 0 if there's nothing else to do here. This resets the task index.
                _context.ActionData.TaskIndex = 0;
                return false;
            }
        }


        [HarmonyPatch(typeof(UAIBase))]
        [HarmonyPatch("Update")]
        public static class UAIBase_Update
        {
            public static bool Prefix(Context _context)
            {
                if (_context.Self.IsSleeping) return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(UAIBase))]
        [HarmonyPatch("chooseAction")]
        public static class UAIBase_considerAction
        {
            private static void AddEntityTargetsToConsider(Context context)
            {
                if (context.ConsiderationData.EntityTargets == null)
                {
                    context.ConsiderationData.EntityTargets = new List<Entity>();
                }

                context.ConsiderationData.EntityTargets.Clear();

                // This is needed for UAIFilterIsSelf. In theory it should be possible to just pass
                // null as the first argument of GetEntitiesInBounds, but I looked in that code,
                // and I suspect there might be unintended consequences to doing that.
                context.ConsiderationData.EntityTargets.Add(context.Self);

                var revengeTarget = context.Self.GetRevengeTarget();
                if (revengeTarget != null)
                    context.ConsiderationData.EntityTargets.Add(revengeTarget);

                var seeDistance = context.Self.GetSeeDistance();
                context.ConsiderationData.EntityTargets.AddRange(
                    context.Self.world.GetEntitiesInBounds(
                        context.Self,
                        BoundsUtils.ExpandBounds(
                            context.Self.boundingBox,
                            seeDistance,
                            seeDistance,
                            seeDistance)));

                if (context.ConsiderationData.EntityTargets.Count > 1)
                {
                    context.ConsiderationData.EntityTargets.Sort(
                        new UAIUtils.NearestEntitySorter(context.Self));
                }
            }

            private static void AddWaypointTargetsToConsider(Context context)
            {
                if (context.ConsiderationData.WaypointTargets == null)
                {
                    context.ConsiderationData.WaypointTargets = new List<Vector3>();
                }

                context.ConsiderationData.WaypointTargets.Clear();

                if (context.ConsiderationData.WaypointTargets.Count > 1)
                {
                    context.ConsiderationData.WaypointTargets.Sort(new UAIUtils.NearestWaypointSorter(context.Self));
                }

                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Scanned WayPoints {context.ConsiderationData.WaypointTargets.Count}");
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"Scanning WayPoints...");
            }

            public static bool Prefix(Context _context)
            {
                float highScore = 0f;
                AddEntityTargetsToConsider(_context);
                AddWaypointTargetsToConsider(_context);

                UAIAction chosenAction = null;
                object chosenTarget = null;
                for (int i = 0; i < _context.AIPackages.Count; i++)
                {
                    string pkg = _context.AIPackages[i];

                    if (!UAIBase.AIPackages.ContainsKey(pkg))
                        continue;

                    var score = UAIBase.AIPackages[pkg].DecideAction(
                        _context,
                        out var action,
                        out var target) * UAIBase.AIPackages[pkg].Weight;

                    // If two actions' scores are equal, the last action wins.
                    // Changing the test to <= would mean the first action wins.
                    if (score < highScore)
                        continue;

                    chosenAction = action;
                    chosenTarget = target;
                    highScore = score;
                }

                // The NPC should stick with the current action if none of the packages found an
                // action to perform, or if the winning action is already being performed on the
                // winning target.
                if (chosenAction == null ||
                    (_context.ActionData.Action == chosenAction &&
                     _context.ActionData.Target == chosenTarget))
                    return false;

                if (_context.ActionData.CurrentTask != null)
                {
                    if (_context.ActionData.Started)
                    {
                        _context.ActionData.CurrentTask.Stop(_context);
                    }
                    if (_context.ActionData.Initialized)
                    {
                        _context.ActionData.CurrentTask.Reset(_context);
                    }
                }

                _context.ActionData.Action = chosenAction;
                _context.ActionData.Target = chosenTarget;
                _context.ActionData.TaskIndex = 0;

                return false;
            }
        }
    }
}