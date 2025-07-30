using System;
using System.Xml.Linq;
using UnityEngine;

//  <requirement name="RequirementRecipeHasTags, SCore" tags="perkGreaseMonkey" />

public class RequirementRecipeHasTags : TargetedCompareRequirementBase
{
    private FastTags<TagGroup.Global> _tags;

    public override bool IsValid(MinEventParams minEventParams)
    {
        // Perform the most basic checks first to fail early.
        if (!base.IsValid(minEventParams) || minEventParams.ItemValue == null) return false;

        if (!minEventParams.ItemValue.HasMetadata("Tags", TypedMetadataValue.TypeTag.String)) return false;
        
        var tags2 = minEventParams.ItemValue.GetMetadata("Tags").ToString();
        var stringTags = minEventParams.ItemValue.GetMetadata("Tags");
        if (stringTags is string stringTag)
        {
            FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse(stringTag);
            return _tags.Test_AnySet(tags);
        }

        return false;


        // // --- 2. Safely Retrieve the Recipe Queue ---
        // // Explicitly check each object in the chain to prevent NullReferenceException.
        // var uiForPlayer = LocalPlayerUI.GetUIForPlayer(minEventParams.Self as EntityPlayerLocal);
        // if (uiForPlayer?.xui?.GetCraftingData() == null) return false;
        //
        // var recipeQueueItems = uiForPlayer.xui.GetCraftingData().RecipeQueueItems;
        // if (recipeQueueItems == null) return false;
        //
        // // --- 3. Iterate Through the Queue ---
        // // This loop checks each item for a potential match.
        // foreach (var recipeQueueItem in recipeQueueItems)
        // {
        //     // Use 'continue' to skip items that don't meet the criteria.
        //     // This avoids deep nesting of 'if' statements.
        //     if (recipeQueueItem?.Recipe == null) continue;
        //     if (recipeQueueItem.Recipe.itemValueType != minEventParams.ItemValue.type) continue;
        //     if (string.IsNullOrEmpty(_tags.ToString())) return true;
        //     if (recipeQueueItem.Recipe.tags.Test_AnySet(_tags)) return true;
        //     if ( tags.IsEmpty) continue;
        //     if ( recipeQueueItem.Recipe.tags.Test_AnySet(tags)) return true;    
        //     
        // }

        // If the loop completes without finding any matches, return false.
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXAttribute(_attribute);
        if (!flag)
        {
            string localName = _attribute.Name.LocalName;
            if (localName == "tags")
            {
                _tags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
                return true;
            }
        }
        return flag;
    }
}