using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.Init))]
    public class XUiCRecipeStackOutputStackInitCraft
    {
        public static void Postfix()
        {
            QuestEventManager.Current.CraftItem -= CheckForCrafting;    
            QuestEventManager.Current.CraftItem += CheckForCrafting;
        }

        private static void CheckForCrafting(ItemStack stack)
        {
            if (stack == null || stack.IsEmpty()) return;
                
            var minEventParams = new MinEventParams {
                TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
                ItemValue = stack.itemValue,
                Self = GameManager.Instance.World.GetPrimaryPlayer()
            };
        
            stack.itemValue.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
            minEventParams.Self.MinEventContext = minEventParams;
            minEventParams.Self.FireEvent(MinEventTypes.onSelfItemCrafted);

        }

    }

}