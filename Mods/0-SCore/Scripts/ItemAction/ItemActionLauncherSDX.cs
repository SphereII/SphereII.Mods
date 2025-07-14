using Audio;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug; // For Debug.WriteLine, if using. Otherwise, Unity's Debug.Log is fine.

/// <summary>
/// Extends ItemActionLauncher to provide custom behavior for holding, reloading,
/// and executing actions for launcher-type items, particularly addressing
/// NPC ammo loading and specific firing logic.
/// </summary>
class ItemActionLauncherSDX : ItemActionLauncher
{
    public override void StartHolding(ItemActionData _action)
    {
        // Issue: Launchers being held by NPCs don't appear to be loading their ammo properly on spawn in,
        // so they are just shooting blanks. This ensures they start with 1 "meta" value,
        // which might represent a loaded state or initial ammo count for this specific item type.
        if (_action.invData.itemValue.Meta == 0)
        {
            _action.invData.itemValue.Meta = 1;
        }

        base.StartHolding(_action);
    }

    /// <summary>
    /// This method hides the base ItemActionLauncher.Reloading method.
    /// It always returns false, effectively disabling the default reloading check
    /// for this specific launcher type. This suggests custom reload handling.
    /// </summary>
    /// <param name="actionData">The ranged item action data.</param>
    /// <returns>Always false, indicating that the item is not reloading via this check.</returns>
    public new static bool Reloading(ItemActionRanged.ItemActionDataRanged actionData)
    {
        // This 'new' keyword hides the base class's static Reloading method.
        // Ensure this is the intended behavior. If the base method has logic that
        // should sometimes run, this might need to be an override if the base method was virtual,
        // or a different approach might be needed.
        return false;
    }

    /// <summary>
    /// Applies effects of the item action after it's fired.
    /// Resets the item's meta value to 0 after firing, likely to represent an "unloaded" state.
    /// </summary>
    /// <param name="_gameManager">The game manager instance.</param>
    /// <param name="_actionData">The item action data.</param>
    /// <param name="_firingState">The current firing state.</param>
    /// <param name="_startPos">The starting position for effects.</param>
    /// <param name="_direction">The direction of the effects.</param>
    /// <param name="_userData">Additional user data.</param>
    public override void ItemActionEffects(GameManager _gameManager, ItemActionData _actionData, int _firingState,
        Vector3 _startPos, Vector3 _direction, int _userData = 0)
    {
        base.ItemActionEffects(_gameManager, _actionData, _firingState, _startPos, _direction, _userData);

        // Casting _actionData to ItemActionRanged.ItemActionDataRanged.
        // A null check or try-cast might be safer if _actionData isn't guaranteed to be this type.
        ItemActionRanged.ItemActionDataRanged itemActionDataRanged = (ItemActionRanged.ItemActionDataRanged)_actionData;
        itemActionDataRanged.invData.itemValue.Meta = 0; // Resets meta after effects.
    }

