using HarmonyLib;
using UnityEngine;

public class XUiC_ItemInfoWindowPatch {

    private const string AdvFeatureClass = "FoodSpoilage";
    private const string Feature = "FoodSpoilage";
    private static readonly bool FoodSpoilage = Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
    public class SCoreGetBindingValueHelper {
        
        public static string GetItemTypeIcon(ItemStack itemStack, string bindingName) {
            
            if (bindingName != "itemtypeicon") return string.Empty;
            if (itemStack.IsEmpty() || itemStack.itemValue.ItemClass == null) return string.Empty;
            if (itemStack.itemValue.ItemClass.IsBlock()) return string.Empty;
            if (!itemStack.itemValue.HasMetadata("Freshness")) return string.Empty;
            var freshNess = (float)itemStack.itemValue.GetMetadata("Freshness");
            if (freshNess < 0.1f) return string.Empty;
            return itemStack.itemValue.ItemClass.AltItemTypeIcon ?? string.Empty;
        }
        
    }
    [HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
    [HarmonyPatch("GetBindingValue")]
    public class ItemInfoWindowGetBindingValue {
        private static bool Prefix(ref bool __result, XUiC_ItemInfoWindow __instance, ref string value, string bindingName) {
            if (!FoodSpoilage) return true;
            var result = SCoreGetBindingValueHelper.GetItemTypeIcon(__instance.itemStack, bindingName);
            if (string.IsNullOrEmpty(result)) return true;
            value = result;
            __result = true;
            return false;
        }
    }
    [HarmonyPatch(typeof(XUiC_ItemStack))]
    [HarmonyPatch("GetBindingValue")]
    public class XUiC_ItemStackGetBindingValue {
        private static bool Prefix(ref bool __result, XUiC_ItemStack __instance, ref string _value, string _bindingName) {
            if (!FoodSpoilage) return true;
            var result = SCoreGetBindingValueHelper.GetItemTypeIcon(__instance.itemStack, _bindingName);
            if (string.IsNullOrEmpty(result)) return true;
            _value = result;
            __result = true;
            return false;
        }
    }
}