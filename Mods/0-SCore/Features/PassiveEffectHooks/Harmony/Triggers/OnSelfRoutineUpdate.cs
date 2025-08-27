using HarmonyLib;
using SCore.Features.ItemDegradation.Harmony;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;


public static class OnSelfRoutineUpdate
{
    public static void RoutineUpdate(ItemValue itemValue)
    {
        if (itemValue == null) return;
        if (!ItemDegradationHelpers.CanDegrade(itemValue)) return;
        var minEventParams = new MinEventParams {
            ItemValue = itemValue,
            Self = GameManager.Instance.World.GetPrimaryPlayer()
        };

        itemValue.ItemClass.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfRoutineUpdate, minEventParams);
    }
}