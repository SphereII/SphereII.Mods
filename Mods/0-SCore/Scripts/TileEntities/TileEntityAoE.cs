using UnityEngine;

public class TileEntityAoE : TileEntity
{
    public TileEntityAoE(Chunk _chunk) : base(_chunk)
    {
    }
    private TileEntityAoE(TileEntityAoE _other) : base(null)
    {
        localChunkPos = _other.localChunkPos;
    }
    
    public override void CopyFrom(TileEntity _other)
    {
        localChunkPos = _other.localChunkPos;
    }
    public override TileEntityType GetTileEntityType()
    {
        return (TileEntityType)SCoreTileEntity.TileEntityAoE;
    }

    public override void Reset(FastTags<TagGroup.Global> questTags)
    {
        base.Reset(questTags);
        setModified();
    }

    public override bool IsActive(World world)
    {
        return true;
    }
    public override TileEntity Clone()
    {
        return new TileEntityAoE(this);
    }
}