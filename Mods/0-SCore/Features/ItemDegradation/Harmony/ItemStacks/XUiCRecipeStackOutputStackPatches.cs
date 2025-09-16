using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.ItemStacks
{
    public class XUiCRecipeStackOutputStackPatches
    {
        // Degrades the tool's when a recipe is outputted, while the player is watching.
        [HarmonyPatch(typeof(XUiC_RecipeStack))]
        [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
        public class XUiCRecipeStackOutputStack
        {
            public static bool Prefix(ref bool __result, XUiC_RecipeStack __instance)
            {
                if (__instance.recipe == null) return true;
                var recipe = RecipeUtils.GetRecipe(__instance.recipe);
                
                var toolGrid = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationToolGrid>();
                if (toolGrid == null) return true;
                var xuicslots = toolGrid.GetItemStackControllers();
                foreach (var slot in xuicslots)
                {
                    if (slot.itemStack.IsEmpty() || slot.itemStack.itemValue == null) continue;
                    var itemValue = slot.itemStack.itemValue;
                   // if (itemValue.type != recipe.craftingToolType) continue;
                    if (!ItemDegradationHelpers.CanDegrade(itemValue)) continue;
                    if (ItemDegradationHelpers.IsDegraded(itemValue))
                    {
                        __result = false;
                        return false;
                    }
                    OnSelfItemDegrade.CheckForDegradation(slot.itemStack);
                    slot.IsDirty = true;
                }
                return true;
            }
        }
    }
}