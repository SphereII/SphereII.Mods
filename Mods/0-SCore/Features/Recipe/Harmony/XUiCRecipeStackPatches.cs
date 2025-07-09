using System.Collections.Generic;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace Harmony.Recipes
{
    /*
    <recipe name="ammo45ACPCase" count="30" craft_time="5" craft_area="MillingMachine" tags="workbenchCrafting,PerkHOHMachineGuns">
        <ingredient name="resourceBrassIngot" count="5"/>
        <effect_group name="Additional Output">
			<triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers" count="2"/>
			<triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceDuctTape" count="1"/>
		</effect_group>
		
		<effect_group name="Sphere Testing">
		    <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="resourceYuccaFibers" count="2"/>

		    <triggered_effect trigger="onSelfItemCrafted" action="AddAdditionalOutput, SCore" item="ammoRocketHE" count="2">
			    <requirement name="HasBuff" buff="god"/>
		    </triggered_effect>

		    <triggered_effect trigger="onSelfItemCrafted" action="PlaySound" sound="player#painsm">
			    <requirement name="!HasBuff" buff="god"/>
		    </triggered_effect>
		    
		    <triggered_effect trigger="onSelfItemCrafted" action="AddBuff" buff="buffDrugEyeKandy"/>
	</effect_group>
	
    </recipe>
     */
    public class XUiCRecipeStackOutputStackPatches
    {
        [HarmonyPatch(typeof(XUiC_RecipeStack))]
        [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
        public class XUiCRecipeStackOutputStack
        {
            private static void DisplayTooltip(EntityPlayerLocal entityPlayer, string text)
            {
                GameManager.ShowTooltip(entityPlayer, text);
                Manager.PlayInsidePlayerHead("ui_denied");
            }

            private static void DisplayInventoryFull(EntityPlayerLocal entityPlayer)
            {
                string text = "No room in inventory, crafting has been halted until space is cleared.";
                if (Localization.Exists("wrnInventoryFull", false))
                {
                    text = Localization.Get("wrnInventoryFull", false);
                }

                DisplayTooltip(entityPlayer, text);
            }

            private static void DisplayWorkstationInventoryFull(EntityPlayerLocal entityPlayer)
            {
                var text =
                    "No room in workstation output, crafting has been halted until space is cleared.";
                if (Localization.Exists("wrnWorkstationOutputFull", false))
                {
                    text = Localization.Get("wrnWorkstationOutputFull", false);
                }

                DisplayTooltip(entityPlayer, text);
            }

            private static List<ItemStack> GetAdditionalOutput(Recipe recipe, MinEventParams minEventParams)
            {
                List<ItemStack> items = new List<ItemStack>();
                foreach (var minEffectGroup in recipe.Effects.EffectGroups)
                {
                    // Fire off all the events that may be on there.
                    minEffectGroup.FireEvent(MinEventTypes.onSelfItemCrafted, minEventParams);

                    // We need to grab the data from the xml, which is only stored in this minevent. We have to loop around looking for it.
                    foreach (var minEventActionBase in minEffectGroup.GetTriggeredEffects(MinEventTypes
                                 .onSelfItemCrafted))
                    {
                        if (minEventActionBase is not MinEventActionAddAdditionalOutput additionalOutput) continue;
                        if (!minEventActionBase.CanExecute(MinEventTypes.onSelfItemCrafted, minEventParams)) continue;

                        var itemStack = additionalOutput.GetItemStack();
                        items.Add(itemStack);
                    }
                }
                return items;
            }

            // Identify the correct recipe via the crafting area.
            private static Recipe GetRecipe(Recipe currentRecipe)
            {
                var recipeName = currentRecipe.GetName();
                var craftingArea = currentRecipe.craftingArea;
                foreach (var recipe in CraftingManager.GetRecipes(recipeName))
                {
                    if (recipe.craftingArea == craftingArea) return recipe;
                }

                return currentRecipe;
            }

            public static MinEventParams GenerateMinEventParams(EntityPlayer currentPlayer, EntityPlayer starterPlayer, ItemValue outputItemValue)
            {
                var minEventParams = new MinEventParams();
                minEventParams.TileEntity = TraderUtils.GetCurrentTraderTileEntity();
                minEventParams.Self = currentPlayer;
                minEventParams.Other = starterPlayer;
                minEventParams.Biome = currentPlayer.biomeStandingOn;
                minEventParams.ItemValue = outputItemValue;
                return minEventParams;
            }
            public static bool Prefix(ref bool __result, XUiC_RecipeStack __instance)
            {
                if (__instance.recipe == null) return true;
                var recipe = GetRecipe(__instance.recipe);

                // no effects? Don't bother.
                if (recipe.Effects == null) return true;

                var entityPlayer = __instance.xui.playerUI.entityPlayer;
                if (entityPlayer == null) return true;
                var startedPlayer = GameManager.Instance.World.GetEntity(__instance.startingEntityId) as EntityPlayer;
                
                var minEventParams = GenerateMinEventParams(entityPlayer, startedPlayer, __instance.outputItemValue);
                List<ItemStack> items = GetAdditionalOutput(recipe, minEventParams);
                
                if (items.Count == 0) return true;
                // if we have a workstation open
                var childByType = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationOutputGrid>();
                if (childByType != null)
                {
                    foreach (var itemStack in items)
                    {
                        ItemStack[] slots = childByType.GetSlots();
                        if (ItemStack.AddToItemStackArray(slots, itemStack) != -1)
                        {
                            childByType.SetSlots(slots);
                            childByType.UpdateData(slots);
                            continue;
                        }

                        __instance.isInventoryFull = true;
                        DisplayWorkstationInventoryFull(entityPlayer);
                        __result = false;
                        return false;
                    }

                    childByType.IsDirty = true;
                    return true;
                }

                // Backpack crafting
                foreach (var itemStack in items)
                {
                    if (__instance.xui.PlayerInventory.AddItem(itemStack)) continue;
                    if (itemStack.count != recipe.count)
                    {
                        __instance.xui.PlayerInventory.DropItem(itemStack);
                    }

                    __instance.isInventoryFull = true;
                    DisplayInventoryFull(entityPlayer);
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}