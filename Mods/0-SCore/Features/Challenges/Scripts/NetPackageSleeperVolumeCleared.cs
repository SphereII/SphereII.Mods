
using System.Collections.Generic;
using UnityEngine;



public class NetPackageSleeperVolumeCleared : NetPackage
{
	
	public int entityID;
	public Vector3 position;

	public NetPackageSleeperVolumeCleared Setup(Vector3 _position,  int _entityID)
	{
		this.entityID = _entityID;
		this.position = _position;
		return this;
	}

	

	public override void read(PooledBinaryReader _reader)
	{
		this.entityID = _reader.ReadInt32();
		this.position = StreamUtils.ReadVector3(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityID);
		StreamUtils.Write(_writer, this.position);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null) return;

		var entityPlayer = _world.GetEntity(entityID) as EntityPlayerLocal;
		if (entityPlayer == null)
		{
			return;
		}
		
		if (Vector3.Distance(position, entityPlayer.position) < GameStats.GetInt(EnumGameStats.PartySharedKillRange))
		{
			EventOnSleeperVolumeClearedUpdate.SleeperVolumeCleared(position);
		}
	}

	
	
	
}
