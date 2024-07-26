using UniLinq;
using UnityEngine;

public class BlockTakeAndReplace : Block
{
    // By default, all blocks using this class will have a take delay of 15 seconds, unless over-ridden by the XML.
    private float fTakeDelay = 6f;
    private string itemNames = "meleeToolRepairT1ClawHammer";
    private string pickupBlock;
    private string validMaterials = "Mwood_weak,Mwood_weak_shapes,Mwood_shapes";
    public override void Init()
    {
        base.Init();
        if (Properties.Values.ContainsKey("TakeDelay")) fTakeDelay = StringParsers.ParseFloat(Properties.Values["TakeDelay"]);
        if (Properties.Values.ContainsKey("HoldingItem")) itemNames = Properties.GetString("HoldingItem");
        if (Properties.Values.ContainsKey("PickUpBlock")) pickupBlock = Properties.GetString("PickUpBlock");
        if (Properties.Values.ContainsKey("ValidMaterials")) validMaterials = Properties.GetString("ValidMaterials");

    }

    // Override the on Block activated, so we can pop up our timer
    public override bool OnBlockActivated(WorldBase world, int clrIdx, Vector3i blockPos,
        BlockValue blockValue, EntityPlayerLocal player) {
        if (!ValidMaterialCheck(blockValue)) return false;
        TakeItemWithTimer(clrIdx, blockPos, blockValue, player);
        return true;
    }

    private ItemStack CreateItemStack(string item) {
        var itemClass = ItemClass.GetItemClass(item);
        if (itemClass != null)
        {
            return new ItemStack(new ItemValue(itemClass.Id), 1);
        }

        return null;
    }
    // Take logic to replace it with the Downgrade block, matching rotations.
    private void TakeTarget(TimerEventData timerData)
    {
        var world = GameManager.Instance.World;
        var array = (object[])timerData.Data;
        var clrIdx = (int)array[0];
        var blockValue = (BlockValue)array[1];
        var vector3I = (Vector3i)array[2];
        var block = world.GetBlock(vector3I);
        var entityPlayerLocal = array[3] as EntityPlayerLocal;

        var itemStack = CreateItemStack(blockValue.Block.GetBlockName());
            
        // Find the block value for the pick up value, and add it to the inventory
        if (!string.IsNullOrEmpty(PickedUpItemValue) && PickedUpItemValue.Contains(":"))
        {
            itemStack = CreateItemStack(PickedUpItemValue);
        }
        if (!string.IsNullOrEmpty(pickupBlock))
        {
            itemStack = CreateItemStack(pickupBlock);
        }
        
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
       // var itemStack = new ItemStack(targetBlock.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        entityPlayerLocal.PlayOneShot("Sounds/DestroyBlock/wooddestroy1");
        // Damage the block for its full health 
        DamageBlock(world, clrIdx, vector3I, block, block.Block.blockMaterial.MaxDamage, entityPlayerLocal.entityId);
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

    private bool ValidMaterialCheck(BlockValue blockValue) {
        foreach (var material in validMaterials.Split(','))
        {
            if (blockMaterial.id.Contains(material)) return true;
        }

        return false;
    }
    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing) {
        if (!ValidMaterialCheck(_blockValue))
            return string.Empty;
        return string.Format(Localization.Get("takeandreplace"), Localization.Get(_blockValue.Block.GetBlockName()));
        //    return "Press <E> to remove the wood from this block.";
    }
}