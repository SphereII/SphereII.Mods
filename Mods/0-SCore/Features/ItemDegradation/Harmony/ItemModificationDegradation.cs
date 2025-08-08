using System.Collections.Generic;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ItemDegradation.Harmony
{
    [HarmonyPatch(typeof(ItemAction))]
    [HarmonyPatch(nameof(ItemAction.HandleItemBreak))]
    public class ItemActionHandleItemBreak
    {
        public static void Postfix(global::ItemActionData _actionData)
        {
            var player = _actionData.invData.holdingEntity as EntityPlayerLocal;
            if (player == null) return;

            for (var i = 0; i < player.inventory.holdingItemItemValue.Modifications.Length; i++)
            {
                var mod = player.inventory.holdingItemItemValue.Modifications[i];
                if (mod == null) continue;
                if (mod.IsEmpty()) continue;
                if (!mod.HasQuality) continue;

                var maxUse =EffectManager.GetValue(PassiveEffects.DegradationMax, mod, 1f,
                    _actionData.invData.holdingEntity, null,mod.ItemClass.ItemTags);
                if (maxUse> 0 &&  mod.UseTimes >= maxUse)
                {
                    Manager.BroadcastPlay(_actionData.invData.holdingEntity, "itembreak");
                    if (mod.ItemClass.MaxUseTimesBreaksAfter.Value)
                    {
                        player.inventory.holdingItemItemValue.Modifications[i] = ItemValue.None;
                    }
                    continue;
                }
                mod.UseTimes +=EffectManager.GetValue(PassiveEffects.DegradationPerUse, mod, 1f,
                    _actionData.invData.holdingEntity, null,mod.ItemClass.ItemTags);
            }
        }
    }
}