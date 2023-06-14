using System.Xml;
using System.Xml.Linq;

public class MinEventActionModifySkillSDX : MinEventActionTargetedBase
{
    private string operation = string.Empty;

    private int points;
    private string progressionName = string.Empty;

    //  <triggered_effect trigger="onSelfBuffStart" action="ModifySkillSDX, SCore" tag="skill_name" operation="add" value="1" /> // levels up skill_name by 1

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i] as EntityPlayer;
            if (entity != null)
            {
                var progression = entity.Progression.GetProgressionValue(progressionName);

                if (progression != null && operation != string.Empty)
                {
                    var num = 0;
                    if (operation.Equals("add")) num = progression.Level + points;
                    if (operation.Equals("subtract")) num = progression.Level - points;
                    var progressionClass = progression.ProgressionClass;
                    if (num >= progressionClass.MaxLevel)
                        progression.Level = progressionClass.MaxLevel;
                    else if (num <= 0)
                        progression.Level = 0;
                    else
                        progression.Level = num;
                }
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (!flag)
        {
            var name = attribute.Name.LocalName;
            if (name != null)
            {
                if (name == "tag")
                {
                    progressionName = attribute.Value;
                    return true;
                }

                if (name == "operation")
                {
                    operation = attribute.Value;
                    return true;
                }

                if (name == "value")
                {
                    int.TryParse(attribute.Value, out points);
                    return true;
                }
            }
        }

        return flag;
    }
}