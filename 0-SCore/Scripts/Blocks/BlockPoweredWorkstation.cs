internal class BlockPoweredWorkstationSDX : BlockWorkstation
{
    private readonly BlockActivationCommand[] cmds =
    {
        new BlockActivationCommand("open", "campfire", true),
        new BlockActivationCommand("take", "hand", false)
    };

    private readonly float TakeDelay = 2f;

    public BlockPoweredWorkstationSDX()
    {
        HasTileEntity = true;
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        if (_blockValue.ischild) return;
        _chunk.AddTileEntity(new TileEntityPoweredWorkstationSdx(_chunk)
        {
            localChunkPos = World.toBlock(_blockPos)
        });
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        base.PlaceBlock(_world, _result, _ea);

        var tileEntityWorkstation = (TileEntityPoweredWorkstationSdx)_world.GetTileEntity(_result.clrIdx, _result.blockPos);
        if (tileEntityWorkstation != null) tileEntityWorkstation.IsPlayerPlaced = true;
    }

    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        var tileEntityWorkstation = (TileEntityPoweredWorkstationSdx)_world.GetTileEntity(_cIdx, _blockPos);
        if (tileEntityWorkstation == null) return false;
        _player.AimingGun = false;
        var blockPos = tileEntityWorkstation.ToWorldPos();
        _world.GetGameManager().TELockServer(_cIdx, blockPos, tileEntityWorkstation.entityId, _player.entityId);
        return true;
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        var flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
        var tileEntityWorkstation = _world.GetTileEntity(_clrIdx, _blockPos);
        var flag2 = false;
        if (tileEntityWorkstation != null) flag2 = (tileEntityWorkstation as TileEntityPoweredWorkstationSdx).IsPlayerPlaced;
        cmds[1].enabled = flag && flag2 && TakeDelay > 0f;
        return cmds;
    }

    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        _chunk.RemoveTileEntityAt<TileEntityPoweredWorkstationSdx>((World)world, World.toBlock(_blockPos));
    }
}