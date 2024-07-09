using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace Harmony.NPCFeatures
{
    internal class EntityAliveStartingEquipment
    {
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("PostInit")]
        public class EntityAliveStartingEquipmentPostInit
        {
            private static void SetupStartingItems(global::EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
            {
                for (var i = 0; i < ___itemsOnEnterGame.Count; i++)
                {
                    var itemStack = ___itemsOnEnterGame[i];
                    var forId = ItemClass.GetForId(itemStack.itemValue.type);
                    if (forId.HasQuality)
                        itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6);
                    else
                        itemStack.count = forId.Stacknumber.Value;
                    __instance.inventory.SetItem(i, itemStack);
                }
            }

            public static void Postfix(global::EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
            {
                if (__instance is EntityPlayerLocal)
                    return; 
                if (___itemsOnEnterGame.Count <= 0) return;


                SetupStartingItems(__instance, ___itemsOnEnterGame);
                

                __instance.inventory.SetHoldingItemIdx(0);

            }
        }
    }
}