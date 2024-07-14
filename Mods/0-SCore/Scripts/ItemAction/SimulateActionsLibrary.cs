using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UAI;
public static class SimulateActionsLibrary
{
    public static IEnumerator SimulateActionThrownTimedCharge(Context _context, Vector3 vector)
    {
        _context.Self.SetLookPosition(vector);
        if (_context.Self.bodyDamage.HasLimbs)
            _context.Self.RotateTo(vector.x, vector.y, vector.z, 30f, 30f);

        _context.Self.Crouching = true;
        yield return new WaitForSeconds(0.1f);

        var originalItem = _context.Self.inventory.GetItem(0);

        var slotID = 0; // _context.Self.inventory.DUMMY_SLOT_IDX
        // Give them the item, and place it in their hand
        var item = ItemClass.GetItem("thrownTimedCharge");
        var _itemStack = new ItemStack(item, 1);
        _context.Self.inventory.SetItem(slotID, _itemStack);
        yield return new WaitForSeconds(0.1f);
        _context.Self.inventory.SetHoldingItemIdx(slotID);
        yield return new WaitForSeconds(0.1f);
        _context.Self.inventory.CallOnToolbeltChangedInternal();
        yield return new WaitForSeconds(0.1f);
        while (_context.Self.inventory.IsHolsterDelayActive())
            yield return new WaitForSeconds(0.1f);


        // We are not doing a full execute here, as the OnHolding Update does a cast of the player, to get the look position from its camera.
        // We want to avoid that, so we just skip that call, and use the position in which we pass in. 
        var _actionData = _context.Self.inventory.GetItemActionDataInSlot(slotID, 0);
        var myInventoryData = (ItemActionThrowAway.MyInventoryData)_actionData;

        // Set the last ThrowTime to -1 so it doesn't trigger the base throwAway()
        myInventoryData.m_LastThrowTime = -1f;

        // Activating the timer
        _context.Self.inventory.Execute(1, false, null);
        myInventoryData.m_LastThrowTime = -1f;
        yield return new WaitForSeconds(0.1f);

        _context.Self.inventory.Execute(1, true, null);
        myInventoryData.m_LastThrowTime = -1f;
        yield return new WaitForSeconds(1f);

        var invData = _actionData.invData;
        var holdingEntity = invData.holdingEntity;
        var lookVector = holdingEntity.GetLookVector();
        var headPosition = holdingEntity.getHeadPosition();

        // If we hit something, then toss the item to the block
        if (!Physics.Raycast(new Ray(vector - Origin.position, lookVector), out var raycastHit, 0.28f, -555274245))
        {
            vector += 0.23f * lookVector;
            vector -= headPosition;
            invData.gameManager.ItemDropServer(new ItemStack(holdingEntity.inventory.holdingItemItemValue, 1), vector, Vector3.zero, Vector3.zero, holdingEntity.entityId, 30f, true, -1);
        }
        _actionData.invData.holdingEntity.emodel.avatarController.TriggerEvent("ItemThrownTrigger");
        myInventoryData.m_LastThrowTime = 0f;

        _context.Self.inventory.SetItem(slotID, originalItem);
        yield return new WaitForSeconds(0.1f);
        _context.Self.inventory.SetHoldingItemIdx(slotID);
        yield return new WaitForSeconds(0.1f);
        _context.Self.inventory.CallOnToolbeltChangedInternal();
        yield return new WaitForSeconds(0.1f);
        while (_context.Self.inventory.IsHolsterDelayActive())
            yield return new WaitForSeconds(0.1f);

        _context.Self.IsBreakingBlocks = false;
        _context.Self.IsBreakingDoors = false;
        _context.Self.Crouching = false;

        yield break;


    }
}

