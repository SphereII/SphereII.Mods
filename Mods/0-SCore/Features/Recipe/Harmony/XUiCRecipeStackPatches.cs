using System.Collections.Generic;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace Harmony.RecipesPatches
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
        // Used for crafting with the workstation open.
        [HarmonyPatch(typeof(XUiC_RecipeStack))]
        [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
        public class XUiCRecipeStackOutputStack
        {
    
            public static bool Prefix(ref bool __result, XUiC_RecipeStack __instance)
            {
                if (__instance.recipe == null) return true;
                var recipe = RecipeUtils.GetRecipe(__instance.recipe);

                // no effects? Don't bother.
                if (recipe.Effects == null) return true;
                var entityPlayer = __instance.xui.playerUI.entityPlayer;
                if (entityPlayer == null) return true;
                
                var startedPlayer = GameManager.Instance.World.GetEntity(__instance.startingEntityId) as EntityPlayer;
                var minEventParams = RecipeUtils.GenerateMinEventParams(entityPlayer, startedPlayer, __instance.outputItemValue);
                List<ItemStack> items = RecipeUtils.GetAdditionalOutput(recipe, minEventParams);
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
                        RecipeUtils.DisplayWorkstationInventoryFull(entityPlayer);
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
                    RecipeUtils.DisplayInventoryFull(entityPlayer);
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}