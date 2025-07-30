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

        if (!minEventParams.ItemValue.HasMetadata("Recipe", TypedMetadataValue.TypeTag.Integer)) return false;
        if (minEventParams.ItemValue.GetMetadata("Recipe") is not int recipeHash) return false;

        var recipe = CraftingManager.GetRecipe(recipeHash);
        if (recipe == null) return false;


        foreach (var ingredient in recipe.ingredients)
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