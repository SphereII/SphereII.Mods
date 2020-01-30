using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
class SphereII_EntityAliveStartingEquipment_Equiment
{

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("PostInit")]
    public class SphereII__EntityAliveStartingEquipment_PostInit
    {
        public static void SetupStartingItems(EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
        {
            for(int i = 0; i < ___itemsOnEnterGame.Count; i++)
            {
                ItemStack itemStack = ___itemsOnEnterGame[i];
                ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
                if(forId.HasQuality)
                {
                    itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
                }
                else
                {
                    itemStack.count = forId.Stacknumber.Value;
                }
                __instance.inventory.SetItem(i, itemStack);
            }
        }

        public static void Postfix(EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
        {
            if (__instance is EntityPlayerLocal)
                return;

            if(___itemsOnEnterGame.Count > 0)
            {
                SetupStartingItems(__instance, ___itemsOnEnterGame);
                __instance.inventory.SetHoldingItemIdx(0);
            }
        }
    }
}

