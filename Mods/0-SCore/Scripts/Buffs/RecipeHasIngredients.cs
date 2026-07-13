using System;
using System.Xml.Linq;
using UnityEngine;

//  <requirement name="RecipeHasIngredients, SCore" ingredients="planted*" />
//  <requirement name="RecipeHasIngredients, SCore" ingredients="plantedCotton1,plantedCoffee*" />

public class RecipeHasIngredients : TargetedCompareRequirementBase
{
    private string _ingredients;

    public override bool IsValid(MinEventParams minEventParams)
    {
        // Perform the most basic checks first to fail early.
        if (!base.IsValid(minEventParams) || minEventParams.ItemValue == null) return false;
        if (string.IsNullOrEmpty(_ingredients)) return false;

        // GetRecipe() changed. It used to take a hash, so we could precisely grab a specific recipe.
        // We'll now store the RecipeName, then loop through all the recipes to see if it matches our hash code
        // Multiple recipes could exist with the same name.
        if (!minEventParams.ItemValue.HasMetadata("Recipe", TypedMetadataValue.TypeTag.Integer)) return false;
        if (minEventParams.ItemValue.GetMetadata("Recipe") is not int recipeHash) return false;

        if (!minEventParams.ItemValue.HasMetadata("RecipeName", TypedMetadataValue.TypeTag.String)) return false;
        if (minEventParams.ItemValue.GetMetadata("RecipeName") is not string recipeName) return false;

        Recipe targetRecipe = null;
        foreach (var recipe in CraftingManager.GetAllRecipes(recipeName))
        {
            if (recipe.GetHashCode() == recipeHash)
            {
                targetRecipe = recipe;
                break;
            };
        }
        if (targetRecipe == null) return false;
        


        foreach (var ingredient in targetRecipe.ingredients)
        {
            foreach (var search in _ingredients.Split(","))
            {
                var searchWord = search;
                if (_ingredients.Contains("*"))
                {
                    searchWord = _ingredients.Replace("*", "");
                    if (ingredient.itemValue.ItemClass.GetItemName().ContainsCaseInsensitive(searchWord)) return true;
                }

                if (ingredient.itemValue.ItemClass.GetItemName().EqualsCaseInsensitive(searchWord)) return true;
            }
        }


        return false;
    }


    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXAttribute(_attribute);
        if (!flag)
        {
            string localName = _attribute.Name.LocalName;
            if (localName == "ingredients")
            {
                _ingredients = _attribute.Value;
                return true;
            }
        }

        return flag;
    }
}