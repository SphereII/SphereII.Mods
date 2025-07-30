using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks
{
    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
    public class RecipeStackOutputStackOnRecipeCrafted 
    {
        public static void Postfix(XUiC_RecipeStack __instance)
        {
            if (__instance.recipe == null) return;
            var itemValue = ItemValue.None.Clone();
            if (string.IsNullOrEmpty(__instance.recipe.craftingArea))
                itemValue.SetMetadata("CraftingArea", "None", TypedMetadataValue.TypeTag.String);
            else
                itemValue.SetMetadata("CraftingArea", __instance.recipe.craftingArea, TypedMetadataValue.TypeTag.String);
           
            itemValue.SetMetadata("Tags", __instance.recipe.tags.ToString(), TypedMetadataValue.TypeTag.String);
            itemValue.SetMetadata("Recipe", __instance.recipe.GetHashCode(), TypedMetadataValue.TypeTag.Integer);

            var minEventParams = new MinEventParams {
                TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
                Self = GameManager.Instance.World.GetPrimaryPlayer(),
                ItemValue =itemValue
                
            };
            minEventParams.Self.MinEventContext = minEventParams;
            // Correct
            minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfCraftedRecipe);
            
        }
    }
    
}