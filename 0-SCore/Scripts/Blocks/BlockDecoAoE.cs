using Audio;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockDecoAoE : BlockParticle
{ 
    public BlockDecoAoE()
    {
        HasTileEntity = true;
    }

  
    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(world, _chunk, _blockPos, _blockValue);
        if (_blockValue.ischild) return;
        
        _chunk.AddTileEntity(new TileEntityAoE(_chunk)
        {
            localChunkPos = World.toBlock(_blockPos)
        });

    }
    public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
        _chunk.RemoveTileEntityAt<TileEntityAoE>((World)world, World.toBlock(_blockPos));
    }
    public override bool IsTileEntitySavedInPrefab()
    {
        return true;
    }
}