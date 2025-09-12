using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.General
{
    [HarmonyPatch(typeof(ItemAction))]
    [HarmonyPatch(nameof(ItemAction.HandleItemBreak))]
    public class ItemActionHandleItemBreak
    {
        public static void Postfix(global::ItemActionData _actionData)
        {
            ItemDegradationHelpers.CheckModificationOnItem(_actionData.invData.holdingEntity.inventory.holdingItemItemValue.Modifications,
                _actionData.invData.holdingEntity);
        }
    }
}