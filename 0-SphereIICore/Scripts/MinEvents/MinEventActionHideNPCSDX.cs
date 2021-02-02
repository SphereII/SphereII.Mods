using System.Xml;

//        <triggered_effect trigger = "onSelfBuffUpdate" action="HideNPCSDX, Mods" hide="true" />
public class MinEventActionHideNPCSDX : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {

        EntityAliveSDX entity = _params.Self as EntityAliveSDX;
        if (entity == null)
            return;

        if (hide)
            entity.SendOnMission(true);
        else
            entity.SendOnMission(false);

    }


    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "hide")
                    hide = StringParsers.ParseBool(_attribute.Value);
            }
        }
        return flag;
    }

    bool hide = false;
}


