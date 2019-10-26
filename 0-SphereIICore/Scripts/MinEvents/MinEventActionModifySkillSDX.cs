using System.Xml;
using UnityEngine;
public class MinEventActionModifySkillSDX : MinEventActionTargetedBase {

    //  <triggered_effect trigger="onSelfBuffStart" action="ModifySkillSDX, Mods" tag="skill_name" operation="add" value="1" /> // levels up skill_name by 1

    public override void Execute(MinEventParams _params){
        for (int i = 0; i < this.targets.Count; i++){
            EntityPlayer entity = this.targets[i] as EntityPlayer;
            if (entity != null) {
                global::ProgressionValue progression = entity.Progression.GetProgressionValue(progressionName);
        
                if (progression != null && this.operation != string.Empty){
                    int num = 0;
                    if (this.operation.Equals("add")){
                        num = progression.Level + points;
                    }
                    if (this.operation.Equals("subtract")){
                        num = progression.Level - points;
                    }
                    global::ProgressionClass progressionClass = progression.ProgressionClass;
                    if (num >= progressionClass.MaxLevel){
                        progression.Level = progressionClass.MaxLevel;
                    } else if (num <= 0) {
                        progression.Level = 0;
                    } else { 
                        progression.Level = num;
                    }
                }
            }
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute attribute){
        bool flag = base.ParseXmlAttribute(attribute);
        if (!flag){
            string name = attribute.Name;
            if (name != null){
                if (name == "tag"){
                    progressionName = attribute.Value;
                    return true;
                }
                if (name == "operation"){
                    operation = attribute.Value;
                    return true;
                }
                if (name == "value"){
                    int.TryParse(attribute.Value, out this.points);
                    return true;
                }
            }
        }
        return flag;
    }

    private int points = 0;
    private string operation = string.Empty;
    private string progressionName = string.Empty;
}