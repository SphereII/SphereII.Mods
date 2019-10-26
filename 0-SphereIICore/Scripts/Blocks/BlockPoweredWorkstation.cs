class BlockPoweredWorkstationSDX : BlockWorkstation
{
    private float TakeDelay = 2f;
    private BlockActivationCommand[] cmds = new BlockActivationCommand[]
{
        new BlockActivationCommand("open", "campfire", true),
        new BlockActivationCommand("take", "hand", false)
};
    public BlockPoweredWorkstationSDX()
    {
        HasTileEntity = true;
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        if(_blockValue.ischild)
        {
            return;
        }
        _chunk.AddTileEntity(new TileEntityPoweredWorkstationSDX(_chunk)
        {
            localChunkPos = World.toBlock(_blockPos)
        });
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
    
        base.PlaceBlock(_world, _result, _ea);

        TileEntityPoweredWorkstationSDX tileEntityWorkstation = (TileEntityPoweredWorkstationSDX)_world.GetTileEntity(_result.clrIdx, _result.blockPos);
        if(tileEntityWorkstation != null)
        {
            tileEntityWorkstation.IsPlayerPlaced = true;
        }
    }

    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        TileEntityPoweredWorkstationSDX tileEntityWorkstation = (TileEntityPoweredWorkstationSDX)_world.GetTileEntity(_cIdx, _blockPos);
        if(tileEntityWorkstation == null)
        {
            return false;
        }
        _player.AimingGun = false;
        Vector3i blockPos = tileEntityWorkstation.ToWorldPos();
        _world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityWorkstation.entityId, _player.entityId);
        return true;
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
        TileEntity tileEntityWorkstation = (TileEntity)_world.GetTileEntity(_clrIdx, _blockPos);
        bool flag2 = false;
        if(tileEntityWorkstation != null)
        {
            flag2 = (tileEntityWorkstation as TileEntityPoweredWorkstationSDX).IsPlayerPlaced;
        }
        this.cmds[1].enabled = (flag && flag2 && this.TakeDelay > 0f);
        return this.cmds;
    }

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        _chunk.RemoveTileEntityAt<TileEntityPoweredWorkstationSDX>((World)world, World.toBlock(_blockPos));
    }


}

