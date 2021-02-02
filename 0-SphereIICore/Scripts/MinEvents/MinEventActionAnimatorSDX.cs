using System.Xml;
public class MinEventActionAnimatorSpeedSDX : MinEventActionRemoveBuff
{
    float floatSpeed = 1f;

    // This loops through all the targets, giving each target the quest. 
    //  <triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, Mods" target="self" value="1" /> // normal speed
    //  <triggered_effect trigger="onSelfBuffStart" action="AnimatorSpeedSDX, Mods" target="self" value="2" /> // twice the speed
    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            EntityAliveSDX entity = targets[i] as EntityAliveSDX;
            if (entity != null)
            {
                if (targets[i].emodel != null && targets[i].emodel.avatarController != null)
                {
                    targets[i].emodel.avatarController.anim.speed = floatSpeed;
                }
            }

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
                if (name == "value")
                {

                    float.TryParse(_attribute.Value, out floatSpeed);

                    return true;
                }
            }
        }
        return flag;
    }
}
