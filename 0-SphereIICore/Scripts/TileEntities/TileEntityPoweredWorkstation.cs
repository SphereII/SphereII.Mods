public class TileEntityPoweredWorkstationSDX : TileEntityWorkstation
{

    public TileEntityPoweredBlock powered;

    public TileEntityPoweredWorkstationSDX(Chunk _chunk) : base(_chunk)
    {
        powered = new TileEntityPoweredBlock(_chunk);
    }
    public override void UpdateTick(World world)
    {
        base.UpdateTick(world);
        powered.UpdateTick(world);
    }

    public override TileEntityType GetTileEntityType()
    {
        return TileEntityType.PoweredWorkstationSDX;
    }

  
}

