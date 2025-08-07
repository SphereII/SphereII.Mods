using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

// <requirement name="RequirementBlockHasHarvestTags, SCore" tags="salvageHarvest" />

public class RequirementBlockHasHarvestTags : TargetedCompareRequirementBase
{
    public FastTags<TagGroup.Global> tags;
    public bool hasAllTags;
    
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }
        
        
        if (!invert)
        {
            return IsValidBlock(_params);
        }
        return !IsValidBlock(_params);
    }

    public virtual bool IsValidBlock(MinEventParams _params)
    {
        var isValid = false;
        if (_params.BlockValue.isair) return false;
        
        if (!_params.BlockValue.Block.HasItemsToDropForEvent(EnumDropEvent.Harvest)) return false;

        foreach (var itemDrop in _params.BlockValue.Block.itemsToDrop[EnumDropEvent.Harvest])
        {
            var itemTag = FastTags<TagGroup.Global>.Parse(itemDrop.tag);
            if (itemTag.Test_AnySet(tags))
            {
                return true;
            }
        }

        return isValid;
    }
    
    public override bool ParseXAttribute(XAttribute _attribute)
    {
        bool flag = base.ParseXAttribute(_attribute);
        if (!flag)
        {
            string localName = _attribute.Name.LocalName;
            if (localName == "tags")
            {
                this.tags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
                return true;
            }
        }
        return flag;
    }
    public override void GetInfoStrings(ref List<string> list)
    {
        list.Add(string.Format("Is {0} Target A Block", this.invert ? "NOT " : ""));
    }
}
