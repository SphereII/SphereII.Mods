using HarmonyLib;
using UnityEngine;

namespace Harmony.ItemActions {
    public class ItemActionRepairLimiter {
        private static int _limitRepairProperty = -1;

        private static int GetRepairLimit(ItemValue itemValue) {
            if (!itemValue.ItemClass.Properties.Contains("RepairLimit")) return -1;
            
            // Check if RepairLimit is an array or not, and if so, expect a value for each quality level.
            var repairLimitValue = itemValue.ItemClass.Properties.GetStringValue("RepairLimit");
            if ( repairLimitValue.Contains(','))
            {
                var quality = itemValue.Quality;
                var index = 0;
                if (quality > 0)
                    index = quality - 1;
                if (repairLimitValue.Length >= index)
                {
                    return int.Parse(repairLimitValue.Split(',')[index]);
                }
                Log.Out($"Item {itemValue.ItemClass.GetItemName()}: Invalid repair limit: {repairLimitValue} for Quality Level {quality}");
                return -1;
            }

            if (StringParsers.TryParseSInt32(repairLimitValue, out var result))
                return result;
            return -1;
        } 
        private static int GetCurrentMetaData(ItemValue itemValue, string key) {
            var valueObj = itemValue.GetMetadata(key);
            if (valueObj is int obj) return obj;
            return 0;
        }
        private static int GetCurrentRepairLimit(ItemValue itemValue) {
            if ( itemValue == null ) return -1;
                
            // Look for the property on the item.
            if (!itemValue.ItemClass.Properties.Contains("RepairLimit")) return -1;
            return GetCurrentMetaData(itemValue, "CurrentRepairLimit");
        }
        
        [HarmonyPatch(typeof(ItemActionEntryRepair))]
        [HarmonyPatch(nameof(ItemActionEntryRepair.OnActivated))]
        public class ItemActionRepairLimiterItemActionEntryRepair {
            public static bool Prefix(ItemActionEntryRepair __instance) {
                // Don't bother checking empty stacks
                var xui = __instance.ItemController.xui;
                var itemValue = ItemClassUtils.GetItemValue(__instance.ItemController);
                if (itemValue == null) return true;
                //var stack = (XUiC_ItemStack)__instance.ItemController;
                // If it's -1, then it doesn't have a repair limit.
                var currentRepair = GetCurrentRepairLimit(itemValue);
                if (currentRepair == -1) return true;

                // Look for the property on the item, checking for quality.
                //var itemValue = stack.ItemStack.itemValue;
                var repairLimit = GetRepairLimit(itemValue);
                
                if ( repairLimit == -1) return true;
                QuestEventManager.Current.RepairItem += RepairItem;
                if (currentRepair < repairLimit) return true;
                GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("repair_limit_reached"));
                return false;

            }

        

            private static void RepairItem(ItemValue itemValue) {
                var currentRepair = GetCurrentMetaData(itemValue, "CurrentRepairLimit");
                currentRepair++;
                itemValue.SetMetadata("CurrentRepairLimit", currentRepair, TypedMetadataValue.TypeTag.Integer);
                QuestEventManager.Current.RepairItem -= RepairItem;
            }
        }
    }
}