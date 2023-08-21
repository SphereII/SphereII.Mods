
using UnityEngine;

public class TileEntityAoE : TileEntity
{
    public TileEntityAoE(Chunk _chunk) : base(_chunk)
    {
    }
    private TileEntityAoE(TileEntityAoE _other) : base(null)
    {
        Debug.Log("Creating new TileEntity");
        localChunkPos = _other.localChunkPos;
    }
    
    public override void CopyFrom(TileEntity _other)
    {
        Debug.Log("Copy From TileEntity");
        localChunkPos = _other.localChunkPos;
    }
    public override TileEntityType GetTileEntityType()
    {
        return (TileEntityType)SCoreTileEntity.TileEntityAoE;
    }

    public override void Reset(FastTags questTags)
    {
        Debug.Log("Resetting TileEntity");
        base.Reset(questTags);
        setModified();
    }

    public override bool IsActive(World world)
    {
        return true;
    }
    public override TileEntity Clone()
    {
        Debug.Log("Cloning TileEntity");
        return new TileEntityAoE(this);
    }
}
