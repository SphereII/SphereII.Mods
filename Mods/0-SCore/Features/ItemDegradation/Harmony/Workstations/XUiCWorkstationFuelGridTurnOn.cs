using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.Workstations
{
    [HarmonyPatch(typeof(XUiC_WorkstationFuelGrid))]
    [HarmonyPatch(nameof(XUiC_WorkstationFuelGrid.TurnOn))]

    public class XUiCWorkstationFuelGridTurnOn
    {
        public static bool Prefix(XUiC_WorkstationFuelGrid __instance)
        {
            var queueItems = __instance.WorkstationData.GetRecipeQueueItems();
            var recipeQueueItem = queueItems[queueItems.Length - 1];
            if (recipeQueueItem?.Recipe == null) return true;
            ItemStack[] slots = __instance.WorkstationData.GetToolStacks();
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty() || slots[i].itemValue == null) continue;
                var itemValue = slots[i].itemValue;
                if (itemValue.type != recipeQueueItem.Recipe.craftingToolType) continue;
                if (!ItemDegradationHelpers.CanDegrade(itemValue)) continue;
                if (ItemDegradationHelpers.IsDegraded(slots[i].itemValue))
                {
                    return false;
                }

            }

            return true;
        }
    
    }
}
