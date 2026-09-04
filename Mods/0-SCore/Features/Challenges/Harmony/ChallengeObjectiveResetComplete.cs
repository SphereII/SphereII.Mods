using Challenges;
using HarmonyLib;

namespace SCore.Features.Challenges.Harmony
{
    /// <summary>
    /// Fixes stale challenge counters in the HUD tracker after an objective is reset.
    /// <para>
    /// <c>BaseChallengeObjective.ResetComplete()</c> is:
    /// </para>
    /// <code>
    /// Complete = false;   // no-op, and no event, when the objective was not complete
    /// current  = 0;       // backing field: the Current setter never runs
    /// </code>
    /// <para>
    /// Only the <c>Current</c> / <c>Complete</c> property setters call
    /// <c>HandleValueChanged()</c>, and that event is what
    /// <c>XUiC_QuestTrackerObjectiveEntry</c> subscribes to in order to redraw the HUD. Writing
    /// the field directly therefore resets the count silently: the challenge journal rebuilds its
    /// entries when opened and reads the true value, but the HUD keeps showing the pre-reset count
    /// until the save is reloaded.
    /// </para>
    /// <para>
    /// Note the complete-objective case is wrong too, not just quiet: <c>Complete = false</c> does
    /// raise the event, but it fires while <c>current</c> still holds the old value, so the HUD
    /// redraws with the stale count and is never told about the reset that follows. Firing once
    /// afterwards covers both cases.
    /// </para>
    /// <para>
    /// Reached from <c>RequirementGroupPhase.ResetComplete()</c> and
    /// <c>BaseRequirementObjectiveGroup.ResetObjectives()</c>, so this covers requirement groups
    /// as well as any objective that calls it directly.
    /// </para>
    /// </summary>
    [HarmonyPatch(typeof(BaseChallengeObjective))]
    [HarmonyPatch(nameof(BaseChallengeObjective.ResetComplete))]
    public class ChallengeObjectiveResetComplete
    {
        public static void Prefix(BaseChallengeObjective __instance, out int __state)
        {
            __state = __instance.Current;
        }

        public static void Postfix(BaseChallengeObjective __instance, int __state)
        {
            // Nothing to redraw when the count was already zero. If the objective was complete,
            // the Complete setter has already raised the event for that half of the reset.
            if (__state == 0) return;

            __instance.HandleValueChanged();
        }
    }
}
