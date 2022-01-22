//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//public class NetPackageGotoPOISDX : NetPackage
//    {
//	public NetPackageGotoPOISDX Setup(int _playerId, QuestTags _questTags, int _questCode, NetPackageQuestGotoPoint.QuestGotoTypes _gotoType, byte _difficulty, int posX = 0, int posZ = -1, float sizeX = 0f, float sizeY = 0f, float sizeZ = 0f, float offset = -1f, BiomeFilterTypes _biomeFilterType = BiomeFilterTypes.AnyBiome, string _biomeFilter = "", string _POIFilter)
//	{
//		this.playerId = _playerId;
//		this.questCode = _questCode;
//		this.GotoType = _gotoType;
//		this.questTags = _questTags;
//		this.position = new Vector2((float)posX, (float)posZ);
//		this.size = new Vector3(sizeX, sizeY, sizeZ);
//		this.difficulty = _difficulty;
//		this.biomeFilterType = _biomeFilterType;
//		this.biomeFilter = _biomeFilter;
//		poiFilter = _POIFilter; 
//		return this;
//	}

//	public override void read(PooledBinaryReader _br)
//	{
//		this.playerId = _br.ReadInt32();
//		this.questCode = _br.ReadInt32();
//		this.GotoType = (NetPackageQuestGotoPoint.QuestGotoTypes)_br.ReadByte();
//		this.questTags = (QuestTags)_br.ReadByte();
//		this.position = new Vector2((float)_br.ReadInt32(), (float)_br.ReadInt32());
//		this.size = StreamUtils.ReadVector3(_br);
//		this.difficulty = _br.ReadByte();
//		this.biomeFilterType = (BiomeFilterTypes)_br.ReadByte();
//		this.biomeFilter = _br.ReadString();
//		this.poiFilter = _br.ReadString();

//	}

//	public override void write(PooledBinaryWriter _bw)
//	{
//		base.write(_bw);
//		_bw.Write(this.playerId);
//		_bw.Write(this.questCode);
//		_bw.Write((byte)this.GotoType);
//		_bw.Write((byte)this.questTags);
//		_bw.Write((int)this.position.x);
//		_bw.Write((int)this.position.y);
//		StreamUtils.Write(_bw, this.size);
//		_bw.Write(this.difficulty);
//		_bw.Write((byte)this.biomeFilterType);
//		_bw.Write(this.biomeFilter);
//		_bw.Write(this.poiFilter);

//	}

//	public override void ProcessPackage(World _world, GameManager _callbacks)
//	{
//		if (_world == null)
//		{
//			return;
//		}
//		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
//		{
//			for (int i = 0; i < 5; i++)
//			{
//				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.playerId) as EntityPlayer;
//				PrefabInstance prefabInstance = null;
//				if (this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Trader)
//				{
//					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(this.questTags, new Vector2(entityPlayer.position.x, entityPlayer.position.z), null, -1, false, this.biomeFilterType, this.biomeFilter);
//				}
//				else if (this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Closest)
//				{
//					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetClosestPOIToWorldPos(this.questTags, new Vector2(entityPlayer.position.x, entityPlayer.position.z), null, -1, false, BiomeFilterTypes.AnyBiome, "");
//				}
//				else if (this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI)
//				{
//					prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityPlayer.position.x, entityPlayer.position.z), 100, 50000000, this.questTags, this.difficulty, null, -1, BiomeFilterTypes.AnyBiome, "");
//				}
//				new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
//				if (prefabInstance != null)
//				{
//					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(this.playerId, this.questTags, this.questCode, this.GotoType, this.difficulty, prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, (float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z, -1f, BiomeFilterTypes.AnyBiome, ""), false, this.playerId, -1, -1, -1);
//					return;
//				}
//			}
//			return;
//		}
//		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
//		Quest quest = primaryPlayer.QuestJournal.FindActiveQuest(this.questCode);
//		if (quest != null)
//		{
//			for (int j = 0; j < quest.Objectives.Count; j++)
//			{
//				if (quest.Objectives[j] is ObjectiveGoto && this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Trader)
//				{
//					((ObjectiveGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
//				}
//				else if (quest.Objectives[j] is ObjectiveClosestPOIGoto && this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.Closest)
//				{
//					((ObjectiveClosestPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
//				}
//				else if (quest.Objectives[j] is ObjectiveRandomPOIGoto && this.GotoType == NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI)
//				{
//					((ObjectiveRandomPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
//				}
//			}
//		}
//	}

//	// Token: 0x060031F9 RID: 12793 RVA: 0x0015A74B File Offset: 0x0015894B
//	public override int GetLength()
//	{
//		return 8;
//	}

//	// Token: 0x04002212 RID: 8722
//	private int playerId;

//	// Token: 0x04002213 RID: 8723
//	private int questCode;

//	// Token: 0x04002214 RID: 8724
//	private QuestTags questTags;

//	// Token: 0x04002215 RID: 8725
//	private Vector2 position;

//	// Token: 0x04002216 RID: 8726
//	private Vector3 size;

//	// Token: 0x04002217 RID: 8727
//	private byte difficulty;

//	// Token: 0x04002218 RID: 8728
//	public NetPackageQuestGotoPoint.QuestGotoTypes GotoType;

//	// Token: 0x04002219 RID: 8729
//	private BiomeFilterTypes biomeFilterType;

//	// Token: 0x0400221A RID: 8730
//	private string biomeFilter;

//	private string poiFilter;

//	// Token: 0x02000E88 RID: 3720
//	public enum QuestGotoTypes
//	{
//		// Token: 0x04006734 RID: 26420
//		Trader,
//		// Token: 0x04006735 RID: 26421
//		Closest,
//		// Token: 0x04006736 RID: 26422
//		RandomPOI
//	}
//}

