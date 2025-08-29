    
    using HarmonyLib;
    using SCore.Features.ItemDegradation.Utils;
    using UnityEngine;
    
    namespace SCore.Features.ItemDegradation.Harmony
    {
        [HarmonyPatch(typeof(TileEntityWorkstation))]
        [HarmonyPatch(nameof(TileEntityWorkstation.HandleRecipeQueue))]
        public class TileEntityWorkstationHandleRecipeQueue
        {
    
            // Used for when crafting station is closed.
            public static bool Prefix(TileEntityWorkstation __instance, float _timePassed)
            {
                if (__instance.bUserAccessing || __instance.queue.Length == 0 || (__instance.isModuleUsed[3] && !__instance.isBurning)) return true;
                
                var recipeQueueItem = __instance.queue[__instance.queue.Length - 1];
                if (recipeQueueItem?.Recipe == null) return true;
                
                if (__instance.Tools == null) return true;
                
                ItemStack[] slots = __instance.Tools;
                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i].IsEmpty()) continue;
                    var itemValue = slots[i].itemValue;
                    if (itemValue == null) continue;
                    if (itemValue.type != recipeQueueItem.Recipe.craftingToolType) continue;
                    if (!ItemDegradationHelpers.CanDegrade(itemValue)) continue;
                    if (ItemDegradationHelpers.IsDegraded(slots[i].itemValue))
                    {
                        __instance.IsBurning = false;
                        __instance.ResetTickTime();
                        return false;
                    }

                }
    
                return true;
    
            }
        }
    }
    
