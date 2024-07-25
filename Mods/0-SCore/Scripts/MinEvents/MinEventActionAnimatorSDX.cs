using System.Xml;
using System.Xml.Linq;

public class MinEventActionAnimatorSpeedSDX : MinEventActionRemoveBuff
{
    private float floatSpeed = 1f;

    // This loops through all the targets, giving each target the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, SCore" target="self" value="1" /> // normal speed
    //  <triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, SCore" target="self" value="2" /> // twice the speed
    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i] as EntityAliveSDX;
            if (entity != null)
                if (targets[i].emodel != null && targets[i].emodel.avatarController != null)
                    targets[i].emodel.avatarController.GetAnimator().speed = floatSpeed;
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
                    float.TryParse(_attribute.Value, out floatSpeed);

                    return true;
                }
        }

        return flag;
    }
}