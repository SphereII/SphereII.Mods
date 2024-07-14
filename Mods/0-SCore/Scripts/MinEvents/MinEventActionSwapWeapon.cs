//        <triggered_effect trigger = "onSelfBuffUpdate" action="SwapWeapon, SCore" item="meleeClub" />

using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionSwapWeapon : MinEventActionTargetedBase
{
    private string _itemName = string.Empty;
    public override void Execute(MinEventParams _params)
    {
        var item = ItemClass.GetItem(_itemName);
        if (item == null)
            return;

        if (_params.Self is EntityAliveSDX entityAliveSdx)
        {
            entityAliveSdx.UpdateWeapon(item);
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var flag = base.ParseXmlAttribute(_attribute);
        if (flag) return true;
        var name = _attribute.Name.LocalName;
        if (name != "item") return true;
        _itemName = _attribute.Value;
        return true;
    }
}