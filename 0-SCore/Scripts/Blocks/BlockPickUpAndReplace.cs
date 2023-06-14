public class BlockTakeAndReplace : Block
{
    // By default, all blocks using this class will have a take delay of 15 seconds, unless over-ridden by the XML.
    private float fTakeDelay = 6f;
    private string itemNames = "meleeToolRepairT1ClawHammer";
    public override void Init()
    {
        base.Init();

        if (Properties.Values.ContainsKey("TakeDelay")) fTakeDelay = StringParsers.ParseFloat(Properties.Values["TakeDelay"]);
        if (Properties.Values.ContainsKey("HoldingItem")) itemNames = Properties.GetString("HoldingItem");
    }

    // Override the on Block activated, so we can pop up our timer
    public override bool OnBlockActivated(WorldBase world, int clrIdx, Vector3i blockPos,
        BlockValue blockValue, EntityAlive player)
    {
        TakeItemWithTimer(clrIdx, blockPos, blockValue, player);
        return true;
    }

    // Take logic to replace it with the Downgrade block, matching rotations.
    private void TakeTarget(TimerEventData timerData)
    {
        var world = GameManager.Instance.World;
        var array = (object[])timerData.Data;
        var clrIdx = (int)array[0];
        var _blockValue = (BlockValue)array[1];
        var vector3i = (Vector3i)array[2];
        var block = world.GetBlock(vector3i);
        var entityPlayerLocal = array[3] as EntityPlayerLocal;
        // Find the block value for the pick up value, and add it to the inventory
        if (PickedUpItemValue.Contains(":"))
            PickedUpItemValue = "boardedWindowsSheet_weak";
        var pickUpBlock = GetBlockValue(PickedUpItemValue, true);
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        var itemStack = new ItemStack(pickUpBlock.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        entityPlayerLocal.PlayOneShot("Sounds/DestroyBlock/wooddestroy1");
        // Damage the block for its full health 
        DamageBlock(world, clrIdx, vector3i, block, block.Block.blockMaterial.MaxDamage, entityPlayerLocal.entityId);
    }


    // Displays the UI for the timer, calling TakeTarget when its done.
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        var playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true);
        var xuiC_Timer = playerUI.xui.GetChildByType<XUiC_Timer>();
        var timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
            _cIdx,
            _blockValue,
            _blockPos,
            _player
        };
        timerEventData.Event += TakeTarget;

        var newTakeTime = fTakeDelay;

        foreach (var item in itemNames.Split(','))
        {
            // If the entity is holding a crow bar or hammer, then reduce the take time.
            if (_player.inventory.holdingItem.Name == item)
            {
                // Make sure the item can still be used
                if (_player.inventory.holdingItemItemValue.MaxUseTimes > 0)
                {
                    // Bump the Use time by one.
                    var itemValue = _player.inventory.holdingItemItemValue;


                    // Calculate the degradation value.
                    itemValue.UseTimes += (int)EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemValue, 1f, _player);
                    _player.inventory.holdingItemData.itemValue = itemValue;

                    // Automatically reduce the take delay by half if you have a crow bar or claw hammer.
                    newTakeTime = fTakeDelay / 2;

                    // Reduce time based on the quality.
                    newTakeTime -= itemValue.Quality;
                    if (newTakeTime < 1)
                        newTakeTime = 1;
                    break;
                }
            }
        }


        xuiC_Timer.SetTimer(newTakeTime, timerEventData);
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        return string.Format(Localization.Get("takeandreplace"), Localization.Get(_blockValue.Block.GetBlockName()));
        //    return "Press <E> to remove the wood from this block.";
    }
}