
public class TileEntityAoE : TileEntity
{
    public TileEntityAoE(Chunk _chunk) : base(_chunk)
    {
    }

    public override TileEntityType GetTileEntityType()
    {
        return (TileEntityType)SCoreTileEntity.TileEntityAoE;
    }

    public override bool IsActive(World world)
    {
        return true;
    }
}