    /// <summary>
    /// Executes the item action, handling press and release logic for the launcher.
    /// Contains custom logic for burst firing, ammo checking, and event triggering.
    /// </summary>
    /// <param name="_actionData">The item action data.</param>
    /// <param name="_bReleased">True if the action button was released, false if pressed/held.</param>
    public override void ExecuteAction(ItemActionData _actionData, bool _bReleased)
    {
        // Attempt to cast _actionData to ItemActionDataRanged.
        // If the cast fails, it means this action data is not for a ranged item,
        // so we exit as this method is not applicable.
        ItemActionRanged.ItemActionDataRanged itemActionDataRanged =
            _actionData as ItemActionRanged.ItemActionDataRanged;
        if (itemActionDataRanged == null)
        {
            return;
        }

        // Handle the action when the input (e.g., mouse click, trigger pull) is released.
        if (_bReleased)
        {
            itemActionDataRanged.bReleased = true;
            itemActionDataRanged.curBurstCount = 0; // Reset burst count when the action is released.

            // Stop any ongoing sound sequence associated with this action.
            if (Manager.IsASequence(itemActionDataRanged.invData.holdingEntity, itemActionDataRanged.SoundStart))
            {
                Manager.StopSequence(itemActionDataRanged.invData.holdingEntity, itemActionDataRanged.SoundStart);
            }

            // Trigger any specific logic or events tied to the action's release.
            this.triggerReleased(itemActionDataRanged, _actionData.indexInEntityOfAction);
            return; // Exit, as the release action has been handled.
        }

        // --- Logic for when the action is pressed or held ---

        // Determine if this is the first press in a rapid trigger sequence.
        bool wasNotPressedBefore = !itemActionDataRanged.bPressed;
        bool isRapidTriggerInitialPress = wasNotPressedBefore && this.rapidTrigger;

        itemActionDataRanged.bPressed = true; // Mark the action as currently pressed.

        // Get the maximum burst count for the item. -1 typically means infinite burst.
        int maxBurstCount = this.GetBurstCount(_actionData);
        bool canContinueBurst = (int)itemActionDataRanged.curBurstCount < maxBurstCount || maxBurstCount == -1;

        // If it's not a rapid trigger's initial press, and the burst is complete,
        // and the action was not just released, then we should not fire.
        if (!isRapidTriggerInitialPress && !canContinueBurst && !itemActionDataRanged.bReleased)
        {
            return;
        }

        // Store the previous released state before modifying it.
        bool wasReleasedBefore = itemActionDataRanged.bReleased;
        itemActionDataRanged.bReleased = false; // Mark action as not released (since it's now pressed/held).

        // Check if the item is currently reloading.
        // Note: The static Reloading method of this class is documented to always return false.
        // This condition might be a placeholder or intended for future expansion.
        if (Reloading(itemActionDataRanged))
        {
            itemActionDataRanged.m_LastShotTime = Time.time; // Update last shot time even if reloading.
            return;
        }

        // Enforce delay between shots for non-rapid trigger actions.
        if (!isRapidTriggerInitialPress && Time.time - itemActionDataRanged.m_LastShotTime < itemActionDataRanged.Delay)
        {
            return;
        }

        // Reset burst shot start flag if it was previously true.
        if (itemActionDataRanged.burstShotStarted)
        {
            itemActionDataRanged.burstShotStarted = false;
        }

        itemActionDataRanged.m_LastShotTime = Time.time; // Update the time of the last successful shot.
        EntityAlive holdingEntity = itemActionDataRanged.invData.holdingEntity;
        holdingEntity.MinEventContext.Other = null; // Clear the 'Other' context for MinEvents.

        // Check if the holding entity is underwater and if the ammo is usable in that state.
        if (holdingEntity.isHeadUnderwater && !this.IsAmmoUsableUnderwater(holdingEntity))
        {
            return;
        }

        // Populate MinEventContext with relevant item data.
        holdingEntity.MinEventContext.ItemValue = holdingEntity.inventory.holdingItemItemValue;
        holdingEntity.MinEventContext.ItemActionData = _actionData; // Assign the current action data.

        itemActionDataRanged.curBurstCount += 1; // Increment the current burst count.

        // Check for available ammo.
        if (!this.checkAmmo(itemActionDataRanged))
        {
            // If no ammo, and the action was just released (or was released before this press),
            // play an empty sound and potentially trigger a reload.
            if (wasReleasedBefore)
            {
                holdingEntity.PlayOneShot(this.soundEmpty, false, false, false, null);
                if (itemActionDataRanged.state != ItemActionFiringState.Off)
                {
                    // Synchronize item action effects to the server to turn off firing state.
                    itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(
                        holdingEntity.entityId, itemActionDataRanged.invData.slotIdx,
                        itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero, Vector3.zero, 0);
                }

                itemActionDataRanged.state = ItemActionFiringState.Off; // Set firing state to off.

                // If auto-reload is possible, request a reload.
                if (this.CanReload(itemActionDataRanged))
                {
                    this.requestReload(itemActionDataRanged);
                    itemActionDataRanged.invData.holdingEntitySoundID =
                        -2; // Magic number: Likely indicates a specific sound state or ID for the holding entity.
                }
            }

            return; // Exit if ammo check fails.
        }

        // Ammo is available, proceed with firing sequence.
        itemActionDataRanged.burstShotStarted = true;
        // Note: Firing 'onSelfRangedBurstShotEnd' before 'onSelfRangedBurstShotStart' seems counter-intuitive.
        // Verify intended event order or if these are meant for different purposes.
        holdingEntity.FireEvent(MinEventTypes.onSelfRangedBurstShotEnd, true);
        holdingEntity.FireEvent(MinEventTypes.onSelfRangedBurstShotStart, true);

        // Update the item's firing state.
        if (itemActionDataRanged.state == ItemActionFiringState.Off)
        {
            itemActionDataRanged.state = ItemActionFiringState.Start; // First shot in a sequence.
        }
        else
        {
            itemActionDataRanged.state = ItemActionFiringState.Loop; // Subsequent shots in a sequence.
        }

        // Consume ammo if the item does not have infinite ammo.
        if (!this.InfiniteAmmo)
        {
            this.ConsumeAmmo(_actionData);
        }

        // Temporarily change the entity's model layer for firing animation/effects.
        int originalModelLayer = holdingEntity.GetModelLayer();
        holdingEntity.SetModelLayer(2, false, null); // Magic number '2': Likely a specific layer for firing animations.

        Vector3 lastShotImpactPosition = Vector3.zero;
        // Determine the number of rays to fire for projectile simulation (e.g., shotgun spread).
        int roundRayCount = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount,
            itemActionDataRanged.invData.itemValue, 1f,
            holdingEntity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);

