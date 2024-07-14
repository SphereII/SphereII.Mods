using System.Globalization;
using System.Xml;
using System.Xml.Linq;

// 	<requirement name="RequirementIsNearFire, SCore" range="5" />

public class RequirementIsNearFire : RequirementBase
{
    float maxRange = 5f;
    public override bool ParamsValid(MinEventParams _params)
    {
        return FireManager.Instance.IsPositionCloseToFire(new Vector3i(_params.Self.position), (int)maxRange);
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var name = _attribute.Name.LocalName;
        if (name != null)
        {
            if (name == "range")
            {
                this.maxRange = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
                return true;
            }
        }

        return base.ParseXAttribute(_attribute);
    }

}