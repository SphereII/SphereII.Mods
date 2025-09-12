using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

// Updates the UI to show ( Broken ) on the Tool grid or item.
namespace SCore.Features.ItemDegradation.Harmony.ItemStacks
{
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch(nameof(XUiC_ItemStack.GetBindingValueInternal))]

    public class XUiCRequiredItemStackGetBindingValue
    {
        public static void Postfix(XUiC_ItemStack __instance, ref string _value, string _bindingName)
        {
            if (__instance.itemStack.IsEmpty()) return;
            var itemValue = __instance.itemStack.itemValue;
            if (!ItemDegradationHelpers.CanDegrade(itemValue)) return;
            if (_bindingName == "tooltip")
            {
                if (ItemDegradationHelpers.IsDegraded(itemValue))
                {
                    _value = $"( {Localization.Get("BrokenItem")} ) {_value}";
                }
            }
        }
    }
}
