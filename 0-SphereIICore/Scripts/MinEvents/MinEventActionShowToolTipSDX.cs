using Audio;
using System.Xml;
using UnityEngine;
public class MinEventActionShowToolTipSDX : MinEventActionShowToolbeltMessage
{
    string message;
    string messageKey;
    string sound;

    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < this.targets.Count; i++)
        {
            EntityPlayerLocal player = this.targets[i] as EntityPlayerLocal;
            if (player)
            {
                if (this.sound != null)
                    Manager.PlayInsidePlayerHead(this.sound, -1, 0f, false, false);
                XUiC_TipWindow.ShowTip(this.messageKey, player, null);

            }
        }
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        string name = _attribute.Name;

        if (name == "message")
        {
            if (this.message == null || this.message == "")
            {
                this.message = _attribute.Value;
            }
            return true;
        }
        if (name == "message_key")
        {
            if (_attribute.Value != "" && Localization.Exists(_attribute.Value))
            {
                this.message = Localization.Get(_attribute.Value);
                this.messageKey = _attribute.Value;
            }
            return true;
        }
        if (name == "sound")
        {
            if (_attribute.Value != "")
            {
                this.sound = _attribute.Value;
            }
            return true;
        }

        return flag;
    }

}
