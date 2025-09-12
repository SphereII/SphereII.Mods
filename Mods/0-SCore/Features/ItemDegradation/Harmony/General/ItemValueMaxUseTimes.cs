using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.General
{
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch(nameof(ItemValue.MaxUseTimes), MethodType.Getter)]
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
