using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200055D RID: 1373
public class ObjectiveRandomPOIGotoSDX : ObjectiveGoto
{
	// Token: 0x0600386D RID: 14445 RVA: 0x00184417 File Offset: 0x00182617
	public override void SetupObjective()
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-SetupObjective");
		this.keyword = Localization.Get("ObjectiveRallyPointHeadTo");
	}

	// Token: 0x0600386E RID: 14446 RVA: 0x00184429 File Offset: 0x00182629
	protected override void SetupIcon()
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-SetupIcon");
		this.icon = "ui_game_symbol_quest";
	}

	// Token: 0x17000682 RID: 1666
	// (get) Token: 0x0600386F RID: 14447 RVA: 0x00184436 File Offset: 0x00182636
	public override bool NeedsNPCSetPosition
	{
		get
		{
			//Log.Out("ObjectiveRandomPOIGotoSDX-NeedsNPCSetPosition");
			return true;
		}
	}

	// Token: 0x06003870 RID: 14448 RVA: 0x00184439 File Offset: 0x00182639
	public override bool SetupPosition(EntityNPC ownerNPC = null, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-SetupPosition");
		return this.GetPosition(ownerNPC, player, usedPOILocations, entityIDforQuests) != Vector3.zero;
	}

	// Token: 0x06003871 RID: 14449 RVA: 0x00184450 File Offset: 0x00182650
	public override void AddHooks()
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-AddHooks");
		base.AddHooks();
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
	}

	// Token: 0x06003872 RID: 14450 RVA: 0x0018446B File Offset: 0x0018266B
	private void SetDistanceOffset(Vector3 POISize)
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-SetDistanceOffset");
		if (POISize.x > POISize.z)
		{
			this.distanceOffset = POISize.x;
			return;
		}
		this.distanceOffset = POISize.z;
	}

	// Token: 0x06003873 RID: 14451 RVA: 0x00184494 File Offset: 0x00182694
	public override void SetPosition(Vector3 POIPosition, Vector3 POISize)
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-SetPosition");
		this.SetDistanceOffset(POISize);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POIPosition, POIPosition);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.POISize, POISize);
		base.OwnerQuest.Position = POIPosition;
		this.position = POIPosition;
	}

	// Token: 0x06003874 RID: 14452 RVA: 0x001844CC File Offset: 0x001826CC
	protected override Vector3 GetPosition(EntityNPC ownerNPC = null, EntityPlayer entityPlayer = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition");
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition))
		{
			//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-1");
			Vector3 vector;
			base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POISize);
			Vector2 vector2 = new Vector2(this.position.x + vector.x / 2f, this.position.z + vector.z / 2f);
			int num = (int)vector2.x;
			int num2 = (int)vector2.y;
			int num3 = (int)GameManager.Instance.World.GetHeightAt(vector2.x, vector2.y);
			this.position = new Vector3((float)num, (float)num3, (float)num2);
			base.OwnerQuest.Position = this.position;
			this.SetDistanceOffset(vector);
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		EntityAlive entityAlive = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-current position: x:" + entityAlive.position.x + ", z:" + entityAlive.position.z);
			PrefabInstance prefabInstance;
			prefabInstance = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 1000, 4000000, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestClass.DifficultyTier, usedPOILocations, entityIDforQuests, this.biomeFilterType, this.biomeFilter);

			float numPlayerX = entityAlive.position.x;
			float numPlayerZ = entityAlive.position.z;

			if (prefabInstance != null)
			{
				Vector2 vector3a = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);

				float numPreFabX = (float)vector3a.x;
				float numPreFabZ = (float)vector3a.y;

				float numDiffXa = Math.Abs(numPreFabX - numPlayerX);
				float numDiffZa = Math.Abs(numPreFabZ - numPlayerZ);
				float numDiffa = Math.Abs(numDiffXa + numDiffZa);

				//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Prefab Name:" + prefabInstance.name);

				//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Distance:" + this.Value);

				//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Original Prefab diffXa:" + numDiffXa + ", diffZa: " + numDiffZa);
				//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Original Prefab diff:" + numDiffa);

				int minDistance = 0;
				int maxDistance = 0;

				if (this.Value != null)
                {
					string[] words = this.Value.Split('-');
					int valueCounter = 0;

					foreach (String word in words)
					{
						//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-Split: " + word);
						valueCounter++;
						if (valueCounter == 1)
                        {
							minDistance = Int32.Parse(word);
                        }
						else
                        {
							maxDistance = Int32.Parse(word);
						}
					}
				}

				//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-minDistance: " + minDistance + ", maxDistance: " + maxDistance);

				for (int j = 0; j < 15; j++)
				{
					PrefabInstance prefabInstance2;
					prefabInstance2 = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetRandomPOINearWorldPos(new Vector2(entityAlive.position.x, entityAlive.position.z), 1000, 4000000, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestClass.DifficultyTier, usedPOILocations, entityIDforQuests, this.biomeFilterType, this.biomeFilter);

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

			if (prefabInstance == null)
			{
				return Vector3.zero;
			}
			if (prefabInstance != null)
			{
				Vector2 vector3 = new Vector2((float)prefabInstance.boundingBoxPosition.x + (float)prefabInstance.boundingBoxSize.x / 2f, (float)prefabInstance.boundingBoxPosition.z + (float)prefabInstance.boundingBoxSize.z / 2f);
				if (vector3.x == -0.1f && vector3.y == -0.1f)
				{
					Log.Error("ObjectiveRandomGoto: No POI found.");
					return Vector3.zero;
				}
				int num4 = (int)vector3.x;
				int num5 = (int)GameManager.Instance.World.GetHeightAt(vector3.x, vector3.y);
				int num6 = (int)vector3.y;
				this.position = new Vector3((float)num4, (float)num5, (float)num6);

				//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-prefab position: x:" + this.position.x + ", z:" + this.position.z);

				if (GameManager.Instance.World.IsPositionInBounds(this.position))
				{
					base.OwnerQuest.Position = this.position;
					base.FinalizePoint(new Vector3((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.y, (float)prefabInstance.boundingBoxPosition.z), new Vector3((float)prefabInstance.boundingBoxSize.x, (float)prefabInstance.boundingBoxSize.y, (float)prefabInstance.boundingBoxSize.z));
					base.OwnerQuest.QuestPrefab = prefabInstance;
					base.OwnerQuest.DataVariables.Add("POIName", base.OwnerQuest.QuestPrefab.location.Name);
					if (usedPOILocations != null)
					{
						usedPOILocations.Add(new Vector2((float)prefabInstance.boundingBoxPosition.x, (float)prefabInstance.boundingBoxPosition.z));
					}
					return this.position;
				}
			}
		}
		else
		{
			//Log.Out("ObjectiveRandomPOIGotoSDX-GetPosition-3");
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestGotoPointSDX>().Setup(entityAlive.entityId, base.OwnerQuest.QuestTags, base.OwnerQuest.QuestCode, NetPackageQuestGotoPointSDX.QuestGotoTypes.RandomPOI, base.OwnerQuest.QuestClass.DifficultyTier, 0, -1, 0f, 0f, 0f, -1f, this.biomeFilterType, this.biomeFilter), false);
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	// Token: 0x06003875 RID: 14453 RVA: 0x001848F0 File Offset: 0x00182AF0
	public override BaseObjective Clone()
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-Clone");
		ObjectiveRandomPOIGotoSDX objectiveRandomPOIGoto = new ObjectiveRandomPOIGotoSDX();
		this.CopyValues(objectiveRandomPOIGoto);
		return objectiveRandomPOIGoto;
	}

	// Token: 0x06003876 RID: 14454 RVA: 0x0018490C File Offset: 0x00182B0C
	public override string ParseBinding(string bindingName)
	{
		//Log.Out("ObjectiveRandomPOIGotoSDX-ParseBinding");
		string id = base.ID;
		string value = base.Value;
		if (bindingName != null)
		{
			if (!(bindingName == "name"))
			{
				if (!(bindingName == "distance"))
				{
					if (bindingName == "direction")
					{
						if (base.OwnerQuest.QuestGiverID != -1)
						{
							EntityNPC entityNPC = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
							if (entityNPC != null)
							{
								this.position.y = 0f;
								Vector3 position = entityNPC.position;
								position.y = 0f;
								return ValueDisplayFormatters.Direction(GameUtils.GetDirByNormal(new Vector2(this.position.x - position.x, this.position.z - position.z)), false);
							}
						}
					}
				}
				else if (base.OwnerQuest.QuestGiverID != -1)
				{
					EntityNPC entityNPC2 = GameManager.Instance.World.GetEntity(base.OwnerQuest.QuestGiverID) as EntityNPC;
					if (entityNPC2 != null)
					{
						Vector3 position2 = entityNPC2.position;
						this.currentDistance = Vector3.Distance(position2, this.position);
						return ValueDisplayFormatters.Distance(this.currentDistance);
					}
				}
			}
			else if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (base.OwnerQuest.DataVariables.ContainsKey("POIName"))
				{
					return base.OwnerQuest.DataVariables["POIName"];
				}
				if (base.OwnerQuest.QuestPrefab == null)
				{
					return "";
				}
				return base.OwnerQuest.QuestPrefab.location.Name;
			}
			else
			{
				if (!base.OwnerQuest.DataVariables.ContainsKey("POIName"))
				{
					return "";
				}
				return base.OwnerQuest.DataVariables["POIName"];
			}
		}
		return "";
	}
}
