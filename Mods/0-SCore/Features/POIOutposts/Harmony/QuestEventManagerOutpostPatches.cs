using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SCore.Features.POIOutposts.Harmony
{
    /// <summary>
    /// Hides outposts from quest POI searches. Vanilla's RandomPOIGoto and SCore's
    /// RandomTaggedPOIGotoSDX both get their POIs from these two lists, so no quest can pick an
    /// outpost and reset it back to the original prefab.
    /// </summary>
    [HarmonyPatch(typeof(QuestEventManager))]
    [HarmonyPatch(nameof(QuestEventManager.GetPrefabsByDifficultyTier))]
    public class QuestEventManagerGetPrefabsByDifficultyTierOutposts
    {
        private static void Postfix(ref List<PrefabInstance> __result)
        {
            try
            {
                __result = POIOutpostManager.WithoutOutposts(__result);
            }
            catch (Exception ex)
            {
                Log.Error($"[SCore] POIOutposts: failed to filter POIs by tier: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(QuestEventManager))]
    [HarmonyPatch(nameof(QuestEventManager.GetPrefabsForTrader))]
    public class QuestEventManagerGetPrefabsForTraderOutposts
    {
        private static void Postfix(ref List<PrefabInstance> __result)
        {
            try
            {
                __result = POIOutpostManager.WithoutOutposts(__result);
            }
            catch (Exception ex)
            {
                Log.Error($"[SCore] POIOutposts: failed to filter trader POIs: {ex}");
            }
        }
    }
}
