using UniLinq;
using UnityEngine;

public class BlockTakeAndReplace : Block {
    // By default, all blocks using this class will have a take delay of 15 seconds, unless over-ridden by the XML.
    private float fTakeDelay = 6f;
    private string itemNames = "meleeToolRepairT1ClawHammer";
    private string pickupBlock;
    private string validMaterials = "Mwood_weak,Mwood_weak_shapes,Mwood_shapes";
    private string takeWithTool;
    private bool validateToolToMaterial;
    private FastTags<TagGroup.Global> silentTags = FastTags<TagGroup.Global>.Parse("silenttake");

    public override void Init() {
        base.Init();
        if (Properties.Values.ContainsKey("TakeDelay"))
            fTakeDelay = StringParsers.ParseFloat(Properties.Values["TakeDelay"]);
        if (Properties.Values.ContainsKey("HoldingItem")) itemNames = Properties.GetString("HoldingItem");
        if (Properties.Values.ContainsKey("PickUpBlock")) pickupBlock = Properties.GetString("PickUpBlock");
        if (Properties.Values.ContainsKey("ValidMaterials")) validMaterials = Properties.GetString("ValidMaterials");
        if (Properties.Values.ContainsKey("TakeWithTool")) takeWithTool = Properties.GetString("TakeWithTool");
        if (Properties.Values.ContainsKey("CheckToolForMaterial"))
            validateToolToMaterial = Properties.GetBool("CheckToolForMaterial");
    }

    // Override the on Block activated, so we can pop up our timer
    public override bool OnBlockActivated(WorldBase world, int clrIdx, Vector3i blockPos,
        BlockValue blockValue, EntityPlayerLocal player) {
        if (!ValidMaterialCheck(blockValue, player)) return false;
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
    private void TakeTarget(TimerEventData timerData) {
        var world = GameManager.Instance.World;
        var array = (object[])timerData.Data;
        var clrIdx = (int)array[0];
        var blockValue = (BlockValue)array[1];
        var vector3I = (Vector3i)array[2];
        var block = world.GetBlock(vector3I);
        var entityPlayerLocal = array[3] as EntityPlayerLocal;
        if (entityPlayerLocal == null) return;
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
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);

        if (!entityPlayerLocal.inventory.holdingItem.HasAnyTags(silentTags))
        {
            var sound = blockMaterial.SurfaceCategory + "destroy";
            entityPlayerLocal.PlayOneShot(sound);
        }

        // Damage the block for its full health 
        DamageBlock(world, clrIdx, vector3I, block, block.Block.blockMaterial.MaxDamage, entityPlayerLocal.entityId);
    }


    // Displays the UI for the timer, calling TakeTarget when its done.
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player) {
        var playerUI = (_player as EntityPlayerLocal)?.PlayerUI;
        if (playerUI == null) return;
        playerUI.windowManager.Open("timer", true);
        var xuiCTimer = playerUI.xui.GetChildByType<XUiC_Timer>();
        var timerEventData = new TimerEventData();
        timerEventData.Data = new object[] {
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
            if (_player.inventory.holdingItem.Name != item) continue;
            // Make sure the item can still be used
            if (_player.inventory.holdingItemItemValue.MaxUseTimes <= 0) continue;
            // Bump the Use time by one.
            var itemValue = _player.inventory.holdingItemItemValue;

            // Calculate the degradation value.
            itemValue.UseTimes +=
                (int)EffectManager.GetValue(PassiveEffects.DegradationPerUse, itemValue, 1f, _player);
            _player.inventory.holdingItemData.itemValue = itemValue;

            // Automatically reduce the take delay by half if you have a crow bar or claw hammer.
            newTakeTime = fTakeDelay / 2;

            // Reduce time based on the quality.
            newTakeTime -= itemValue.Quality;
            if (newTakeTime < 1)
                newTakeTime = 1;
            break;
        }

        xuiCTimer.SetTimer(newTakeTime, timerEventData);
    }

    private bool ValidMaterialCheck(BlockValue blockValue, EntityAlive entityAlive) {
        var result = false;
        var holdingItem = entityAlive.inventory.holdingItem;

        if (validateToolToMaterial)
        {
            return entityAlive.inventory.holdingItem.HasAnyTags(FastTags<TagGroup.Global>.Parse(blockMaterial.id));
        }

        // Check to see if the material is valid to pick up
        foreach (var material in validMaterials.Split(','))
        {
            if (!blockMaterial.id.Contains(material)) continue;
            result = true;
            break;
        }

        // Check to see if we are filtering based on any tool
        if (string.IsNullOrEmpty(takeWithTool)) return result;

        // Check to see if we are holding a supported tool.
        return takeWithTool.Contains(holdingItem.Name) || result;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos,
        EntityAlive _entityFocusing) {
        if (!ValidMaterialCheck(_blockValue, _entityFocusing))
            return string.Empty;
        return string.Format(Localization.Get("takeandreplace"), Localization.Get(_blockValue.Block.GetBlockName()));
        //    return "Press <E> to remove the wood from this block.";
    }
}