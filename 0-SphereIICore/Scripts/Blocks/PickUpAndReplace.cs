using System;
using UnityEngine;
using System.Collections;
using System.Globalization;

public class BlockTakeAndReplace : Block
{
    // By default, all blocks using this class will have a take delay of 15 seconds, unless over-ridden by the XML.
    private float fTakeDelay = 6f;

    public override void Init()
    {
        base.Init();

        if (this.Properties.Values.ContainsKey("TakeDelay"))
        {
            this.fTakeDelay = StringParsers.ParseFloat(this.Properties.Values["TakeDelay"], 0, -1, NumberStyles.Any);
        }
    }

    // Override the on Block activated, so we can pop up our timer
    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx,
        Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
        return true;
    }

    // Take logic to replace it with the Downgrade block, matching rotations.
    private void TakeTarget(TimerEventData timerData)
    {
        World world = GameManager.Instance.World;
        object[] array = (object[])timerData.Data;
        int clrIdx = (int) array[0];
        BlockValue _blockValue = (BlockValue) array[1];
        Vector3i vector3i = (Vector3i) array[2];
        BlockValue block = world.GetBlock(vector3i);
        EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
        // Find the block value for the pick up value, and add it to the inventory
        BlockValue pickUpBlock = Block.GetBlockValue(this.PickedUpItemValue, true);
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        ItemStack itemStack = new ItemStack(pickUpBlock.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        entityPlayerLocal.PlayOneShot("Sounds/DestroyBlock/wooddestroy1");
        // Damage the block for its full health 
        this.DamageBlock(world, clrIdx, vector3i, block, block.Block.blockMaterial.MaxDamage, entityPlayerLocal.entityId, false);
    }


    // Displays the UI for the timer, calling TakeTarget when its done.
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true, false, true);
        XUiC_Timer xuiC_Timer = (XUiC_Timer) playerUI.xui.GetChildByType<XUiC_Timer>();
        TimerEventData timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
            _cIdx,
            _blockValue,
            _blockPos,
            _player
        };
        timerEventData.Event += this.TakeTarget;

        float newTakeTime = this.fTakeDelay;

        // If the entity is holding a crow bar or hammer, then reduce the take time.
        if (_player.inventory.holdingItem.Name == "CrowBar" || _player.inventory.holdingItem.Name == "meleeToolClawHammer")
        {
            // Make sure the item can still be used
            if (_player.inventory.holdingItemItemValue.MaxUseTimes > 0)
            {
                // Bump the Use time by one.
                global::ItemValue itemValue = _player.inventory.holdingItemItemValue;

              
                // Calculate the degradation value.
                itemValue.UseTimes += (int)EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemValue, 1f, _player, null, default(FastTags), true, true, true, true);
                _player.inventory.holdingItemData.itemValue = itemValue;

                // Automatically reduce the take delay by half if you have a crow bar or claw hammer.
                newTakeTime = (this.fTakeDelay / 2);

                // Reduce time based on the quality.
                newTakeTime -= itemValue.Quality;
                if (newTakeTime < 1)
                    newTakeTime = 1;
            }
        }

        xuiC_Timer.SetTimer(newTakeTime, timerEventData);
    }

    public override string GetActivationText(global::WorldBase _world, global::BlockValue _blockValue, int _clrIdx, global::Vector3i _blockPos, global::EntityAlive _entityFocusing)
    {
        return string.Format(Localization.Get("takeandreplace"), Localization.Get(_blockValue.Block.GetBlockName()));
    //    return "Press <E> to remove the wood from this block.";
    }
}
