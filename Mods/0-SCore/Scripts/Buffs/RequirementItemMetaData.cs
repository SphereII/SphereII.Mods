using System.Xml.Linq;
using UnityEngine;

public class RequirementItemMetaData: TargetedCompareRequirementBase
{
    public string strKey = "";

    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
            return false;

        if (this.target == null)
            return false;

        var itemValue = this.target.inventory.holdingItemItemValue;
       
        return itemValue.HasMetadata(strKey);
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        if (_attribute.Name.LocalName == "key")
        {
            strKey = _attribute.Value;
            return true;
        }

        return base.ParseXAttribute(_attribute);
    }
}