using Audio;
using System.Xml;
using System.Xml.Linq;

public class MinEventActionShowToolTipSDX : MinEventActionShowToolbeltMessage
{
    private string message;
    private string messageKey;
    private string sound;

    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var player = targets[i] as EntityPlayerLocal;
            if (player)
            {
                if (sound != null)
                    Manager.PlayInsidePlayerHead(sound);
                XUiC_TipWindow.ShowTip(messageKey, player, null);
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        var name = _attribute.Name.LocalName;

        if (name == "message")
        {
            if (message == null || message == "") message = _attribute.Value;
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
            if (_attribute.Value != "") sound = _attribute.Value;
            return true;
        }

        return flag;
    }
}