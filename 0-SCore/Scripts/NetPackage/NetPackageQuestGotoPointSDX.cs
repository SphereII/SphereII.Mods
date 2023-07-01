using System;
using UnityEngine;

// Token: 0x020004C5 RID: 1221
public class NetPackageQuestGotoPointSDX : NetPackage
{
	// Token: 0x060031E6 RID: 12774 RVA: 0x0015996C File Offset: 0x00157B6C
	public NetPackageQuestGotoPointSDX Setup(int _playerId, FastTags _questTags, int _questCode, NetPackageQuestGotoPointSDX.QuestGotoTypes _gotoType, byte _difficulty, int posX = 0, int posZ = -1, float sizeX = 0f, float sizeY = 0f, float sizeZ = 0f, float offset = -1f, BiomeFilterTypes _biomeFilterType = BiomeFilterTypes.AnyBiome, string _biomeFilter = "")
	{
		//Log.Out("NetPackageQuestGotoPointSDX-Setup");
		this.playerId = _playerId;
		this.questCode = _questCode;
		this.GotoType = _gotoType;
		this.questTags = _questTags;
		this.position = new Vector2((float)posX, (float)posZ);
		this.size = new Vector3(sizeX, sizeY, sizeZ);
		this.difficulty = _difficulty;
		this.biomeFilterType = _biomeFilterType;
		this.biomeFilter = _biomeFilter;
		return this;
	}

	// Token: 0x060031E7 RID: 12775 RVA: 0x001599D4 File Offset: 0x00157BD4
	public override void read(PooledBinaryReader _br)
	{
		//Log.Out("NetPackageQuestGotoPointSDX-read");
		this.playerId = _br.ReadInt32();
		this.questCode = _br.ReadInt32();
		this.GotoType = (NetPackageQuestGotoPointSDX.QuestGotoTypes)_br.ReadByte();
		this.questTags = FastTags.Parse(_br.ReadString());
		this.position = new Vector2((float)_br.ReadInt32(), (float)_br.ReadInt32());
		this.size = StreamUtils.ReadVector3(_br);
		this.difficulty = _br.ReadByte();
		this.biomeFilterType = (BiomeFilterTypes)_br.ReadByte();
		this.biomeFilter = _br.ReadString();
	}

	// Token: 0x060031E8 RID: 12776 RVA: 0x00159A5C File Offset: 0x00157C5C
	public override void write(PooledBinaryWriter _bw)
	{
		//Log.Out("NetPackageQuestGotoPointSDX-write");
		base.write(_bw);
		_bw.Write(this.playerId);
		_bw.Write(this.questCode);
		_bw.Write((byte)this.GotoType);
		_bw.Write(this.questTags.ToString());
		_bw.Write((int)this.position.x);
		_bw.Write((int)this.position.y);
		StreamUtils.Write(_bw, this.size);
		_bw.Write(this.difficulty);
		_bw.Write((byte)this.biomeFilterType);
		_bw.Write(this.biomeFilter);
	}

