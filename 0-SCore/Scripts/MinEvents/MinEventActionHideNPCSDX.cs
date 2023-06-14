using System.Xml;
using System.Xml.Linq;

//        <triggered_effect trigger = "onSelfBuffUpdate" action="HideNPCSDX, SCore" hide="true" />
public class MinEventActionHideNPCSDX : MinEventActionTargetedBase
{
    private bool hide;

    public override void Execute(MinEventParams _params)
    {
        var entity = _params.Self as EntityAliveSDX;
        if (entity == null)
            return;

        if (hide)
            entity.SendOnMission(true);
        else
            entity.SendOnMission(false);
    }


    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
                if (name == "hide")
                    hide = StringParsers.ParseBool(_attribute.Value);
        }

        return flag;
    }
}