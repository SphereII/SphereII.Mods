internal class TileEntityPoweredWorkstationSdx : TileEntityPowered
{
    private readonly TileEntityWorkstation _workstation;

    public TileEntityPoweredWorkstationSdx(Chunk _chunk) : base(_chunk)
    {
        _workstation = new TileEntityWorkstation(_chunk);
    }

    public override void UpdateTick(World world)
    {
        base.UpdateTick(world);
        _workstation.UpdateTick(world);
    }

    public override TileEntityType GetTileEntityType()
    {
        return TileEntityType.Workstation;
    }
}