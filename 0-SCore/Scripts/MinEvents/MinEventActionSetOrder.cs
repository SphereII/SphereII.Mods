//        <triggered_effect trigger = "onSelfBuffUpdate" action="SetOrder, SCore" value="Stay" />

using System.Xml;
using System.Xml.Linq;

public class MinEventActionSetOrder : MinEventActionTargetedBase
{
    private EntityUtilities.Orders order = EntityUtilities.Orders.Wander;

    public override void Execute(MinEventParams _params)
    {
        EntityUtilities.SetCurrentOrder(_params.Self.entityId, order);
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
            {
                if (name == "order")
                {
                    order = EnumUtils.Parse<EntityUtilities.Orders>(_attribute.Value, true);
                    return true;
                }
            }
        }

        return flag;
    }
}