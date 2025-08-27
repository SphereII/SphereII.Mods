using HarmonyLib;

namespace SCore.Features.ItemDisplayEntry.Harmony
{
    [HarmonyPatch(typeof(XUiM_ItemStack))]
    [HarmonyPatch(nameof(XUiM_ItemStack.GetCustomValue))]
    public class XUiMItemStackGetCustomValue
    {
        public static bool Prefix(ref float __result, DisplayInfoEntry entry, ItemValue itemValue, bool useMods)
        {
            if (string.IsNullOrEmpty(entry.CustomName)) return true;
            var value = ItemDisplayEntryUtils.ParseCustomValue(entry, itemValue);
            if (value >= 0)
            {
                __result = value;
                return false;
            }
            return true;
        }
    
    }
}
