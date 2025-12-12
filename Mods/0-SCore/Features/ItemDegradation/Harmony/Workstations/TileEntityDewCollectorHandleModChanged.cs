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
            for (int i = 0; i < __instance.modSlots.Length; i++)
            {
                var mod = __instance.modSlots[i];
                if (mod.IsEmpty()) continue;

                if (!ItemDegradationHelpers.CanDegrade(mod.itemValue)) continue;

                // Reset the conversions based on if each type is degraded. 
                OnSelfItemDegrade.CheckForDegradation(mod);

                // If it's not degraded, don't reset its items.
                if (!ItemDegradationHelpers.IsDegraded(mod.itemValue)) continue;

                switch (blockDewCollector.ModTypes[i])
                {
                    case BlockCollector.ModEffectTypes.Type:
                        __instance.IsModdedConvertItem = false;
                        break;
                    case BlockCollector.ModEffectTypes.Speed:
                        __instance.CurrentConvertSpeed = 1f;
                        break;
                    case BlockCollector.ModEffectTypes.Count:
                        __instance.CurrentConvertCount = 1;
                        break;
                }
            }
        }
    }
}