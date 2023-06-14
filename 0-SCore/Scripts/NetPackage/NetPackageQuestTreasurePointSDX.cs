using System;
using UnityEngine;

// Token: 0x020004C7 RID: 1223
public class NetPackageQuestTreasurePointSDX : NetPackage
{
	// Token: 0x060031F4 RID: 12788 RVA: 0x0015A280 File Offset: 0x00158480
	public NetPackageQuestTreasurePointSDX Setup(int _playerId, float _distance, int _offset, int _questCode, int posX = 0, int posY = -1, int posZ = 0, bool _useNearby = false)
	{
		this.playerId = _playerId;
		this.distance = _distance;
		this.offset = _offset;
		this.questCode = _questCode;
		this.position = new Vector3i(posX, posY, posZ);
		this.useNearby = _useNearby;
		this.treasureOffset = Vector3.zero;
		this.ActionType = QuestPointActions.GetGotoPoint;
		return this;
	}

	// Token: 0x060031F5 RID: 12789 RVA: 0x0015A2D8 File Offset: 0x001584D8
	public NetPackageQuestTreasurePointSDX Setup(int _playerId, int _questCode, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		this.playerId = _playerId;
		this.distance = 0f;
		this.offset = 0;
		this.questCode = _questCode;
		this.position = _position;
		this.treasureOffset = _treasureOffset;
		this.blocksPerReduction = _blocksPerReduction;
		this.ActionType = QuestPointActions.GetTreasurePoint;
		return this;
	}

	// Token: 0x060031F6 RID: 12790 RVA: 0x0015A324 File Offset: 0x00158524
	public NetPackageQuestTreasurePointSDX Setup(int _questCode, float _distance, int _offset, float _treasureRadius, Vector3 _startPosition, int _playerId, bool _useNearby, int _blocksPerReduction)
	{
		this.playerId = _playerId;
		this.distance = _distance;
		this.offset = _offset;
		this.questCode = _questCode;
		this.treasureRadius = _treasureRadius;
		this.position = new Vector3i(_startPosition);
		this.useNearby = _useNearby;
		this.treasureOffset = Vector3.zero;
		this.blocksPerReduction = _blocksPerReduction;
		this.ActionType = QuestPointActions.GetTreasurePoint;
		return this;
	}

	// Token: 0x060031F7 RID: 12791 RVA: 0x0015A386 File Offset: 0x00158586
	public NetPackageQuestTreasurePointSDX Setup(int _questCode, Vector3i _updatedPosition)
	{
		this.questCode = _questCode;
		this.position = _updatedPosition;
		this.ActionType = QuestPointActions.UpdateTreasurePoint;
		return this;
	}

	// Token: 0x060031F8 RID: 12792 RVA: 0x0015A39E File Offset: 0x0015859E
	public NetPackageQuestTreasurePointSDX Setup(int _questCode, int _blocksPerReduction)
	{
		this.questCode = _questCode;
		this.blocksPerReduction = _blocksPerReduction;
		this.ActionType = QuestPointActions.UpdateBlocksPerReduction;
		return this;
	}

	// Token: 0x060031F9 RID: 12793 RVA: 0x0015A3B8 File Offset: 0x001585B8
	public override void read(PooledBinaryReader _br)
	{
		this.ActionType = (QuestPointActions)_br.ReadByte();
		if (this.ActionType == QuestPointActions.UpdateTreasurePoint)
		{
			this.questCode = _br.ReadInt32();
			this.position = StreamUtils.ReadVector3i(_br);
			return;
		}
		this.playerId = _br.ReadInt32();
		this.distance = _br.ReadSingle();
		this.offset = _br.ReadInt32();
		this.treasureRadius = _br.ReadSingle();
		this.blocksPerReduction = _br.ReadInt32();
		this.questCode = _br.ReadInt32();
		this.position = StreamUtils.ReadVector3i(_br);
		this.treasureOffset = StreamUtils.ReadVector3(_br);
	}

	// Token: 0x060031FA RID: 12794 RVA: 0x0015A454 File Offset: 0x00158654
	public override void write(PooledBinaryWriter _bw)
	{
		_bw.Write((byte)this.PackageId);
		_bw.Write((byte)this.ActionType);
		if (this.ActionType == QuestPointActions.UpdateTreasurePoint)
		{
			_bw.Write(this.questCode);
			StreamUtils.Write(_bw, this.position);
			return;
		}
		_bw.Write(this.playerId);
		_bw.Write(this.distance);
		_bw.Write(this.offset);
		_bw.Write(this.treasureRadius);
		_bw.Write(this.blocksPerReduction);
		_bw.Write(this.questCode);
		StreamUtils.Write(_bw, this.position);
		StreamUtils.Write(_bw, this.treasureOffset);
	}

	// Token: 0x060031FB RID: 12795 RVA: 0x0015A4F8 File Offset: 0x001586F8
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
            var player= GameManager.Instance.World.GetPrimaryPlayer();
			for (int i = 0; i < 15; i++)
			{
                if (ObjectiveTreasureChest.CalculateTreasurePoint(player.position, distance, this.offset, treasureRadius,useNearby, out position, out treasureOffset))
                {
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestTreasurePointSDX>().Setup(this.playerId, this.distance, this.offset, this.questCode, position.x, position.y, position.z, false), false, this.playerId, -1, -1, -1);
					return;
				}
			}
			return;
		}
		Quest quest = GameManager.Instance.World.GetPrimaryPlayer().QuestJournal.FindActiveQuest(this.questCode);
		if (quest != null)
		{
			for (int j = 0; j < quest.Objectives.Count; j++)
			{
				if (quest.Objectives[j] is ObjectiveTreasureChest)
				{
					((ObjectiveTreasureChest)quest.Objectives[j]).FinalizePoint(this.position.x, this.position.y, this.position.z);
				}
				else if (quest.Objectives[j] is ObjectiveRandomGotoSDX)
				{
					((ObjectiveRandomGotoSDX)quest.Objectives[j]).FinalizePoint(this.position.x, this.position.y, this.position.z);
				}
			}
		}

	}

	// Token: 0x060031FC RID: 12796 RVA: 0x0015A70B File Offset: 0x0015890B
	public override int GetLength()
	{
		//Log.Out("NetPackageQuestTreasurePointSDX-GetLength-Start");
		return 8;
	}

	// Token: 0x04002215 RID: 8725
	private int playerId;

	// Token: 0x04002216 RID: 8726
	private float distance;

	// Token: 0x04002217 RID: 8727
	private int offset;

	// Token: 0x04002218 RID: 8728
	private float treasureRadius;

	// Token: 0x04002219 RID: 8729
	private int questCode;

	// Token: 0x0400221A RID: 8730
	private Vector3i position;

	// Token: 0x0400221B RID: 8731
	private bool useNearby;

	// Token: 0x0400221C RID: 8732
	private Vector3 treasureOffset = Vector3.zero;

	// Token: 0x0400221D RID: 8733
	private int blocksPerReduction;

	// Token: 0x0400221E RID: 8734
	private QuestPointActions ActionType;

	// Token: 0x02000E85 RID: 3717
	private enum QuestPointActions
	{
		// Token: 0x0400670C RID: 26380
		GetGotoPoint,
		// Token: 0x0400670D RID: 26381
		GetTreasurePoint,
		// Token: 0x0400670E RID: 26382
		UpdateTreasurePoint,
		// Token: 0x0400670F RID: 26383
		UpdateBlocksPerReduction
	}
}
