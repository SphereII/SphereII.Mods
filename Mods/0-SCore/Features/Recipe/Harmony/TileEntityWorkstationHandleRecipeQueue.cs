using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;

namespace SCore.Features.Recipe.Harmony
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
            if (recipeQueueItem == null) return true;
        
            recipeQueueItem.CraftingTimeLeft -= _timePassed;
        
            var player = GameManager.Instance.World.GetPrimaryPlayer();
        
            // A single loop handles all completed crafts
            while (recipeQueueItem.CraftingTimeLeft < 0f && __instance.hasRecipeInQueue())
            {
                if (recipeQueueItem.Multiplier > 0)
                {
                    ItemValue itemValue = new ItemValue(recipeQueueItem.Recipe.itemValueType, false);
                    if (ItemClass.list[recipeQueueItem.Recipe.itemValueType] != null && ItemClass.list[recipeQueueItem.Recipe.itemValueType].HasQuality)
                    {
                        itemValue = new ItemValue(recipeQueueItem.Recipe.itemValueType, (int)recipeQueueItem.Quality, (int)recipeQueueItem.Quality, false, null, 1f);
                    }

                    if (ItemStack.AddToItemStackArray(__instance.output, new ItemStack(itemValue, recipeQueueItem.Recipe.count), -1) == -1)
                    {
                        return false;
                    }

                    var startingPlayer = GameManager.Instance.World.GetEntity(recipeQueueItem.StartingEntityId) as EntityPlayer;
                    var minEffect = RecipeUtils.GenerateMinEventParams(player, startingPlayer, itemValue);
                    var recipe = RecipeUtils.GetRecipe(recipeQueueItem.Recipe);

                    ItemDegradationHelpers.CheckToolsForDegradation(__instance, recipe);
                    
                    var additionalOutputs = RecipeUtils.GetAdditionalOutput(recipe, minEffect);
                    foreach (var addtionalOutput in additionalOutputs)
                    {
                        if (ItemStack.AddToItemStackArray(__instance.output, addtionalOutput) == -1)
                        {
                            return false;
                        }
                    }
                    __instance.AddCraftComplete(recipeQueueItem.StartingEntityId, itemValue, recipeQueueItem.Recipe.GetName(), recipeQueueItem.Recipe.IsScrap ? recipeQueueItem.Recipe.ingredients[0].itemValue.ItemClass.GetItemName() : "", recipeQueueItem.Recipe.craftExpGain, recipeQueueItem.Recipe.count);
                    recipeQueueItem.Multiplier--;
                    recipeQueueItem.CraftingTimeLeft += recipeQueueItem.OneItemCraftTime;
                }

                if (recipeQueueItem.Multiplier <= 0)
                {
                    var craftingTimeLeft = recipeQueueItem.CraftingTimeLeft;
                    __instance.cycleRecipeQueue();
                    recipeQueueItem = __instance.queue[__instance.queue.Length - 1];
                    recipeQueueItem.CraftingTimeLeft += ((craftingTimeLeft < 0f) ? craftingTimeLeft : 0f);
                }
            }


            return false;
        }
    }
}