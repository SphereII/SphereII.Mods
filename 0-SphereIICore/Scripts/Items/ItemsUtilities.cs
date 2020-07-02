using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;

public static class ItemsUtilities
{

    public static void WarnQueueFull(EntityPlayerLocal player)
    {
        string text = "No room in queue!";
        if (Localization.Exists("wrnQueueFull"))
        {
            text = Localization.Get("wrnQueueFull");
        }
        GameManager.ShowTooltip(player, text);
        Audio.Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
    }

    public static bool CheckProperty(ItemClass itemClass, string Property)
    {
        if (itemClass.Properties.Contains(Property))
            return true;
        if (itemClass.Properties.Classes.ContainsKey(Property))
            return true;
        return false;
    }
    public static string GetStackSummary(List<ItemStack> stacks)
    {
        StringBuilder stringBuilder = new StringBuilder();
        foreach (ItemStack stack in stacks)
            stringBuilder.Append("  " + stack.itemValue.ItemClass.GetLocalizedItemName() + " (" + stack.count + ")\n");

        return stringBuilder.ToString();

    }
    // short cut to convert class properties
    public static List<ItemStack> ParseProperties(DynamicProperties dynamicProperties3)
    {
        List<ItemStack> stacks = new List<ItemStack>();
        foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            stacks.Add(ItemsUtilities.CreateItemStack(keyValuePair.Key, keyValuePair.Value.ToString()));

        return stacks;
    }

    // Short cut to convert <property name="MyProperty" value="resourceWood,0,resourceLeather,2" /> 
    public static List<ItemStack> ParseProperties(string strData)
    {
        List<ItemStack> stacks = new List<ItemStack>();
        string[] array = strData.Split(new char[] { ',' });
        for (int i = 0; i < array.Length; i += 2)
            stacks.Add(ItemsUtilities.CreateItemStack(array[i], array[i + 1]));
        return stacks;
    }

    // Create a stack of item, using name and count ( as string for reading from dynamicproperties ) 
    public static ItemStack CreateItemStack(string strItemName, string Count)
    {
        int amount = StringParsers.ParseSInt32(Count);
        return CreateItemStack(strItemName, amount);
    }

    public static ItemStack CreateItemStack(string strItemName, int Count)
    {
        ItemClass itemClass = ItemClass.GetItemClass(strItemName, false);
        return new ItemStack(ItemClass.GetItem(strItemName, false), Count);
    }

    public static bool Scrap(List<ItemStack> scrapIngredients, ItemStack OriginalStack, XUiController ItemController)
    {
        bool result = false;
        foreach (ItemStack scrapStack in scrapIngredients)
        {
            int TotalCount = scrapStack.count * OriginalStack.count;
            scrapStack.count = TotalCount;
            if (!ItemController.xui.PlayerInventory.AddItem(scrapStack, true))
                ItemController.xui.PlayerInventory.DropItem(scrapStack);
        }


        ((XUiC_ItemStack)ItemController).ItemStack = ItemStack.Empty.Clone();
        ((XUiC_ItemStack)ItemController).WindowGroup.Controller.SetAllChildrenDirty();

        return result;
    }
    public static bool ConvertAndCraft(Recipe recipe, EntityPlayerLocal player, XUiController ItemController)
    {
        bool result = false;

        XUi xui = ItemController.xui;
        XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();
        ItemValue itemValue = ((XUiC_ItemStack)ItemController).ItemStack.itemValue;

        if (!CheckIngredients(recipe.ingredients, player))
            return false;
        // Verify we can craft this.
        if (!recipe.CanCraft(recipe.ingredients, player))
            return false;

        if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), itemValue.MaxUseTimes))
        {
            WarnQueueFull(player);
            return false;
        }
        ((XUiC_ItemStack)ItemController).ItemStack = ItemStack.Empty.Clone();
        xui.PlayerInventory.RemoveItems(recipe.ingredients, 1);
        result = true;

        return result;
    }
    public static bool ConvertAndCraft(string strRecipe, int Reduction, EntityPlayerLocal player, XUiController ItemController)
    {
        bool result = false;
        Recipe newRecipe = GetReducedRecipes(strRecipe, Reduction);
        result = ConvertAndCraft(newRecipe, player, ItemController);
        return result;
    }
    public static Recipe GetReducedRecipes(string recipeName, int Reduction)
    {
        List<ItemStack> ingredients = new List<ItemStack>();

        // If there's a recipe, grab it, and change it into a repair recipe.
        Recipe recipe = CraftingManager.GetRecipe(recipeName);
        if (recipe != null)
        {
            foreach (ItemStack ingredient in recipe.ingredients)
            {
                if (ingredient.count >= Reduction)
                {
                    int repairCount = ingredient.count / Reduction;
                    ingredient.count = repairCount;
                    ingredients.Add(ingredient);
                }
            }

            recipe.craftingTime = Math.Max(1f, recipe.craftingTime / Reduction);
            recipe.craftExpGain = Math.Max(1, recipe.craftExpGain / Reduction);
            recipe.ingredients = ingredients;
        }
        return recipe;
    }

    public static bool CheckIngredients(List<ItemStack> ingredients, EntityPlayerLocal player)
    {
        bool result = true;
        foreach (ItemStack ingredient in ingredients)
        {
            // Check if the palyer hs the items in their inventory or bag.
            int playerHas = player.inventory.GetItemCount(ingredient.itemValue);
            if (ingredient.count > playerHas)
            {
                playerHas = player.bag.GetItemCount(ingredient.itemValue);
                if (ingredient.count > playerHas)
                {
                    result = false;
                }
            }
        }

        //// The player has all the ingredients.
        //if ( result )
        //{ 
        //    if (ingredient.count < playerHas)
        //    {
        //        ItemClass itemClass = ItemClass.GetItemClass(ingredient.itemValue.ItemClass.GetItemName(), false);
        //        ItemStack missingStack = new ItemStack(ingredient.itemValue, ingredient.count);
        //        player.AddUIHarvestingItem(missingStack, true);
        //        result = false;
        //    }
            
        //}

        return result;
    }

    public static List<BlockRadiusEffect> GetRadiusEffect(String strItemClass)
    {
        ItemClass itemClass = ItemClass.GetItemClass(strItemClass, false);
        List<BlockRadiusEffect> list2 = new List<BlockRadiusEffect>();

        if (itemClass.Properties.Values.ContainsKey("ActivatedBuff"))
        {
            String strBuff = itemClass.Properties.Values["ActivatedBuff"];
            string[] array5 = strBuff.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);

            // Grab the current radius effects
            foreach (string text4 in array5)
            {
                int num12 = text4.IndexOf('(');
                int num13 = text4.IndexOf(')');
                BlockRadiusEffect item = default(BlockRadiusEffect);
                if (num12 != -1 && num13 != -1 && num13 > num12 + 1)
                {
                    item.radius = StringParsers.ParseFloat(text4.Substring(num12 + 1, num13 - num12 - 1), 0, -1, NumberStyles.Any);
                    item.variable = text4.Substring(0, num12);
                }
                else
                {
                    item.radius = 1f;
                    item.variable = text4;
                }
                list2.Add(item);
            }
        }
        return list2;

    }

}

