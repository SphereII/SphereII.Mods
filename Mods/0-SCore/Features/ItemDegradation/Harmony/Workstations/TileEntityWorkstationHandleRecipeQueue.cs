    
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

                // Gate: stop the workstation if a degradable tool is already fully degraded.
                ItemStack[] slots = __instance.Tools;
                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i].IsEmpty()) continue;
                    var itemValue = slots[i].itemValue;
                    if (itemValue == null) continue;
                    if (!ItemDegradationHelpers.CanDegrade(itemValue)) continue;
                    if (ItemDegradationHelpers.IsDegraded(slots[i].itemValue))
                    {
                        __instance.IsBurning = false;
                        __instance.ResetTickTime();
                        return false;
                    }
                }

                // Degrade tools once per recipe completion.
                // Count how many individual crafts finish within this tick's time window
                // using the same time math the recipe queue itself uses, so degradation
                // fires at the correct per-craft rate regardless of tick frequency.
                if (recipeQueueItem.OneItemCraftTime > 0f)
                {
                    int completions = 0;
                    float timeLeft = recipeQueueItem.CraftingTimeLeft;
                    float timeAccounted = _timePassed;
                    while (timeAccounted >= timeLeft && completions < recipeQueueItem.Multiplier)
                    {
                        completions++;
                        timeAccounted -= timeLeft;
                        timeLeft = recipeQueueItem.OneItemCraftTime;
                    }
                    for (int c = 0; c < completions; c++)
                        ItemDegradationHelpers.CheckToolsForDegradation(__instance, recipeQueueItem.Recipe);
                }

                return true;
            }
        }
    }
    
