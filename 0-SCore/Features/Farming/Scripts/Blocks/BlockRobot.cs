using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BlockRobot : Block
{
    private string crop = "plantedPumpkin1";
    private float speed = 20f;
    public override void LateInit()
    {
        if (Properties.Values.ContainsKey("Crop"))
            crop = Properties.Values["Crop"];
        if (Properties.Values.ContainsKey("Speed"))
            speed = StringParsers.ParseFloat(this.Properties.Values["Speed"], 0, -1, NumberStyles.Any);

        base.LateInit();
    }

    public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
    {
        _chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
        {
            bNeedsTemperature = true
        });
        world.GetWBT().AddScheduledBlockUpdate(_chunk.ClrIdx, _blockPos, this.blockID, this.GetTickRate());

    }
    public override ulong GetTickRate()
    {
        return (ulong)speed;
    }

    public virtual void Move(Vector3i _blockPos)
    {
        var currentRailPos = _blockPos + Vector3i.down;
        var nextPosition = Vector3i.zero;
        foreach( var neighbor in Vector3i.HORIZONTAL_DIRECTIONS)
        {
            var nextRailPos = currentRailPos + neighbor;
            var nextPlot = FarmPlotManager.Instance.GetFarmPlotsNearby(_blockPos);
            if (nextPlot.Visited) continue;
            
            nextPosition = nextPlot.GetBlockPos();
        }
        if (nextPosition == Vector3i.zero)
            return;

        var _ebcd = GameManager.Instance.World.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (_ebcd == null || _ebcd.transform == null)
            return;

        _ebcd.transform.Translate(nextPosition);
    }
 
    public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
    {
        //var farmPlot = FarmPlotManager.Instance.GetFarmPlotsNearby(_blockPos);
        //if (farmPlot == null)
        //{
        //    farmPlot.Manage(crop);
        //    _world.GetWBT().AddScheduledBlockUpdate(_clrIdx, _blockPos, this.blockID, this.GetTickRate());
        //    Move(_blockPos);

        //}
        return base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
    }
}

