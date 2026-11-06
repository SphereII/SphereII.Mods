using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;

namespace SCore.Features.QuestRescue.Harmony
{
    /// <summary>
    /// Tracks which POIs World.ResetPOIS is rebuilding, so the rescue objective doesn't spawn its
    /// captive until the blocks are back in place. ResetPOIS only builds the coroutine; the work
    /// happens when it runs, so the coroutine is wrapped to see when it finishes.
    /// </summary>
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch(nameof(World.ResetPOIS))]
    public class WorldResetPOISRescuePatch
    {
        private static void Postfix(List<PrefabInstance> prefabInstances, ref IEnumerator __result)
        {
            if (prefabInstances == null || __result == null) return;

            try
            {
                var tracked = new List<PrefabInstance>(prefabInstances);
                RescueCaptiveManager.MarkResetStarted(tracked);
                __result = TrackReset(__result, tracked);
            }
            catch (Exception ex)
            {
                // Never block the vanilla reset.
                Log.Error($"[SCore] QuestRescue: failed to track POI reset: {ex}");
            }
        }

        private static IEnumerator TrackReset(IEnumerator reset, List<PrefabInstance> prefabInstances)
        {
            try
            {
                while (reset.MoveNext())
                    yield return reset.Current;
            }
            finally
            {
                RescueCaptiveManager.MarkResetFinished(prefabInstances);
            }
        }
    }
}
