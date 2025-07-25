
using System;
using System.Xml.Linq;
using UnityEngine;

//  <requirement name="RequirementRecipeCraftArea, SCore" craft_area="forge,workstation,chemistryStation" />

public class RequirementRecipeCraftArea : TargetedCompareRequirementBase
{
    private string _craftArea;

    public override bool IsValid(MinEventParams minEventParams)
    {
        if (!base.IsValid(minEventParams) || minEventParams.ItemValue == null) return false;

        if (!minEventParams.ItemValue.HasMetadata("CraftingArea")) return false;
        var craftArea = minEventParams.ItemValue.GetMetadata("CraftingArea").ToString();
        string[] requiredAreas = _craftArea.Split(',');
        foreach (string area in requiredAreas)
        {
            if (area.Equals(craftArea, StringComparison.OrdinalIgnoreCase))
            {
                return true; // A matching craft area was found.
            }
        }
        return false;
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        if (_attribute.Name.LocalName == "craft_area")
        {
            _craftArea = _attribute.Value.ToLower();
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }
}