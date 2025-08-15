using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

/*
 * This RoutineUpdate will trigger a FireEvent using a new SCore onSelfRoutineUpdate event.
 * This action can be configured on through a buff with an update tick, and it'll execute it periodically.
 *   <triggered_effect trigger="onSelfBuffUpdate" action="RoutineUpdate, SCore" slots="bag,inventory,equipment"/>
 */
public class MinEventActionRoutineUpdate : MinEventActionTargetedBase
{
    private bool bEquipment;
    private bool bToolbelt;
    private bool bBag;
    
    public override void Execute(MinEventParams _params)
    {
        // Backpack
        if (bBag)
        {
            foreach (var item in _params.Self.bag.GetSlots())
                CheckSlots(item);
        }

        // Equipment
        if (bEquipment)
        {
            foreach (var item in _params.Self.equipment.GetItems())
                CheckItemValue(item);
        }

        // Tool Belt.
        if (bToolbelt)
        {
            foreach (var item in _params.Self.inventory.GetSlots())
                CheckSlots(item);
        }
    }

    private void CheckItemValue(ItemValue itemValue)
    {
        if (itemValue == null) return;
        OnSelfRoutineUpdate.RoutineUpdate(itemValue);
        foreach (var mod in itemValue.Modifications)
        {
            if ( mod?.ItemClass == null) continue;
            OnSelfRoutineUpdate.RoutineUpdate(mod);
        }
    }

    private void CheckSlots(ItemStack itemStack)
    {
        if (itemStack.IsEmpty()) return;
        CheckItemValue(itemStack.itemValue);
    }
    
    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var localName = attribute.Name.LocalName;
        if (localName == "slots")
        {
            var slots = attribute.Value;
            bBag = slots.Contains("bag");
            bEquipment = slots.Contains("equipment");
            bToolbelt = slots.Contains("inventory");
            return true;
        }

        return false;
    }
}