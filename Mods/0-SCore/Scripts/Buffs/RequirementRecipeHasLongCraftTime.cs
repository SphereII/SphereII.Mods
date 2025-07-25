using System;
using System.Xml.Linq;
using UnityEngine;

// If the recipe has over 100 seconds of crafting time.
//  <requirement name="RequirementRecipeHasLongCraftTime, SCore" operation="GTE" value="100" />

public class RequirementRecipeHasLongCraftTime : TargetedCompareRequirementBase
{
    private string _craftArea;
    private FastTags<TagGroup.Global> _tags;

    public override bool IsValid(MinEventParams minEventParams)
    {
        // --- 1. Initial Guard Clauses ---
        // Perform the most basic checks first to fail early.
        if (!base.IsValid(minEventParams) || minEventParams.ItemValue == null) return false;

        // --- 2. Safely Retrieve the Recipe Queue ---
        // Explicitly check each object in the chain to prevent NullReferenceException.
        var uiForPlayer = LocalPlayerUI.GetUIForPlayer(minEventParams.Self as EntityPlayerLocal);
        if (uiForPlayer?.xui?.GetCraftingData() == null) return false;

        var recipeQueueItems = uiForPlayer.xui.GetCraftingData().RecipeQueueItems;
        if (recipeQueueItems == null) return false;

        // --- 3. Iterate Through the Queue ---
        // This loop checks each item for a potential match.
        foreach (var recipeQueueItem in recipeQueueItems)
        {
            // Use 'continue' to skip items that don't meet the criteria.
            // This avoids deep nesting of 'if' statements.
            if (recipeQueueItem?.Recipe == null) continue;
            if (recipeQueueItem.Recipe.itemValueType != minEventParams.ItemValue.type) continue;
            var result = RequirementBase.compareValues(recipeQueueItem.Recipe.craftingTime, operation, value);
            if (invert) return !result;
            return result;
        }

        // If the loop completes without finding any matches, return false.
        return false;
    }

  
}