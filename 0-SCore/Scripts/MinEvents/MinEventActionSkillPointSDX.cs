using System.Xml;
using System.Xml.Linq;

public class MinEventActionSkillPointSDX : MinEventActionRemoveBuff
{
    private int mySkillPoint;

    //  <triggered_effect trigger="onSelfBuffStart" action="SkillPointSDX, SCore" target="self" value="2" /> // two Skill points
    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i] as EntityPlayer;
            if (entity != null)
                entity.Progression.SkillPoints += mySkillPoint;
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
                if (name == "value")
                {
                    int.TryParse(_attribute.Value, out mySkillPoint);
                    return true;
                }
        }

        return flag;
    }
}