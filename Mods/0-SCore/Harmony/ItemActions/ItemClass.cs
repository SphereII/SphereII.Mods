using HarmonyLib;
using System.Collections.Generic;
using System.Text;

namespace Harmony.ItemActions
{
   
    public class ItemClasses
    {

        [HarmonyPatch(typeof(ItemInventoryData))]
        [HarmonyPatch(MethodType.Constructor)]
        [HarmonyPatch(new[] { typeof(ItemClass), typeof(ItemStack), typeof(IGameManager), typeof(global::EntityAlive), typeof(int) })]
        public class SCoreItemActionScrapping
        {
            public static void Postfix(ItemInventoryData __instance)
            {
                // Adding the other 3 spots.
                __instance.actionData.Add(null);
                __instance.actionData.Add(null);
                __instance.actionData.Add(null);
            }
        }

        // Expand the default action data for more complex items for NPCs
        [HarmonyPatch(typeof(ItemClass))]
        [HarmonyPatch("CreateInventoryData")]
        public class CreateInventoryData
        {
            public static ItemInventoryData Postfix(ItemInventoryData __result, ItemClass __instance, ItemStack _itemStack, IGameManager _gameManager, global::EntityAlive _holdingEntity, int _slotIdxInInventory)
            {
//                if (__result.actionData.Count > 2)
                    __result.actionData[2] = ((__instance.Actions[2] != null) ? __instance.Actions[2].CreateModifierData(__result, 2) : null);
  //              if (__result.actionData.Count > 3)
                    __result.actionData[3] = ((__instance.Actions[3] != null) ? __instance.Actions[3].CreateModifierData(__result, 3) : null);
    //            if (__result.actionData.Count > 4)
                    __result.actionData[4] = ((__instance.Actions[4] != null) ? __instance.Actions[4].CreateModifierData(__result, 4) : null);

                return __result;

            }
        }
    }
}