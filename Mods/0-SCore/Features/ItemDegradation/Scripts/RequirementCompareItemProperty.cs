using System.Xml.Linq;
using UnityEngine;

public class RequirementCompareItemProperty : TargetedCompareRequirementBase
{
// 	<requirement name="CompareItemProperty, SCore" property="Quality" operation="Equals" value="1"/>


    private string property = string.Empty;

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }
        var itemValue = _params.ItemValue;
        if (property == "quality")
        {
            if (!itemValue.HasQuality) return false;
            var quality = itemValue.Quality;
            if (invert)
            {
                return !compareValues((float)quality, operation, value);
            }

            return compareValues((float)quality, operation, value);
        }
        return base.IsValid(_params);
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var name = _attribute.Name.LocalName;
        if (name == "item_property")
        {
            property = _attribute.Value.ToLower();
        }
        else
            return base.ParseXAttribute(_attribute);

        return true;
    }
}