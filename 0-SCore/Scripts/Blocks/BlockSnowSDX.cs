using System.Collections.Generic;
using UnityEngine;

public class BlockSnowSDX : Block
{
    private void Spread(Vector3i _blockPos, BlockValue _blockValue)
    {
        var changes = new List<BlockChangeInfo>();
        foreach (var neighbor in Vector3i.HORIZONTAL_DIRECTIONS)
        {
            var position = _blockPos + neighbor;
            var block = GameManager.Instance.World.GetBlock(position);
            if (block.type != 0) continue;
            
            changes.Add(new BlockChangeInfo(position, _blockValue, MarchingCubes.DensityTerrain));
        }

        if (changes.Count > 0)
        {
            Debug.Log($"Spreading Snow: {_blockPos}");
            GameManager.Instance.SetBlocksRPC(changes);
        }
        
    }

    public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue,
        Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
    {
        
        base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
      //  Spread(_myBlockPos, _myBlockValue);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
      //  Spread(_blockPos, _blockValue);
    }
    
    public override ulong GetTickRate()
    {
        return 10UL;
    }

    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick,
        ulong _ticksIfLoaded, GameRandom _rnd)
    {
        Spread(_blockPos, _blockValue);
        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
        
    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        //_world.GetWBT().AddScheduledBlockUpdate(_chunk.ClrIdx, _blockPos, this.blockID, this.GetTickRate());

   //     Spread(_blockPos, _blockValue);
    }
}