using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
            [HarmonyPatch(typeof(XUiC_RecipeStack))]
            [HarmonyPatch(nameof(XUiC_RecipeStack.Init))]
            public class XUiCRecipeStackOutputStackInit
            {
                public static void Postfix()
                {
                    QuestEventManager.Current.RepairItem -= CheckForDegradation;    
                    QuestEventManager.Current.RepairItem += CheckForDegradation;
                }

                private static void CheckForDegradation(ItemValue stack)
                {
                    if (stack == null || stack.IsEmpty()) return;
                
                    var minEventParams = new MinEventParams {
                        TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
                        ItemValue = stack,
                        Self = GameManager.Instance.World.GetPrimaryPlayer()
                    };
                    stack.ItemClass.FireEvent(MinEventTypes.onSelfItemRepaired, minEventParams);
                    minEventParams.Self.MinEventContext = minEventParams;
                    minEventParams.Self.FireEvent(MinEventTypes.onSelfItemRepaired);

                }
            }

}