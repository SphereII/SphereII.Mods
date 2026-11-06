using System;
using System.Collections.Generic;
using HarmonyLib;

namespace SCore.Features.QuestPOIVariants.Harmony
{
    /// <summary>
    /// World.ResetPOIS returns a coroutine that reads prefabInstance.prefab when it runs, so swapping the
    /// prefab here, when the method is called, makes the rebuild use the variant.
    /// </summary>
    [HarmonyPatch(typeof(World))]
    [HarmonyPatch(nameof(World.ResetPOIS))]
    public class WorldResetPOISPatch
    {
        private static void Prefix(World __instance, List<PrefabInstance> prefabInstances, QuestClass questClass)
        {
            try
            {
                QuestPOIVariantManager.PrepareForReset(__instance, prefabInstances, questClass);
            }
            catch (Exception ex)
            {
                // Never block the vanilla reset.
                Log.Error($"[SCore] QuestPOIVariants: failed to prepare POI reset: {ex}");
            }
        }
    }
}
