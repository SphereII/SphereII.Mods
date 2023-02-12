using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class BlockWaterPipeSDX : BlockBaseWaterSystem
{

    //protected BlockValue brokenPipe;

    //public override void LateInit()
    //{
    //    base.LateInit();
    //    if (this.Properties.Values.ContainsKey("BrokenPipe"))
    //        brokenPipe = ItemClass.GetItem(this.Properties.Values["BrokenPipe"], false).ToBlockValue();
    //}

    // This allows for the water logic to pass through it.
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        WaterPipeManager.Instance.ClearPipes();
        BlockUtilitiesSDX.removeParticles(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        WaterPipeManager.Instance.ClearPipes();
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
    }

    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        WaterPipeManager.Instance.ClearPipes();
        base.PlaceBlock(_world, _result, _ea);
    }
    public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
    {
        int pipeCount = 0;
        // If we still have two pipes, update the piping system
        // otherwise clear it, as we are probably disconnected.
        foreach (var neighbor in Vector3i.AllDirections)
        {
            var position = _myBlockPos + neighbor;
            var block = GameManager.Instance.World.GetBlock(position);
            if (block.Block is BlockWaterPipeSDX)
                pipeCount++;
        }

        if (_newNeighborBlockValue.isair)
            WaterPipeManager.Instance.ClearPipes();

        if (pipeCount < 2)
            WaterPipeManager.Instance.ClearPipes();
        else
            WaterPipeManager.Instance.GetWaterForPosition(_myBlockPos);

    }

  

}

