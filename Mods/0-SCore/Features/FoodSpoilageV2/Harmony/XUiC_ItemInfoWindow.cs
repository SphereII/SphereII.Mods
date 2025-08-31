using HarmonyLib;
using UnityEngine; // Needed for String.IsNullOrEmpty

namespace SphereII.FoodSpoilage.HarmonyPatches
{
    /// <summary>
    /// Harmony patches for UI elements (Item Info Window, Item Stack Icon)
    /// to potentially reflect food spoilage status (e.g., alternative icons).
    /// </summary>
    public class ItemInfoWindowPatches
    {
        /// <summary>
        /// Helper class to share logic between ItemInfoWindow and ItemStack patches for getting binding values.
        /// </summary>
        public static class BindingValueHelper
        {
            public static string GetAlternateItemTypeIcon(ItemStack itemStack, string bindingName)
            {
                // Only intercept the specific binding name for the icon
                if (bindingName != "itemtypeicon") return string.Empty;

                 // Basic validity checks
                if (itemStack == null || itemStack.IsEmpty() || itemStack.itemValue?.ItemClass == null) return string.Empty;
                 if (itemStack.itemValue.ItemClass.IsBlock()) return string.Empty; // Don't change block icons

                // Check if the item is spoilable and has freshness data
                if (!itemStack.itemValue.ItemClass.Properties.GetBool(SpoilageConstants.PropSpoilable)) return string.Empty;
                if (!itemStack.itemValue.HasMetadata(SpoilageConstants.MetaFreshness)) return string.Empty;

                 // Check if freshness is below threshold (e.g., show rotten icon if not fresh)
                // This implementation retrieves an *alternate* icon defined in XML, it doesn't dynamically change based on freshness level beyond presence.
                 if (!FoodSpoilagePatches.FoodSpoilageXUiCItemStackUpdatePatch.IsFreshEnough(itemStack.itemValue))
                 {
                      // Return the alternate icon name specified in XML, if it exists
                     return itemStack.itemValue.ItemClass.Properties.GetString(SpoilageConstants.PropAltItemTypeIcon) ?? string.Empty;
                 }

                // If fresh or no alternate icon defined, return empty to let original logic handle it
                return string.Empty;
            }
        }

        // Patch for the main Item Info Window
        [HarmonyPatch(typeof(XUiC_ItemInfoWindow))]
        [HarmonyPatch("GetBindingValueInternal")]
        public class ItemInfoWindowGetBindingValuePatch
        {
            private static bool Prefix(ref bool __result, XUiC_ItemInfoWindow __instance, ref string value, string bindingName)
            {
                 // Check if the core feature is enabled
                if (!SpoilageConfig.IsFoodSpoilageEnabled) return true; // Let original method run

                // Try to get an alternate icon
                var alternateIcon = BindingValueHelper.GetAlternateItemTypeIcon(__instance?.itemStack, bindingName);

                if (!string.IsNullOrEmpty(alternateIcon))
                {
                    value = alternateIcon; // Set the binding value to the alternate icon
                    __result = true;       // Indicate success
                    return false;          // Prevent original method from running
                }

                return true; // No alternate icon found, let original method run
            }
        }

        // Patch for the ItemStack UI element itself (used in inventory grids, etc.)
        [HarmonyPatch(typeof(XUiC_ItemStack))]
        [HarmonyPatch("GetBindingValueInternal")]
        public class XUiC_ItemStackGetBindingValuePatch
        {
             // Parameters renamed slightly from original decompiled code for clarity (_value, _bindingName)
            private static bool Prefix(ref bool __result, XUiC_ItemStack __instance, ref string _value, string _bindingName)
            {
                 // Check if the core feature is enabled
                 if (!SpoilageConfig.IsFoodSpoilageEnabled) return true; // Let original method run

                 // Try to get an alternate icon
                 var alternateIcon = BindingValueHelper.GetAlternateItemTypeIcon(__instance?.itemStack, _bindingName);

                 if (!string.IsNullOrEmpty(alternateIcon))
                 {
                     _value = alternateIcon; // Set the binding value to the alternate icon
                     __result = true;       // Indicate success
                     return false;          // Prevent original method from running
                 }

                 return true; // No alternate icon found, let original method run
            }
        }
    }
}