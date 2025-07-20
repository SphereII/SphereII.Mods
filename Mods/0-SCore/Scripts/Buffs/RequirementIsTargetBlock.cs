using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class RequirementIsTargetBlock : TargetedCompareRequirementBase
{
    public FastTags<TagGroup.Global> blockTags;
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
        if (this.hasAllTags)
        {
            isValid = _params.BlockValue.Block.HasAllFastTags(this.blockTags);
        }
        else
        {
            isValid = _params.BlockValue.Block.HasAnyFastTags(this.blockTags);
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
                this.blockTags = FastTags<TagGroup.Global>.Parse(_attribute.Value);
                return true;
            }
            if (localName == "has_all_tags")
            {
                this.hasAllTags = StringParsers.ParseBool(_attribute.Value, 0, -1, true);
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
