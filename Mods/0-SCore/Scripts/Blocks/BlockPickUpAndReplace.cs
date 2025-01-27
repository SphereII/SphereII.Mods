using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UniLinq;
using UnityEngine;

public class BlockTakeAndReplace : Block {
    private const string AdvFeatureClass = "AdvancedPickUpAndPlace";

    // By default, all blocks using this class will have a take delay of 15 seconds, unless over-ridden by the XML.
    private float fTakeDelay = 6f;
    private string itemNames = "meleeToolRepairT1ClawHammer";
    private string pickupBlock;
    private string validMaterials = "Mwood_weak,Mwood_weak_shapes,Mwood_shapes";
    private string takeWithTool;
    private bool checkToolForMaterial;
    private static FastTags<TagGroup.Global> silentTags = FastTags<TagGroup.Global>.Parse("silenttake");
    private bool legacy = true;
    private bool triggerHarvest = false;

    public override void Init() {
        base.Init();
        if (Properties.Values.ContainsKey("TakeDelay"))
            fTakeDelay = StringParsers.ParseFloat(Properties.Values["TakeDelay"]);

        if (Properties.Values.ContainsKey("HoldingItem")) itemNames = Properties.GetString("HoldingItem");
        if (Properties.Values.ContainsKey("PickUpBlock")) pickupBlock = Properties.GetString("PickUpBlock");
        if (Properties.Values.ContainsKey("ValidMaterials")) validMaterials = Properties.GetString("ValidMaterials");

        if (Properties.Values.ContainsKey("CheckToolForMaterial"))
            checkToolForMaterial = Properties.GetBool("CheckToolForMaterial");

        // If we use the TakeWithTool on this block or shape, clear the default items.
        if (Properties.Values.ContainsKey("TakeWithTool"))
        {
            takeWithTool = Properties.GetString("TakeWithTool");
            itemNames = takeWithTool;
        }

        if (Properties.Values.ContainsKey("HarvestOnPickUp"))
            triggerHarvest = Properties.GetBool("HarvestOnPickUp");
    }

    public override void LateInit() {
        base.LateInit();
        // This needs to be in the LateInit, as all the changes to the config block won't be available.

        // Check to see if we want to do the legacy or a simplistic method of pulling wood boards off.
        var result = Configuration.GetPropertyValue(AdvFeatureClass, "Legacy");
        if (!string.IsNullOrEmpty(result))
        {
            legacy = StringParsers.ParseBool(result);
            if (legacy)
            {
                if (Properties.Values.ContainsKey("HoldingItem")) itemNames = Properties.GetString("HoldingItem");
                return;
            }
        }

        // See if we are using a key for the material in teh config block
        var globalMaterial = Configuration.GetPropertyValue(AdvFeatureClass, validMaterials);
        if (!string.IsNullOrEmpty(globalMaterial))
            validMaterials = globalMaterial;


        var globalTool = Configuration.GetPropertyValue(AdvFeatureClass, takeWithTool);
        if (!string.IsNullOrEmpty(globalTool))
        {
            takeWithTool = globalTool;
            itemNames = globalTool;
        }
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

        if (triggerHarvest)
        {
            Harvest(block, entityPlayerLocal);
            DamageBlock(world, clrIdx, vector3I, block, block.Block.blockMaterial.MaxDamage, entityPlayerLocal.entityId);
            return;
        }

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

        // Damage the block for its full health 
        DamageBlock(world, clrIdx, vector3I, block, block.Block.blockMaterial.MaxDamage, entityPlayerLocal.entityId);
    
    }

    private void Harvest(BlockValue blockValue, EntityPlayerLocal player) {
        if (!blockValue.Block.HasItemsToDropForEvent(EnumDropEvent.Harvest)) return;
        if (GameUtils.random == null)
        {
            GameUtils.random = GameRandomManager.Instance.CreateGameRandom();
            GameUtils.random.SetSeed((int)GameManager.Instance.World.GetWorldTime());
        }
        
        var itemDropProb = blockValue.Block.itemsToDrop[EnumDropEvent.Harvest];
        for (var i = 0; i < itemDropProb.Count; i++)
        {
            var num2 = EffectManager.GetValue(PassiveEffects.HarvestCount, blockValue.ToItemValue(), 1f,
                player, null, FastTags<TagGroup.Global>.Parse(itemDropProb[i].tag));
            var itemValue = new ItemValue(ItemClass.GetItem(itemDropProb[i].name, false).type, false);
            if (itemValue.type == 0 || ItemClass.list[itemValue.type] == null || (!(itemDropProb[i].prob > 0.999f) &&
                    !(GameUtils.random.RandomFloat <= itemDropProb[i].prob))) continue;
            
            var count = (int)(GameUtils.random.RandomRange(itemDropProb[i].minCount, itemDropProb[i].maxCount + 1) * num2);
            if (count <= 0) continue;
            var itemStack = new ItemStack(itemValue, count);
            var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player);
            var playerInventory = uiforPlayer.xui.PlayerInventory;
            QuestEventManager.Current.HarvestedItem(itemValue, itemStack, blockValue);
            if (playerInventory.AddItem(itemStack)) continue;
            var dropPos = GameManager.Instance.World.GetPrimaryPlayer().GetDropPosition();
            GameManager.Instance.ItemDropServer(new ItemStack(itemValue, itemStack.count),dropPos, new Vector3(0.5f, 0.5f, 0.5f));
        }
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
        if (legacy) return true;
        // If we do not have a valid material to pick up, then skip all the other checks
        if (!validMaterials.Contains(blockMaterial.id))
        {
            return false;
        }

        // Do we need to check the tools?
        if (checkToolForMaterial)
        {
            // Check to see if we are filtering based on any tool
            return takeWithTool.Contains(entityAlive.inventory.holdingItem.Name);
        }

        return entityAlive.inventory.holdingItem.HasAnyTags(FastTags<TagGroup.Global>.Parse(blockMaterial.id));
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos,
        EntityAlive _entityFocusing) {
        if (!ValidMaterialCheck(_blockValue, _entityFocusing))
            return string.Empty;
        return string.Format(Localization.Get("takeandreplace"), Localization.Get(_blockValue.Block.GetBlockName()));
    }
}