        bool didHitSomething = false;
        for (int i = 0; i < roundRayCount; i++)
        {
            bool hitThisShot = false;
            lastShotImpactPosition = this.fireShot(i, itemActionDataRanged, ref hitThisShot);
            if (hitThisShot)
            {
                didHitSomething = true;
            }
        }

        // If no target was hit by any ray, fire the appropriate miss event.
        if (!didHitSomething && holdingEntity != null)
        {
            MinEventTypes missEventType = (_actionData.indexInEntityOfAction == 0)
                ? MinEventTypes.onSelfPrimaryActionMissEntity // Primary action miss.
                : MinEventTypes.onSelfSecondaryActionMissEntity; // Secondary action miss.
            holdingEntity.FireEvent(missEventType, true);
        }

        // Restore the entity's original model layer.
        holdingEntity.SetModelLayer(originalModelLayer, false, null);

        // Get action effects values and send them to the server for synchronization.
        Vector3 localOffset;
        Vector3 localDir;
        int actionEffectsValues = this.GetActionEffectsValues(_actionData, out localOffset, out localDir);
        itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(holdingEntity.entityId,
            itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction,
            (int)itemActionDataRanged.state, localOffset, localDir,
            actionEffectsValues | this.getUserData(_actionData));

        // Special handling for single-shot launchers that become unloaded (Meta == 0).
        if (this.GetMaxAmmoCount(itemActionDataRanged) == 1 && itemActionDataRanged.invData.itemValue.Meta == 0)
        {
            if (itemActionDataRanged.state != ItemActionFiringState.Off)
            {
                // Send server effect to explicitly turn off the firing state.
                itemActionDataRanged.invData.gameManager.ItemActionEffectsServer(holdingEntity.entityId,
                    itemActionDataRanged.invData.slotIdx, itemActionDataRanged.indexInEntityOfAction, 0, Vector3.zero,
                    Vector3.zero, 0);
            }

            itemActionDataRanged.state = ItemActionFiringState.Off; // Set firing state to off.
            this.item.StopHoldingAudio(itemActionDataRanged.invData); // Stop any holding audio.

            // If auto-reload is enabled and possible, request a reload.
            if (this.AutoReload && this.CanReload(itemActionDataRanged))
            {
                this.requestReload(itemActionDataRanged);
            }
        }

        // This call is crucial for creating new projectiles or other visual/gameplay elements
        // that need to be initialized after the shot is fired.
        StartHolding(_actionData);
    }
}