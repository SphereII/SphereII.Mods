using HarmonyLib;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;
using UAI;

public class UtilityAIPatches
{
    private const BindingFlags _NonPublicStaticFlags = BindingFlags.Static | BindingFlags.NonPublic;

    /// <summary>
    /// Helper method to get the highest possible score for a package.
    /// The package's highest possible score is the highest weight out of any of its actions,
    /// multiplied by the package's weight.
    /// </summary>
    /// <param name="package"></param>
    /// <returns></returns>
    public static float GetHighestPossibleScore(UAIPackage package)
    {
        if (package == null)
            return 0f;

        var actions = package.GetActions();

        if (actions == null || actions.Count == 0)
            return 0f;

        return package.Weight * actions.Max(a => a.Weight);
    }

    [HarmonyPatch(typeof(UAIFromXml))]
    [HarmonyPatch("parseAIPackagesNode")]
    public class UAIFromXml_parseAIPackagesNode
    {
        /// <summary>
        /// This postfix method allows modders to specify the maximum numbers of
        /// entities and/or waypoints that should be considered for actions.
        /// In this implementation, the specified maximums cannot go below the existing maximums.
        /// (Should this be changed?)
        /// </summary>
        /// <example>
        /// Set the maximum entities to consider to 30, and the maximum waypoints to consider
        /// to 10.
        /// <code>
        /// &lt;ai_packages max_entities="30" max_waypoints="10>
        /// </code>
        /// </example>
        /// <param name="_element"></param>
        static void Postfix(XmlElement _element)
        {
            if (_element.HasAttribute("max_entities"))
            {
                int maxEntitiesToConsider = StringParsers.ParseSInt32(
                    _element.GetAttribute("max_entities"),
                    0,
                    -1,
                    NumberStyles.Integer);
                UAIBase.MaxEntitiesToConsider = Utils.FastMax(
                    UAIBase.MaxEntitiesToConsider,
                    maxEntitiesToConsider);
            }

            if (_element.HasAttribute("max_waypoints"))
            {
                int maxWaypointsToConsider = StringParsers.ParseSInt32(
                    _element.GetAttribute("max_waypoints"),
                    0,
                    -1,
                    NumberStyles.Integer);
                UAIBase.MaxWaypointsToConsider = Utils.FastMax(
                    UAIBase.MaxWaypointsToConsider,
                    maxWaypointsToConsider);
            }
        }
    }

    [HarmonyPatch(typeof(UAIBase))]
    [HarmonyPatch("chooseAction")]
    public class UAIBase_chooseAction
    {
        /// <summary>
        /// This prefix method replaces UAIBase.chooseAction in order to fix a bug,
        /// and to introduce a "fail fast" mechanism for efficiency (hopefully).
        /// </summary>
        /// <param name="_context"></param>
        /// <returns></returns>
        public static bool Prefix(Context _context)
        {
            float highScore = 0f;

            _context.ConsiderationData.EntityTargets.Clear();
            _context.ConsiderationData.WaypointTargets.Clear();

            // These are private static methods so we need to call them using reflection
            var addEntityTargetsToConsider = typeof(UAIBase).GetMethod(
                "addEntityTargetsToConsider",
                _NonPublicStaticFlags);
            var addWaypointTargetsToConsider = typeof(UAIBase).GetMethod(
                "addWaypointTargetsToConsider",
                _NonPublicStaticFlags);
            addEntityTargetsToConsider.Invoke(null, new object[] { _context });
            addWaypointTargetsToConsider.Invoke(null, new object[] { _context });

            // Sort AIPackages according to each package's highest possible score, descending.
            // (If there is only one package, no sorting is needed.)
            if (_context.AIPackages.Count > 0)
            {
                _context.AIPackages.Sort((a, b) =>
                {
                    if (!UAIBase.AIPackages.ContainsKey(a))
                        return 1; // a should go last
                    if (!UAIBase.AIPackages.ContainsKey(b))
                        return -1; // b should go last

                    var aScore = UtilityAIPatches.GetHighestPossibleScore(UAIBase.AIPackages[a]);
                    var bScore = UtilityAIPatches.GetHighestPossibleScore(UAIBase.AIPackages[b]);
                    return bScore.CompareTo(aScore);
                });
            }

            for (var i = 0; i < _context.AIPackages.Count; i++)
            {
                if (!UAIBase.AIPackages.ContainsKey(_context.AIPackages[i]))
                    continue;

                var package = UAIBase.AIPackages[_context.AIPackages[i]];

                // If the current high score is greater than the highest score this package can
                // produce, it's also higher than any of the remaining packages, so we're done.
                // (If there is only one package, this test is not needed.)
                if (i > 0 && highScore >= UtilityAIPatches.GetHighestPossibleScore(package))
                    break;

                UAIAction action;
                object target;
                var score = package.DecideAction(_context, out action, out target) * package.Weight;

                // From vanilla: Only change the action if it's not already the action being taken.
                // This also means the target will not change, even if the same action against a
                // different target would produce a higher score this time.
                // (Should we change this?)
                if (score > highScore && _context.ActionData.Action != action)
                {
                    if (_context.ActionData.Action != null && _context.ActionData.CurrentTask != null)
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

                    _context.ActionData.Action = action;
                    _context.ActionData.Target = target;
                    _context.ActionData.TaskIndex = 0;
                    highScore = score; // bug fix - vanilla code never sets the high score
                }
            }

            // Don't call through to the original
            return false;
        }
    }

    [HarmonyPatch(typeof(UAIAction))]
    [HarmonyPatch("GetScore")]
    public class UAIAction_GetScore
    {
        /// <summary>
        /// This prefix method fixes what I believe to be a bug in the vanilla code, having to do
        /// with a misplaced cast to float. It also changes the vanilla behavior, to not give a
        /// higher weight to actions that have more considerations.
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="__result"></param>
        /// <param name="_context"></param>
        /// <param name="_target"></param>
        /// <param name="min"></param>
        /// <returns></returns>
        public static bool Prefix(
            // used by Harmony
            UAIAction __instance,
            ref float __result,
            // original parameters
            Context _context,
            object _target,
            float min = 0f)
        {
            if (__instance.GetTasks().Count == 0)
            {
                __result = 0f;
                return false;
            }

            var considerations = __instance.GetConsiderations();
            if (considerations.Count == 0)
            {
                // The vanilla code returns score * this.Weight, but at this point, the score is
                // always 1, so we can save the extra multiplication and just use the weight.
                __result = __instance.Weight;
                return false;
            }

            var score = 1f;
            for (var i = 0; i < considerations.Count; i++)
            {
                // bug fix: vanilla only fails fast if 0 > score and not if 0 == score
                if (0f >= score || score < min)
                {
                    __result = 0f;
                    return false;
                }

                var consideration = considerations[i];
                score *= consideration.ComputeResponseCurve(consideration.GetScore(_context, _target));
            }

            // If we want to give a higher score to actions with more considerations, then
            // we can use a bugfixed version of the vanilla code:
            // __result = (score + (1f - score) * (1f - 1f / __instance.considerations.Count) * score) * __instance.Weight;
            __result = score * __instance.Weight;
            return false;
        }
    }
}