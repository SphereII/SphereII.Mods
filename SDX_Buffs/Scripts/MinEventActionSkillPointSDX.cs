using System.Xml;
using UnityEngine;
public class MinEventActionSkillPointSDX : MinEventActionRemoveBuff
{
    int mySkillPoint = 0;
    //  <triggered_effect trigger="onSelfBuffStart" action="SkillPointSDX, Mods" target="self" value="2" /> // two Skill points
    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < this.targets.Count; i++)
        {
            EntityPlayer entity = this.targets[i] as EntityPlayer;
            if (entity != null)
                entity.Progression.SkillPoints += mySkillPoint;
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string name = _attribute.Name;
            if (name != null)
            {
                if (name == "value" )
                {
                    int.TryParse(_attribute.Value, out mySkillPoint);
                    return true;
                }
            }
        }
        return flag;
    }
}
