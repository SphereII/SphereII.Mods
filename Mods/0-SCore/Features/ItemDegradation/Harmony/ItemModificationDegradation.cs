using System.Collections.Generic;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ItemDegradation.Harmony
{
    public class ItemDegradationHelpers
    {
        public static bool CanDegrade(ItemValue mod)
        {
            if (mod == null) return false;
            if (mod.IsEmpty()) return false;
            if (!mod.HasQuality) return false;
            if (mod.MaxUseTimes <= 1) return false;
            return true;
        }

        public static bool IsDegraded(ItemValue mod)
        {
            if (!CanDegrade(mod)) return false;
            return mod.UseTimes >= mod.MaxUseTimes;
        }

        public static void CheckModification(ItemValue mod, EntityAlive player)
        {
            if (!CanDegrade(mod)) return;
            if (IsDegraded(mod)) return;


            mod.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, mod);

            if (mod.UseTimes < mod.MaxUseTimes) return;
            Manager.BroadcastPlay(player, "itembreak");
            if (mod.ItemClass.MaxUseTimesBreaksAfter.Value)
            {
                mod = ItemValue.None;
            }
        }

        public static void CheckModificationOnItem(ItemValue[] items, EntityAlive player)
        {
            for (var i = 0; i < items.Length; i++)
            {
                CheckModification(items[i], player);
            }
        }

        [HarmonyPatch(typeof(ItemAction))]
        [HarmonyPatch(nameof(ItemAction.HandleItemBreak))]
        public class ItemActionHandleItemBreak
        {
            public static void Postfix(global::ItemActionData _actionData)
            {
                CheckModificationOnItem(_actionData.invData.holdingEntity.inventory.holdingItemItemValue.Modifications,
                    _actionData.invData.holdingEntity);
            }
        }

        [HarmonyPatch(typeof(EntityAlive))]
        [HarmonyPatch(nameof(EntityAlive.ApplyLocalBodyDamage))]
        public class EntityAliveApplyLocalBodyDamage
        {
            public static void Postfix(global::EntityAlive __instance, DamageResponse _dmResponse)
            {
                if (__instance.equipment == null) return;

                var wornArmor = __instance.equipment.GetArmor();
                foreach (var armor in wornArmor)
                {
                    if (armor.ItemClass is ItemClassArmor armorItemClass)
                    {
                        if (_dmResponse.ArmorSlot == armorItemClass.EquipSlot)
                        {
                            CheckModification(armor, __instance);
                        }
                    }
                }
            }
        }
    }
}