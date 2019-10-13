class TileEntityPoweredWorkstationSDX : TileEntityPowered
{
    TileEntityWorkstation workstation;

    public TileEntityPoweredWorkstationSDX(Chunk _chunk) : base(_chunk)
    {
        workstation = new TileEntityWorkstation(_chunk);
    }
    public override void UpdateTick(World world)
    {
        base.UpdateTick(world);
        workstation.UpdateTick(world);
    }

    public override TileEntityType GetTileEntityType()
    {
        return TileEntityType.Workstation;
    }

  
}

