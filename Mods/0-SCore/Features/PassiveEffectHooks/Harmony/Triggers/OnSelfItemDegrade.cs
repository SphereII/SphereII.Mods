using HarmonyLib;
using SCore.Features.ItemDegradation.Harmony;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;


public static class OnSelfItemDegrade
{
    public static void CheckForDegradation(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty()) return;
        if (!ItemDegradationHelpers.CanDegrade(stack.itemValue)) return;
        
        var minEventParams = new MinEventParams {
            ItemValue = stack.itemValue,
            Self = GameManager.Instance.World.GetPrimaryPlayer()
        };

        stack.itemValue.ItemClass.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfItemDegrade, minEventParams);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfItemDegrade);

    }
 
}