	// Token: 0x060031E9 RID: 12777 RVA: 0x00159AF8 File Offset: 0x00157CF8
	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		//Log.Out("NetPackageQuestGotoPointSDX-ProcessPackage");
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			//Log.Out("NetPackageQuestGotoPointSDX-ProcessPackage-Server");
			for (int i = 0; i < 5; i++)
			{
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.playerId) as EntityPlayer;
				PrefabInstance prefabInstance = null;
				prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityPlayer.position.x, entityPlayer.position.z), 100, 4000000, this.questTags, this.difficulty, null, -1, BiomeFilterTypes.AnyBiome, "");

				new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);

				float numPlayerX = entityPlayer.position.x;
				float numPlayerZ = entityPlayer.position.z;

				if (prefabInstance != null)
				{
					Vector2 vector3a = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);

					float numPreFabX = (float)vector3a.x;
					float numPreFabZ = (float)vector3a.y;

					float numDiffXa = Math.Abs(numPreFabX - numPlayerX);
					float numDiffZa = Math.Abs(numPreFabZ - numPlayerZ);
					float numDiffa = Math.Abs(numDiffXa + numDiffZa);

					//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Prefab Name:" + prefabInstance.name);

					//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Original Prefab diffXa:" + numDiffXa + ", diffZa: " + numDiffZa);
					//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Original Prefab diff:" + numDiffa);

					int minDistance = 400;
					int maxDistance = 900;

					//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-minDistance: " + minDistance + ", maxDistance: " + maxDistance);

					for (int j = 0; j < 15; j++)
					{
						PrefabInstance prefabInstance2;
						prefabInstance2 = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityPlayer.position.x, entityPlayer.position.z), 100, 4000000, this.questTags, this.difficulty, null, -1, BiomeFilterTypes.AnyBiome, "");

						Vector2 vector3b = new Vector2((float)prefabInstance2.boundingBoxPosition.x + (float)prefabInstance2.boundingBoxSize.x / 2f, (float)prefabInstance2.boundingBoxPosition.z + (float)prefabInstance2.boundingBoxSize.z / 2f);

						numPreFabX = (float)vector3b.x;
						numPreFabZ = (float)vector3b.y;

						float numDiffXb = Math.Abs(numPreFabX - numPlayerX);
						float numDiffZb = Math.Abs(numPreFabZ - numPlayerZ);
						float numDiffb = Math.Abs(numDiffXb + numDiffZb);

						if (numDiffb < numDiffa)
						{
							if ((numDiffb > minDistance) && (numDiffb < maxDistance))
							{
								prefabInstance = prefabInstance2;
								numDiffa = numDiffb;
								//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Prefab Name:" + prefabInstance.name);
							}
						}

						//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-New Prefab diffXb:" + numDiffXb + ", diffZb: " + numDiffZb);
						//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-New Prefab diff:" + numDiffb);
					}
				}

				if (prefabInstance != null)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestGotoPointSDX>().Setup(this.playerId, this.questTags, this.questCode, this.GotoType, this.difficulty, prefabInstance.boundingBoxPosition.x, prefabInstance.boundingBoxPosition.z, (float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z, -1f, BiomeFilterTypes.AnyBiome, ""), false, this.playerId, -1, -1, -1);
					return;
				}
			}
			return;
		}
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Quest quest = primaryPlayer.QuestJournal.FindActiveQuest(this.questCode);
		if (quest != null)
		{
			for (int j = 0; j < quest.Objectives.Count; j++)
			{
				if (quest.Objectives[j] is ObjectiveGoto && this.GotoType == NetPackageQuestGotoPointSDX.QuestGotoTypes.Trader)
				{
					((ObjectiveGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
				}
				else if (quest.Objectives[j] is ObjectiveClosestPOIGoto && this.GotoType == NetPackageQuestGotoPointSDX.QuestGotoTypes.Closest)
				{
					((ObjectiveClosestPOIGoto)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
				}
				else if (quest.Objectives[j] is ObjectiveRandomPOIGotoSDX && this.GotoType == NetPackageQuestGotoPointSDX.QuestGotoTypes.RandomPOI)
				{
					((ObjectiveRandomPOIGotoSDX)quest.Objectives[j]).FinalizePoint(new Vector3(this.position.x, primaryPlayer.position.y, this.position.y), this.size);
				}
			}
		}
	}

	// Token: 0x060031EA RID: 12778 RVA: 0x00159EAB File Offset: 0x001580AB
	public override int GetLength()
	{
		return 8;
	}

	// Token: 0x04002208 RID: 8712
	private int playerId;

	// Token: 0x04002209 RID: 8713
	private int questCode;

	// Token: 0x0400220A RID: 8714
	private FastTags questTags;

	// Token: 0x0400220B RID: 8715
	private Vector2 position;

	// Token: 0x0400220C RID: 8716
	private Vector3 size;

	// Token: 0x0400220D RID: 8717
	private byte difficulty;

	// Token: 0x0400220E RID: 8718
	public NetPackageQuestGotoPointSDX.QuestGotoTypes GotoType;

	// Token: 0x0400220F RID: 8719
	private BiomeFilterTypes biomeFilterType;

	// Token: 0x04002210 RID: 8720
	private string biomeFilter;

	// Token: 0x02000E83 RID: 3715
	public enum QuestGotoTypes
	{
		// Token: 0x04006704 RID: 26372
		Trader,
		// Token: 0x04006705 RID: 26373
		Closest,
		// Token: 0x04006706 RID: 26374
		RandomPOI
	}
}
