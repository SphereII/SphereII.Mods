using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TileEntityPoweredPortal : TileEntityPoweredBlock, ILockable, ITileEntitySignable, ITileEntity
{
    private string signText ="";
	private bool isLocked;
	private PlatformUserIdentifierAbs ownerID;
	private List<PlatformUserIdentifierAbs> allowedUserIds;
	private string password;
    private bool isPowered;

    public TileEntityPoweredPortal(Chunk _chunk) : base(_chunk)
    {
        this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
        this.isLocked = true;
        this.ownerID = null;
        this.password = "";
    }

    public string GetText()
    {
        return this.signText;
    }

	public void SetText(string _text, bool _syncData = true)
	{
		this.signText = _text;


		var block = chunk.GetBlock(localChunkPos).Block as BlockPoweredPortal;

		if ( block != null )
			block.ToggleAnimator(localChunkPos, IsPowered);
		if ( _syncData)
			setModified();
	}

	public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
    {
        base.read(_br, _eStreamMode);
		_br.ReadInt32();
		this.isLocked = _br.ReadBoolean();
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.password = _br.ReadString();
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
		this.signText = _br.ReadString();
		this.SetText(this.signText, false);
	}

    public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
    {
        base.write(_bw, _eStreamMode);
		_bw.Write(1);
		_bw.Write(this.isLocked);
		this.ownerID.ToStream(_bw, false);
		_bw.Write(this.password);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
		_bw.Write(this.signText);
		this.SetText(this.signText, false);
	}

	public override TileEntity Clone()
	{
		return new TileEntityPoweredPortal(this.chunk)
		{
			localChunkPos = base.localChunkPos,
			isLocked = this.isLocked,
			ownerID = this.ownerID,
			password = this.password,
			allowedUserIds = new List<PlatformUserIdentifierAbs>(this.allowedUserIds),
			signText = this.signText
		};
	}
	public int GetEntityID()
	{
		return this.entityId;
	}

	//public new bool IsPowered
	//{
	//	get
	//	{
	//		var isOn = isPowered;
	//		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
	//		{
	//			if (PowerItem != null)
	//				isOn = PowerItem.IsPowered;
	//		}

	//		var block = chunk.GetBlock(localChunkPos).Block as BlockPoweredPortal;
	//		if ( block != null )
	//				block.ToggleAnimator(localChunkPos, isOn);
	//		return isOn;


	//	}
	//}
	////public override bool Activate(bool activated)
	//{
	//	World world = GameManager.Instance.World;
	//	BlockValue block = this.chunk.GetBlock(base.localChunkPos);
	//	return block.Block.ActivateBlock(world, base.GetClrIdx(), base.ToWorldPos(), block, activated, activated);
	//}

	//public void SetModified()
	//   {
	//	var block = chunk.GetBlock(localChunkPos).Block as BlockPoweredPortal;
	//	if ( block != null )
	//		block.ToggleAnimator(localChunkPos, IsPowered);
	//	setModified();
	//   }

	public override TileEntityType GetTileEntityType()
	{
		return (TileEntityType)SCoreTileEntity.TileEntityPoweredPortal;
	}
	public void SetEntityID(int _entityID)
	{
		this.entityId = _entityID;
	}
	public override void CopyFrom(TileEntity _other)
	{
		base.localChunkPos = ((TileEntityPoweredPortal)_other).localChunkPos;
		this.isLocked = ((TileEntityPoweredPortal)_other).isLocked;
		this.ownerID = ((TileEntityPoweredPortal)_other).ownerID;
		this.password = ((TileEntityPoweredPortal)_other).password;
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>(((TileEntityPoweredPortal)_other).allowedUserIds);
		this.signText = ((TileEntityPoweredPortal)_other).signText;
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
		this.setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	
	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerID)) || this.allowedUserIds.Contains(_userIdentifier);
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.password);
	}

	public string GetPassword()
	{
		return this.password;
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public bool CheckPassword(string _password, PlatformUserIdentifierAbs _userIdentifier, out bool changed)
	{
		changed = false;
		if (_userIdentifier != null && _userIdentifier.Equals(this.ownerID))
		{
			if (Utils.HashString(_password) != this.password)
			{
				changed = true;
				this.password = Utils.HashString(_password);
				this.allowedUserIds.Clear();
				this.setModified();
			}
			return true;
		}
		if (Utils.HashString(_password) == this.password)
		{
			this.allowedUserIds.Add(_userIdentifier);
			this.setModified();
			return true;
		}
		return false;
	}

}
