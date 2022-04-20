using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    public  class BlockFarmPlotSDX : Block
    {
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        FarmPlotManager.Instance.Remove(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        FarmPlotManager.Instance.Add(_blockPos);
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        FarmPlotManager.Instance.Add(_blockPos);
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
    }
    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        FarmPlotManager.Instance.Add(_result.blockPos);
        base.PlaceBlock(_world, _result, _ea);
    }
}

