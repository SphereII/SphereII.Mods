using Audio;
using System.Xml;
public class MinEventActionShowToolTipSDX : MinEventActionShowToolbeltMessage
{
    string message;
    string messageKey;
    string sound;

    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            EntityPlayerLocal player = targets[i] as EntityPlayerLocal;
            if (player)
            {
                if (sound != null)
                    Manager.PlayInsidePlayerHead(sound, -1, 0f, false, false);
                XUiC_TipWindow.ShowTip(messageKey, player, null);

            }
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        string name = _attribute.Name;

        if (name == "message")
        {
            if (message == null || message == "")
            {
                message = _attribute.Value;
            }
            return true;
        }
        if (name == "message_key")
        {
            if (_attribute.Value != "" && Localization.Exists(_attribute.Value))
            {
                message = Localization.Get(_attribute.Value);
                messageKey = _attribute.Value;
            }
            return true;
        }
        if (name == "sound")
        {
            if (_attribute.Value != "")
            {
                sound = _attribute.Value;
            }
            return true;
        }

        return flag;
    }

}
