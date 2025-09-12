using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.Workstations
{
    // If the recipe has a requirement that is degraded, block it.
    [HarmonyPatch(typeof(XUiC_WorkstationToolGrid))]
    [HarmonyPatch(nameof(XUiC_WorkstationToolGrid.HasRequirement))]
    public class XUiCWorkstationToolGridHasRequirement
    {
        public static void Postfix(ref bool __result, XUiC_WorkstationToolGrid __instance, global::Recipe recipe)
        {
            if (recipe == null) return;
            if (recipe.craftingToolType == 0) return;
            ItemStack[] slots = __instance.GetSlots();
            for (var i = 0; i < slots.Length; i++)
            {
                // Check to make sure that the tool is usable before crafting.
                if (slots[i].itemValue.type != recipe.craftingToolType) continue;
                
                // If it's not completed degraded, let it pass.
                if (!ItemDegradationHelpers.IsDegraded(slots[i].itemValue)) continue;
                __result = false;
                return;
            }
        }
    }
}