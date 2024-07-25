using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockFarmPlotSDX : Block
{
    public BlockFarmPlotSDX()
    {
        base.IsNotifyOnLoadUnload = true;
    }
    public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        FarmPlotManager.Instance.Remove(_blockPos);
        base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
    }

    public override void OnBlockLoaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockLoaded(_world, _clrIdx, _blockPos, _blockValue);
        FarmPlotManager.Instance.Add(_blockPos);
    }


    public override void OnBlockUnloaded(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockUnloaded(_world, _clrIdx, _blockPos, _blockValue);
        FarmPlotManager.Instance.Remove(_blockPos);

    }

    public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        FarmPlotManager.Instance.Add(_blockPos);
    }
    public override void PlaceBlock(WorldBase _world, BlockPlacement.Result _result, EntityAlive _ea)
    {
        FarmPlotManager.Instance.Add(_result.blockPos);
        base.PlaceBlock(_world, _result, _ea);
    }

    public override string GetCustomDescription(Vector3i _blockPos, BlockValue _bv)
    {
        var localPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        if (localPlayer == null) return base.GetCustomDescription(_blockPos, _bv);
        
        if (localPlayer.playerInput.PermanentActions.Activate.IsPressed || localPlayer.playerInput.Activate.IsPressed)
        {
            return WaterPipeManager.Instance.GetWaterSummary(_blockPos);
        }
        return base.GetCustomDescription(_blockPos, _bv);
    }
}

