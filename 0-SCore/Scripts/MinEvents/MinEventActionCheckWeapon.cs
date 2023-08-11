//        <triggered_effect trigger = "onSelfBuffUpdate" action="CheckWeapon, SCore" />

using System.Xml;
using System.Xml.Linq;
using UnityEngine;

public class MinEventActionCheckWeapon : MinEventActionTargetedBase
{
    private string _itemName = string.Empty;
    public override void Execute(MinEventParams _params)
    {
        if (_params.Self is not EntityAliveSDX entityAliveSdx)
            return;
        
        var currentWeaponId = (int) entityAliveSdx.Buffs.GetCustomVar("CurrentWeaponID");
        Debug.Log($"Current Weapon ID: {currentWeaponId}");
        var item = ItemClass.GetForId(currentWeaponId);
        if (item == null) return;
        var itemValue = ItemClass.GetItem(item.GetItemName());
        entityAliveSdx.UpdateWeapon(itemValue);
    }

}