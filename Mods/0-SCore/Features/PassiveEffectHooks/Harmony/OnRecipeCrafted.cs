using HarmonyLib;

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
            itemValue.SetMetadata("CraftingArea", __instance.recipe.craftingArea, TypedMetadataValue.TypeTag.String);
            
            var minEventParams = new MinEventParams {
                TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
                Self = GameManager.Instance.World.GetPrimaryPlayer(),
                ItemValue =itemValue
                
            };
            minEventParams.Self.MinEventContext = minEventParams;
            __instance.xui?.playerUI?.entityPlayer?.FireEvent((MinEventTypes)SCoreMinEventTypes.onRecipeCrafted);
        }
    }
    
}