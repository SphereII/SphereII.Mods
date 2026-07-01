using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.Workstations
{
    [HarmonyPatch(typeof(TileEntityCollector))]
    [HarmonyPatch(nameof(TileEntityCollector.HandleModChanged))]
    public class TileEntityDewCollectorHandleModChanged
    {
        public static void Postfix(TileEntityCollector __instance)
        {
            var blockDewCollector = (BlockCollector)__instance.blockValue.Block;
            for (int i = 0; i < __instance.ModSlots.Length; i++)
            {
                var mod = __instance.ModSlots[i];
                if (mod.IsEmpty()) continue;

                if (!ItemDegradationHelpers.CanDegrade(mod.itemValue)) continue;

                // Reset the conversions based on if each type is degraded.
                OnSelfItemDegrade.CheckForDegradation(mod);

                // If it's not degraded, don't reset its items.
                if (!ItemDegradationHelpers.IsDegraded(mod.itemValue)) continue;

                // IsModdedConvertItem / CurrentConvertSpeed / CurrentConvertCount removed in new TileEntityCollector API
            }
        }
    }
}