using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

// <requirement name="BlockHasName, SCore" block_name="DewCollector*,Workbench*,ChemistryStation*,CementMixer*" />

public class BlockHasName : TargetedCompareRequirementBase
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
        var blockName = _params.BlockValue.Block.GetBlockName();
        if (string.IsNullOrEmpty(blockName)) return false;
        foreach (var search in nameSearch.Split(","))
        {
            var searchWord = search;
            if (search.Contains("*"))
            {
                searchWord = search.Replace("*", "");
                if (blockName.ContainsCaseInsensitive(searchWord)) return true;
                continue;
            }

            if (blockName.EqualsCaseInsensitive(searchWord)) return true;
        }
        return false;
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