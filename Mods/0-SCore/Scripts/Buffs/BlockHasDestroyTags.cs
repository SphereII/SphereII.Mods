using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

// <requirement name="BlockHasDestroyName, SCore" block_name="plantedGraceCorn1" />

public class BlockHasDestroyName : TargetedCompareRequirementBase
{
    private string nameSearch;
    
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
        if (!_params.BlockValue.Block.HasItemsToDropForEvent(EnumDropEvent.Destroy)) return false;

        foreach (var itemDrop in _params.BlockValue.Block.itemsToDrop[EnumDropEvent.Destroy])
        {
            if ( string.IsNullOrEmpty(itemDrop.name)) continue;
                foreach (var search in nameSearch.Split(","))
                {
                    var searchWord = search;
                    if (search.Contains("*"))
                    {
                        searchWord = search.Replace("*", "");
                        if (itemDrop.name.ContainsCaseInsensitive(searchWord)) return true;
                        continue;
                    }

                    if (itemDrop.name.EqualsCaseInsensitive(searchWord)) return true;
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
            if (localName == "block_name")
            {
                nameSearch = _attribute.Value;
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
