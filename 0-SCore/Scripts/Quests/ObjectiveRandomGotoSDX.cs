using System;
using System.Globalization;
using UnityEngine;

// Token: 0x0200055B RID: 1371
public class ObjectiveRandomGotoSDX : BaseObjective
{
	// Token: 0x1700067E RID: 1662
	// (get) Token: 0x06003856 RID: 14422 RVA: 0x001839B5 File Offset: 0x00181BB5
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Distance;
		}
	}

	// Token: 0x06003857 RID: 14423 RVA: 0x001839B8 File Offset: 0x00181BB8
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveRallyPointHeadTo");
		this.SetupIcon();
	}

	// Token: 0x1700067F RID: 1663
	// (get) Token: 0x06003858 RID: 14424 RVA: 0x001839D0 File Offset: 0x00181BD0
	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	// Token: 0x06003859 RID: 14425 RVA: 0x001839DE File Offset: 0x00181BDE
	public override void SetupDisplay()
	{
		base.Description = this.keyword;
		this.StatusText = "";
	}

	// Token: 0x17000680 RID: 1664
	// (get) Token: 0x0600385A RID: 14426 RVA: 0x001839F8 File Offset: 0x00181BF8
	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				return ValueDisplayFormatters.Distance(this.distance);
			}
			if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
			{
				return "";
			}
			if (base.ObjectiveState == BaseObjective.ObjectiveStates.Failed)
			{
				return Localization.Get("failed");
			}
			return Localization.Get("completed");
		}
	}

	// Token: 0x0600385B RID: 14427 RVA: 0x00183A50 File Offset: 0x00181C50
	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
	}

	// Token: 0x0600385C RID: 14428 RVA: 0x00183A70 File Offset: 0x00181C70
	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
	}

	// Token: 0x0600385D RID: 14429 RVA: 0x00183A7D File Offset: 0x00181C7D
	protected virtual void SetupIcon()
	{
	}

	// Token: 0x0600385E RID: 14430 RVA: 0x00183A80 File Offset: 0x00181C80
	protected virtual Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.Location))
		{
			base.OwnerQuest.Position = this.position;
			this.positionSet = true;
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.TreasurePoint))
		{
			this.positionSet = true;
			base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, base.OwnerQuest.Position);
			base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
			base.CurrentValue = 2;
			return this.position;
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		float num = 50f;
		if (base.Value != null && base.Value != "" && !StringParsers.TryParseFloat(base.Value, out num, 0, -1, NumberStyles.Any) && base.Value.Contains("-"))
		{
			string[] array = base.Value.Split(new char[]
			{
				'-'
			});
			float num2 = StringParsers.ParseFloat(array[0], 0, -1, NumberStyles.Any);
			float num3 = StringParsers.ParseFloat(array[1], 0, -1, NumberStyles.Any);
			num = GameManager.Instance.World.GetGameRandom().RandomFloat * (num3 - num2) + num2;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Vector3i vector3i = ObjectiveRandomGotoSDX.CalculateRandomPoint(ownerPlayer.entityId, num, base.OwnerQuest.ID);
			if (!GameManager.Instance.World.CheckForLevelNearbyHeights((float)vector3i.x, (float)vector3i.z, 5) || GameManager.Instance.World.GetWaterAt((float)vector3i.x, (float)vector3i.z))
			{
				return Vector3.zero;
			}
			World world = GameManager.Instance.World;
			if (vector3i.y > 0 && world.IsPositionInBounds(this.position) && !world.IsPositionWithinPOI(this.position, 5))
			{
				this.FinalizePoint(vector3i.x, vector3i.y, vector3i.z);
				return this.position;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePointSDX>().Setup(ownerPlayer.entityId, num, 1, base.OwnerQuest.QuestCode, 0, -1, 0, false), false);
			base.CurrentValue = 1;
		}
		return Vector3.zero;
	}

	// Token: 0x0600385F RID: 14431 RVA: 0x00183CDC File Offset: 0x00181EDC
	public static Vector3i CalculateRandomPoint(int entityID, float distance, string questID)
	{
		World world = GameManager.Instance.World;
		EntityAlive entityAlive = world.GetEntity(entityID) as EntityAlive;
		var vector = new Vector3(world.GetGameRandom().RandomFloat * 2f + -1f, 0f, world.GetGameRandom().RandomFloat * 2f + -1f);
		vector.Normalize();
		Vector3 vector2 = entityAlive.position + vector * distance;
		int x = (int)vector2.x;
		int z = (int)vector2.z;
		int y = (int)world.GetHeightAt(vector2.x, vector2.z);
		Vector3i vector3i = new Vector3i(x, y, z);
		Vector3 vector3  = new Vector3((float)vector3i.x, (float)vector3i.y, (float)vector3i.z);
		if (!world.IsPositionInBounds(vector3) || (entityAlive is EntityPlayer && !world.CanPlaceBlockAt(vector3i, GameManager.Instance.GetPersistentLocalPlayer(), false)) || world.IsPositionWithinPOI(vector3, 2))
		{
			return new Vector3i(0, -99999, 0);
		}
		if (!world.CheckForLevelNearbyHeights(vector2.x, vector2.z, 5) || world.GetWaterAt(vector2.x, vector2.z))
		{
			return new Vector3i(0, -99999, 0);
		}
		return vector3i;
	}

	// Token: 0x06003860 RID: 14432 RVA: 0x00183E24 File Offset: 0x00182024
	public void FinalizePoint(int x, int y, int z)
	{
		this.position = new Vector3((float)x, (float)y, (float)z);
		base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Location, this.position);
		base.OwnerQuest.Position = this.position;
		this.positionSet = true;
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Location, this.NavObjectName, -1);
		base.CurrentValue = 2;
	}

	// Token: 0x06003861 RID: 14433 RVA: 0x00183E88 File Offset: 0x00182088
	public override void Update(float deltaTime)
	{
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			if (!this.positionSet && base.CurrentValue != 1)
			{
				bool position = this.GetPosition() != Vector3.zero;
			}
			switch (base.CurrentValue)
			{
			case 0:
					bool flag = this.GetPosition() != Vector3.zero;
				return;
			case 1:
					break;
			case 2:
			{
				Entity ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
				if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != this.position)
				{
					base.OwnerQuest.NavObject.TrackedPosition = this.position;
				}
				Vector3 vector = ownerPlayer.position;
				this.distance = Vector3.Distance(vector, this.position);
				if (this.distance < this.completionDistance && base.OwnerQuest.CheckRequirements())
				{
					base.CurrentValue = 3;
					this.Refresh();
					return;
				}
				break;
			}
			case 3:
			{
				if (this.completeWithinRange)
				{
					QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
					return;
				}
				Entity ownerPlayer2 = base.OwnerQuest.OwnerJournal.OwnerPlayer;
				if (base.OwnerQuest.NavObject != null && base.OwnerQuest.NavObject.TrackedPosition != this.position)
				{
					base.OwnerQuest.NavObject.TrackedPosition = this.position;
				}
				Vector3 vector2 = ownerPlayer2.position;
				this.distance = Vector3.Distance(vector2, this.position);
				if (this.distance > this.completionDistance)
				{
					base.CurrentValue = 2;
					this.Refresh();
				}
				break;
			}
			default:
					return;
			}
		}
	}

	// Token: 0x06003862 RID: 14434 RVA: 0x00184038 File Offset: 0x00182238
	public override void Refresh()
	{
		bool complete = base.CurrentValue == 3;
		base.Complete = complete;
		if (base.Complete)
		{
			base.OwnerQuest.CheckForCompletion(QuestClass.CompletionTypes.AutoComplete, null, this.PlayObjectiveComplete);
		}
	}

	// Token: 0x06003863 RID: 14435 RVA: 0x00184074 File Offset: 0x00182274
	public override BaseObjective Clone()
	{
		ObjectiveRandomGotoSDX objectiveRandomGoto = new ObjectiveRandomGotoSDX();
		this.CopyValues(objectiveRandomGoto);
		objectiveRandomGoto.position = this.position;
		objectiveRandomGoto.positionSet = this.positionSet;
		objectiveRandomGoto.completionDistance = this.completionDistance;
		return objectiveRandomGoto;
	}

	// Token: 0x06003864 RID: 14436 RVA: 0x001840B3 File Offset: 0x001822B3
	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		this.FinalizePoint((int)pos.x, (int)pos.y, (int)pos.z);
		return true;
	}

	// Token: 0x06003865 RID: 14437 RVA: 0x001840D4 File Offset: 0x001822D4
	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveRandomGotoSDX.PropDistance))
		{
			base.Value = properties.Values[ObjectiveRandomGotoSDX.PropDistance];
		}
		if (properties.Values.ContainsKey(ObjectiveRandomGotoSDX.PropCompletionDistance))
		{
			this.completionDistance = StringParsers.ParseFloat(properties.Values[ObjectiveRandomGotoSDX.PropCompletionDistance], 0, -1, NumberStyles.Any);
		}
	}

	// Token: 0x04002577 RID: 9591
	protected bool positionSet;

	// Token: 0x04002578 RID: 9592
	protected float distance;

	// Token: 0x04002579 RID: 9593
	protected float completionDistance = 10f;

	// Token: 0x0400257A RID: 9594
	protected Vector3 position;

	// Token: 0x0400257B RID: 9595
	protected string icon = "ui_game_symbol_quest";

	// Token: 0x0400257C RID: 9596
	private float updateTime;

	// Token: 0x0400257D RID: 9597
	protected bool completeWithinRange = true;

	// Token: 0x0400257E RID: 9598
	public static string PropDistance = "distance";

	// Token: 0x0400257F RID: 9599
	public static string PropCompletionDistance = "completion_distance";

	// Token: 0x02000EE6 RID: 3814
	protected enum GotoStates
	{
		// Token: 0x0400692F RID: 26927
		NoPosition,
		// Token: 0x04006930 RID: 26928
		WaitingForPoint,
		// Token: 0x04006931 RID: 26929
		TryComplete,
		// Token: 0x04006932 RID: 26930
		Completed
	}
}
