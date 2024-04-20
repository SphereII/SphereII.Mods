// using System;
//
// public class TileEntityNPCRebirth : TileEntity
// {
// 	public TileEntityNPCRebirth(Chunk _chunk) : base(_chunk)
// 	{
// 		//this.TraderData = new TraderData();
// 	}
//
// 	private TileEntityNPCRebirth(TileEntityNPCRebirth _other) : base(null)
// 	{
// 		this.bUserAccessing = _other.bUserAccessing;
// 	}
//
// 	public override TileEntity Clone()
// 	{
// 		return new TileEntityNPCRebirth(this);
// 	}
//
// 	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
// 	{
// 		base.read(_br, _eStreamMode);
// 		_br.ReadInt32();
// 	}
//
// 	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
// 	{
// 		base.write(_bw, _eStreamMode);
// 		_bw.Write(1);
// 	}
//
// 	public int GetEntityID()
// 	{
// 		return this.entityId;
// 	}
//
// 	public void SetEntityID(int _entityID)
// 	{
// 		this.entityId = _entityID;
// 	}
//
// 	public override TileEntityType GetTileEntityType()
// 	{
// 		return (TileEntityType)SCoreTileEntity.TileEntityAliveV2;
// 	}
//
// 	private const int ver = 1;
// }
