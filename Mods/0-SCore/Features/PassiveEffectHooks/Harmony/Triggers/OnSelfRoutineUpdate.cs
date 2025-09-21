using HarmonyLib;
using SCore.Features.ItemDegradation.Harmony;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;


public static class OnSelfRoutineUpdate
{
    public static void RoutineUpdate(ItemValue itemValue)
    {
        if (itemValue == null)
        {
//            Debug.Log("RoutineUpdate():: No ItemValue");
            return;
        }

        if (!ItemDegradationHelpers.CanDegrade(itemValue))
        {
  //          Debug.Log($"RoutineUpdate():: Cannot Degrade: {itemValue.ItemClass.GetItemName()}");
            return;
        }
        var minEventParams = new MinEventParams {
            ItemValue = itemValue,
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

    //    Debug.Log($"RoutineUpdate() :: ItemValue: {itemValue.ItemClass.GetItemName()} :: {itemValue.UseTimes}");
        itemValue.ItemClass.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfRoutineUpdate, minEventParams);
    }
}