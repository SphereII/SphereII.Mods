using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.General
{
    // Patch MaxUseTimesBase rather than MaxUseTimes. Since the perma-degradation change,
    // vanilla has MaxUseTimes => ModMaxUseTimes(MaxUseTimesBase, this) and
    // MaxUseTimesUI => MaxUseTimesBase. The durability bar (XUiC_ItemStack durabilityfill)
    // divides by MaxUseTimesUI, so patching only MaxUseTimes left the bar's denominator at
    // the vanilla DegradationMax value (often 0 for SCore-degraded items), pinning the bar
    // at full until the item broke. MaxUseTimes is also small enough for Mono to inline,
    // which bypasses a prefix on it; MaxUseTimesBase is not.
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch(nameof(ItemValue.MaxUseTimesBase), MethodType.Getter)]
    public class ItemValueMaxUseTimeGetter
    {
        public static bool Prefix(ref int __result, ItemValue __instance)
        {
            if (!ItemDegradationHelpers.CanDegrade(__instance)) return true;

            // This allows me to switch between properties and passive effects for maxuse times.
            __result = ItemDegradationHelpers.GetMaxUseTimes(__instance);
            return false;
        }
    }
}
