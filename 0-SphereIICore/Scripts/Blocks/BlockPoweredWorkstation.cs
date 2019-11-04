using System;
using System.Globalization;
using UnityEngine;
// Token: 0x02000201 RID: 513
public class BlockWorkstationPoweredSDX : BlockParticle
{
    // Token: 0x06001009 RID: 4105 RVA: 0x00062D14 File Offset: 0x00060F14
    public BlockWorkstationPoweredSDX()
    {
        this.HasTileEntity = true;
    }

    public override void Init()
    {
        base.Init();
        if (this.Properties.Values.ContainsKey("TakeDelay"))
        {
            this.TakeDelay = StringParsers.ParseFloat(this.Properties.Values["TakeDelay"], 0, -1, NumberStyles.Any);
        }
        else
        {
            this.TakeDelay = 2f;
        }
        this.WorkstationData = new WorkstationData(base.GetBlockName(), this.Properties);
        CraftingManager.AddWorkstationData(this.WorkstationData);
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        if (_blockValue.ischild)
        {
            return;
        }
        _chunk.AddTileEntity(new TileEntityPoweredWorkstationSDX(_chunk)
        {
            localChunkPos = World.toBlock(_blockPos)
        });
    }

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        _chunk.RemoveTileEntityAt<TileEntityPoweredWorkstationSDX>((World)world, World.toBlock(_blockPos));
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        base.PlaceBlock(_world, _result, _ea);
        TileEntityPoweredWorkstationSDX tileEntityWorkstation = (TileEntityPoweredWorkstationSDX)_world.GetTileEntity(_result.clrIdx, _result.blockPos);
        if (tileEntityWorkstation != null)
        {
            tileEntityWorkstation.IsPlayerPlaced = true;
        }
    }

    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        Debug.Log("OnBlockActivated() 2");

        TileEntityPoweredWorkstationSDX tileEntityWorkstation = (TileEntityPoweredWorkstationSDX)_world.GetTileEntity(_cIdx, _blockPos);
        if (tileEntityWorkstation == null)
        {
            Debug.Log("OnBlockActivated() is null");
            return false;
        }
        _player.AimingGun = false;
        Vector3i blockPos = tileEntityWorkstation.ToWorldPos();
        Debug.Log("OnBlockActivated() 3");
        _world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityWorkstation.entityId, _player.entityId);
        return true;
    }

    public override void OnBlockValueChanged(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
    {
        base.OnBlockValueChanged(_world, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
        this.checkParticles(_world, _clrIdx, _blockPos, _newBlockValue);
    }

    public override byte GetLightValue(BlockValue _blockValue)
    {
        return 0;
    }

    protected override void checkParticles(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        bool flag = _world.GetGameManager().HasBlockParticleEffect(_blockPos);
        if (_blockValue.meta != 0 && !flag)
        {
            this.addParticles(_world, _clrIdx, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
            return;
        }
        if (_blockValue.meta == 0 && flag)
        {
            this.removeParticles(_world, _blockPos.x, _blockPos.y, _blockPos.z, _blockValue);
        }
    }

    public static bool IsLit(BlockValue _blockValue)
    {
        return _blockValue.meta > 0;
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        return Localization.Get("useWorkstation", "");
    }

    public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        Debug.Log("OnBlockActivated: " + _indexInBlockActivationCommands);
        if (_indexInBlockActivationCommands == 0)
        {
            return this.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
        }
        if (_indexInBlockActivationCommands != 1)
        {
            return false;
        }
        this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
        return true;
    }

    // Token: 0x06001015 RID: 4117 RVA: 0x00062FA File Offset: 0x000611AC
    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
        TileEntityPoweredWorkstationSDX tileEntityWorkstation = (TileEntityPoweredWorkstationSDX)_world.GetTileEntity(_clrIdx, _blockPos);
        bool flag2 = false;
        if (tileEntityWorkstation != null)
        {
            flag2 = tileEntityWorkstation.IsPlayerPlaced;
        }
        this.cmds[1].enabled = (flag && flag2 && this.TakeDelay > 0f);
        return this.cmds;
    }

    // Token: 0x06001016 RID: 4118 RVA: 0x00063018 File Offset: 0x00061218
    public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (_blockValue.damage > 0)
        {
            GameManager.ShowTooltipWithAlert(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup", ""), "ui_denied");
            return;
        }
        LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
        playerUI.windowManager.Open("timer", true, false, true);
        XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
        TimerEventData timerEventData = new TimerEventData();
        timerEventData.Data = new object[]
        {
            _cIdx,
            _blockValue,
            _blockPos,
            _player
        };
        timerEventData.Event += this.EventData_Event;
        childByType.SetTimer(this.TakeDelay, timerEventData, -1f, "");
    }

    // Token: 0x06001017 RID: 4119 RVA: 0x000630D4 File Offset: 0x000612D4
    private void EventData_Event(TimerEventData timerData)
    {
        World world = GameManager.Instance.World;
        object[] array = (object[])timerData.Data;
        int clrIdx = (int)array[0];
        BlockValue blockValue = (BlockValue)array[1];
        Vector3i vector3i = (Vector3i)array[2];
        BlockValue block = world.GetBlock(vector3i);
        EntityPlayerLocal entityPlayerLocal = array[3] as EntityPlayerLocal;
        if (block.damage > 0)
        {
            GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttRepairBeforePickup", ""), "ui_denied");
            return;
        }
        if (block.type != blockValue.type)
        {
            GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttBlockMissingPickup", ""), "ui_denied");
            return;
        }
        TileEntityPoweredWorkstationSDX tileEntityWorkstation = world.GetTileEntity(clrIdx, vector3i) as TileEntityPoweredWorkstationSDX;
        if (tileEntityWorkstation.IsUserAccessing())
        {
            GameManager.ShowTooltipWithAlert(entityPlayerLocal, Localization.Get("ttCantPickupInUse", ""), "ui_denied");
            return;
        }
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
        this.HandleTakeInternalItems(tileEntityWorkstation, uiforPlayer);
        ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
        if (!uiforPlayer.xui.PlayerInventory.AddItem(itemStack, true))
        {
            uiforPlayer.xui.PlayerInventory.DropItem(itemStack);
        }
        world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
    }

    // Token: 0x06001018 RID: 4120 RVA: 0x00063204 File Offset: 0x00061404
    protected virtual void HandleTakeInternalItems(TileEntityPoweredWorkstationSDX te, LocalPlayerUI playerUI)
    {
        ItemStack[] array = te.Output;
        for (int i = 0; i < array.Length; i++)
        {
            if (!array[i].IsEmpty() && !playerUI.xui.PlayerInventory.AddItem(array[i], true))
            {
                playerUI.xui.PlayerInventory.DropItem(array[i]);
            }
        }
        array = te.Tools;
        for (int j = 0; j < array.Length; j++)
        {
            if (!array[j].IsEmpty() && !playerUI.xui.PlayerInventory.AddItem(array[j], true))
            {
                playerUI.xui.PlayerInventory.DropItem(array[j]);
            }
        }
        array = te.Fuel;
        for (int k = 0; k < array.Length; k++)
        {
            if (!array[k].IsEmpty() && !playerUI.xui.PlayerInventory.AddItem(array[k], true))
            {
                playerUI.xui.PlayerInventory.DropItem(array[k]);
            }
        }
    }

    // Token: 0x04000CB1 RID: 3249
    protected int lootList;

    // Token: 0x04000CB2 RID: 3250
    private float TakeDelay = 2f;

    // Token: 0x04000CB3 RID: 3251
    public WorkstationData WorkstationData;

    // Token: 0x04000CB4 RID: 3252
    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
    {
        new BlockActivationCommand("open", "campfire", true),
        new BlockActivationCommand("take", "hand", false)
    };
}
