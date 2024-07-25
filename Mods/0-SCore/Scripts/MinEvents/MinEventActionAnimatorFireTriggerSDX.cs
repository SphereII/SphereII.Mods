using System.Xml.Linq;
using UnityEngine;

// Fires the specified trigger on all animators found on the target.
//			<triggered_effect trigger="onSelfBuffStart" action="AnimatorFireTriggerSDX, SCore" trigger="triggerName"/>

public class MinEventActionAnimatorFireTriggerSDX : MinEventActionTargetedBase
{
    private string _triggerName;
    
    public override void Execute(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_triggerName)) return;
            
        foreach (var entityAlive in this.targets)
        {
            foreach (var animator in entityAlive.GetComponents<Animator>())
            {
                animator.SetTrigger(_triggerName);
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag || attribute.Name.LocalName != "trigger") return flag;
        _triggerName = attribute.Value;
        return true;
    }


}