using System.Collections.Generic;
using Audio;
using UnityEngine;

public class RecipeUtils
{
    public static void DisplayTooltip(EntityPlayerLocal entityPlayer, string text)
    {
        GameManager.ShowTooltip(entityPlayer, text);
        Manager.PlayInsidePlayerHead("ui_denied");
    }

    public static void DisplayInventoryFull(EntityPlayerLocal entityPlayer)
    {
        string text = "No room in inventory, crafting has been halted until space is cleared.";
        if (Localization.Exists("wrnInventoryFull", false))
        {
            text = Localization.Get("wrnInventoryFull", false);
        }

        DisplayTooltip(entityPlayer, text);
    }

    public static void DisplayWorkstationInventoryFull(EntityPlayerLocal entityPlayer)
    {
        var text =
            "No room in workstation output, crafting has been halted until space is cleared.";
        if (Localization.Exists("wrnWorkstationOutputFull", false))
        {
            text = Localization.Get("wrnWorkstationOutputFull", false);
        }

        DisplayTooltip(entityPlayer, text);
    }

    public static List<ItemStack> GetAdditionalOutput(Recipe recipe, MinEventParams minEventParams)
    {
        List<ItemStack> items = new List<ItemStack>();
        if (recipe?.Effects?.EffectGroups == null) return items;

        foreach (var minEffectGroup in recipe.Effects.EffectGroups)
        {
            // Fire off all the events that may be on there.
            minEffectGroup.FireEvent(MinEventTypes.onSelfItemCrafted, minEventParams);

            // We need to grab the data from the xml, which is only stored in this minevent. We have to loop around looking for it.
            foreach (var minEventActionBase in minEffectGroup.GetTriggeredEffects(MinEventTypes
                         .onSelfItemCrafted))
            {
                if (minEventActionBase is not MinEventActionAddAdditionalOutput additionalOutput) continue;
                var results = minEventActionBase.CanExecute(MinEventTypes.onSelfItemCrafted, minEventParams);
                if ( !results) continue;
                var itemStack = additionalOutput.GetItemStack();
                items.Add(itemStack);
            }
        }

        return items;
    }

    // Identify the correct recipe via the crafting area. The recipe stored and passed around the system may not have the right information.
    public static Recipe GetRecipe(Recipe currentRecipe)
    {
        var recipeName = currentRecipe.GetName();
        var craftingArea = currentRecipe.craftingArea;
        foreach (var recipe in CraftingManager.GetRecipes(recipeName))
        {
            if (recipe.craftingArea == craftingArea) return recipe;
        }

        return currentRecipe;
    }

    public static Recipe GetRecipe(string recipeName, string craftingArea)
    {
        foreach (var recipe in CraftingManager.GetRecipes(recipeName))
        {
            if ( string.IsNullOrEmpty(recipe.craftingArea)) continue;
            if (recipe.craftingArea == craftingArea) return recipe;
        }

        return null;
    }


    public static MinEventParams GenerateMinEventParams(EntityPlayer currentPlayer, EntityPlayer starterPlayer, ItemValue outputItemValue)
    {
        
        var minEventParams = new MinEventParams();
        minEventParams.TileEntity = TraderUtils.GetCurrentTraderTileEntity();
        minEventParams.Self = currentPlayer == null ? starterPlayer : currentPlayer;
        minEventParams.Other = starterPlayer;
        if (minEventParams.TileEntity != null)
        {
            var position = minEventParams.TileEntity.ToWorldPos();
            minEventParams.Biome = GameManager.Instance.World.GetBiome(position.x, position.y);
        }
        else
        {
            minEventParams.Biome = currentPlayer?.biomeStandingOn ?? starterPlayer?.biomeStandingOn;
        }

        minEventParams.ItemValue = outputItemValue;
        return minEventParams;
    }
}