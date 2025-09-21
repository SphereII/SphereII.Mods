using HarmonyLib;
using SCore.Features.ItemDegradation.Harmony;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;


public static class OnSelfItemDegrade
{
    public static void CheckForDegradation(ItemStack stack)
    {
        if (stack == null || stack.IsEmpty()) return;
        CheckForDegradation(stack.itemValue, GameManager.Instance.World.GetPrimaryPlayer());
    }
    
    public static void CheckForDegradation(ItemValue itemValue, EntityAlive playerAlive)
    {
        if (!ItemDegradationHelpers.CanDegrade(itemValue)) return;
        var minEventParams = new MinEventParams {
            ItemValue = itemValue,
            Self = playerAlive,
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

        itemValue.ItemClass.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfItemDegrade, minEventParams);
        // if (minEventParams.Self == null) return;
        // minEventParams.Self.MinEventContext = minEventParams;
        // minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfItemDegrade);

    }
 
}