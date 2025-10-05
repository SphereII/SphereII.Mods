using System.Globalization;
using System.Xml.Linq;
using Audio;
using SCore.Features.ItemDegradation.Utils;
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
    private bool bVehicle;
    
    public override void Execute(MinEventParams _params)
    {
        if (bVehicle)
        {
            var entityVehicle = _params.Self as EntityVehicle;
            if (entityVehicle == null) return;
            var vehicle = entityVehicle.vehicle;
            CheckItemValue(vehicle.itemValue, null);
            return;
        }
        var localPlayer = _params.Self as EntityPlayerLocal;
        var playerUI = localPlayer?.playerUI;
        if (playerUI == null) return;
        
        
        // Backpack
        if (bBag)
        {
            foreach (var item in _params.Self.bag.GetSlots())
                CheckSlots(item, null);
        }

        // Equipment
        if (bEquipment)
        {
            for( var x =0; x < _params.Self.equipment.GetSlotCount(); x++ )
            {
                var item = _params.Self.equipment.GetSlotItem(x);
                if ( ItemDegradationHelpers.IsDegraded(item)) continue;
                CheckItemValue(item, null);
                if (!ItemDegradationHelpers.IsDegraded(item) || !item.ItemClass.MaxUseTimesBreaksAfter.Value) continue;
                Manager.BroadcastPlay(localPlayer, "itembreak");
                _params.Self.equipment.SetSlotItem(x, ItemValue.None);
            }
        }

        // Tool Belt.
        if (bToolbelt)
        {
            var childByType = playerUI.xui.FindWindowGroupByName("toolbelt").GetChildByType<XUiC_Toolbelt>();
            if (childByType != null)
            {
                foreach (var slot in childByType.GetItemStackControllers())
                {
                    CheckSlots(slot.itemStack, slot);
                }
            }
            else
            {
                foreach (var item in _params.Self.inventory.GetSlots())
                {
                    CheckSlots(item, null);
                }
            }
          
        }

    }

    private void CheckItemValue(ItemValue itemValue, XUiC_ItemStack stack)
    {
        if (itemValue == null) return;
        OnSelfRoutineUpdate.RoutineUpdate(itemValue);
        foreach (var mod in itemValue.Modifications)
        {
            if ( mod?.ItemClass == null) continue;
            OnSelfRoutineUpdate.RoutineUpdate(mod);
        }
    }

    private void CheckSlots(ItemStack itemStack, XUiC_ItemStack stack)
    {
        if (itemStack.IsEmpty()) return;
        CheckItemValue(itemStack.itemValue, stack);
    }
    
    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var localName = attribute.Name.LocalName;
        if (localName != "slots") return false;

        var slots = attribute.Value.ToLower();
        bBag = slots.Contains("bag");
        bEquipment = slots.Contains("equipment");
        bToolbelt = slots.Contains("inventory");
        bVehicle = slots.Contains("vehicle");

        return true;

    }
}