using System;
using System.Collections.Generic;
using System.Globalization;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIApproachAndAttackTargetCompanion : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 3;
		this.executeDelay = 0.1f;
    }

    public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		this.targetClasses = new List<EAIApproachAndAttackTargetCompanion.TargetClass>();
		string text;
		if (data.TryGetValue("class", out text))
		{
			string[] array = text.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i += 2)
			{
				EAIApproachAndAttackTargetCompanion.TargetClass targetClass = default(EAIApproachAndAttackTargetCompanion.TargetClass);
				targetClass.type = EntityFactory.GetEntityType(array[i]);
				targetClass.chaseTimeMax = 0f;
				if (i + 1 < array.Length)
				{
					targetClass.chaseTimeMax = StringParsers.ParseFloat(array[i + 1], 0, -1, NumberStyles.Any);
				}
				this.targetClasses.Add(targetClass);
				if (targetClass.type == typeof(EntityEnemyAnimal))
				{
					targetClass.type = typeof(EntityAnimalSnake);
					this.targetClasses.Add(targetClass);
				}
			}
		}
	}

	public override bool CanExecute()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute START");

        float currentOrder = this.theEntity.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            return false;
        }

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute STAND STILL");
            this.theEntity.SetRevengeTarget(null);
            this.theEntity.SetAttackTarget(null, 0);

            return false;
        }

        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None || (this.theEntity.Jumping && !this.theEntity.isSwimming))
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 1");
            return false;
		}
		
		this.entityTarget = this.theEntity.GetAttackTarget();

        if (this.entityTarget != null)
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute this.entityTarget: " + this.entityTarget.EntityClass.entityClassName);
        }
        else
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute NO ATTACK TARGET");
        }

        bool stopAttacking = this.theEntity.Buffs.HasBuff("buffNPCModStopAttacking") ||
                                this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill");

        if (this.entityTarget == null || stopAttacking)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 2");
            this.OwnerID = (int)this.theEntity.Buffs.GetCustomVar("Leader");

            if (this.OwnerID > 0)
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute this.OwnerID: " + this.OwnerID);

                EntityPlayer entityPlayer = (EntityPlayer)this.theEntity.world.GetEntity(this.OwnerID);
                if (entityPlayer != null)
				{
                    entityTarget = entityPlayer;
                    //this.theEntity.SetAttackTarget(entityTarget, 1);

                    float distance = this.theEntity.GetDistance(entityTarget);

                    if (distance > 35)
                    {
                        EntityAliveV2 npc = (EntityAliveV2)this.theEntity;

                        if (npc != null)
                        {
                            npc.TeleportToPlayer(entityTarget, false);
                        }
                    }

                    //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute FOUND OWNER");
                    return true;
                }
            }

            return false;
        }

        Type type = this.entityTarget.GetType();
		for (int i = 0; i < this.targetClasses.Count; i++)
		{
			EAIApproachAndAttackTargetCompanion.TargetClass targetClass = this.targetClasses[i];
			if (targetClass.type != null && targetClass.type.IsAssignableFrom(type))
			{
				this.chaseTimeMax = targetClass.chaseTimeMax;
                //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 5");
                return true;
			}
		}
        //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 6");
        return false;
	}

	public override void Start()
	{
		this.entityTargetPos = this.entityTarget.position;
		this.entityTargetVel = Vector3.zero;
		this.isTargetToEat = false;
		this.isEating = false;
		this.theEntity.IsEating = false;
		this.homeTimeout = (this.theEntity.IsSleeper ? 90f : this.chaseTimeMax);
		this.hasHome = (this.homeTimeout > 0f);
		this.isGoingHome = false;
		if (this.theEntity.ChaseReturnLocation == Vector3.zero)
		{
			this.theEntity.ChaseReturnLocation = (this.theEntity.IsSleeper ? this.theEntity.SleeperSpawnPosition : this.theEntity.position);
		}
		this.pathCounter = 0;
		this.relocateTicks = 0;
		this.attackTimeout = 5;
	}

	public override bool Continue()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAIApproachAndAttackTargetCompanion-Continue START");

        float currentOrder = this.theEntity.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            return false;
        }

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            this.theEntity.SetRevengeTarget(null);
            this.theEntity.SetAttackTarget(null, 0);

            return false;
        }

        if (this.theEntity.sleepingOrWakingUp || this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Continue 1, false");
            return false;
		}

        bool result = false;

        EntityAlive attackTarget = this.theEntity.GetAttackTarget();

        if (attackTarget != null)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Continue 1, attackTarget: " + attackTarget.EntityClass.entityClassName);
        }
        else
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Continue NO ATTACK TARGET");
        }

        bool stopAttacking = this.theEntity.Buffs.HasBuff("buffNPCModStopAttacking") ||
                                this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill");

        if (stopAttacking)
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-Continue 2");
            this.OwnerID = (int)this.theEntity.Buffs.GetCustomVar("Leader");

            if (this.OwnerID > 0)
            {
                //Log.Out("EAIApproachAndAttackTargetCompanion-Continue this.OwnerID: " + this.OwnerID);

                EntityPlayer entityPlayer = (EntityPlayer)this.theEntity.world.GetEntity(OwnerID);
                if (entityPlayer != null)
				{
                    attackTarget = entityPlayer;
                    this.entityTarget = entityPlayer;
					this.theEntity.SetAttackTarget(null, 0);
                    //Log.Out("EAIApproachAndAttackTargetCompanion-Continue FOUND OWNER");
                    return false;
                }
            }
        }

        if (this.isGoingHome)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Continue 3, " + result);
            result = !attackTarget && this.theEntity.ChaseReturnLocation != Vector3.zero;
            return result;
		}

		result = attackTarget && !(attackTarget != this.entityTarget);

        //Log.Out("EAIApproachAndAttackTargetCompanion-Continue 4, " + result);
        return result;
	}

	public override void Reset()
	{
		this.theEntity.IsEating = false;
		this.theEntity.moveHelper.Stop();
		if (this.blockTargetTask != null)
		{
			this.blockTargetTask.canExecute = false;
		}
	}

	public override void Update()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return;
        }

        //Log.Out("EAIApproachAndAttackTargetCompanion-Update START");

        if (this.entityTarget == null)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 1");
            return;
		}

        float currentOrder = this.theEntity.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            return;
        }

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            this.theEntity.SetRevengeTarget(null);
            this.theEntity.SetAttackTarget(null, 0);
            return;
        }

        if (this.relocateTicks > 0)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 2");
            if (!this.theEntity.navigator.noPathAndNotPlanningOne())
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 3");
                this.relocateTicks--;
				this.theEntity.moveHelper.SetFocusPos(this.entityTarget.position);
                return;
			}
			this.relocateTicks = 0;
		}
		
		Vector3 vector2 = this.entityTarget.position;

        theEntity.SetLookPosition(vector2);
        theEntity.RotateTo(vector2.x, vector2.y + 2, vector2.z, 8f, 8f);

        Vector3 a = theEntity.position - vector2;

        //Log.Out("EAIApproachAndAttackTargetCompanion-Update this.entityTarget.position: " + this.entityTarget.position);
        //Log.Out("EAIApproachAndAttackTargetCompanion-Update entityTargetPosn: " + vector2);
        //Log.Out("EAIApproachAndAttackTargetCompanion-Update a: " + a);
        //Log.Out("EAIApproachAndAttackTargetCompanion-Update a.sqrMagnitude: " + a.sqrMagnitude);

        //Log.Out("EAIApproachAndAttackTargetCompanion-Update B entityClassName: " + this.entityTarget.EntityClass.entityClassName);

        /*if (this.theEntity.GetAttackTarget() != null)
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update GetAttackTarget: " + this.theEntity.GetAttackTarget().EntityClass.entityClassName);
        }
        else
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update NO ATTACK TARGET");
        }*/


        //Log.Out("EAIApproachAndAttackTargetCompanion-Update this.entityTarget.entityId: " + this.entityTarget.entityId);
        //Log.Out("EAIApproachAndAttackTargetCompanion-Update this.OwnerID: " + this.theEntity.Buffs.GetCustomVar("Leader"));

        this.OwnerID = (int)this.theEntity.Buffs.GetCustomVar("Leader");

		float magnitude = 4f;

		bool isClient = SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient;

		if (isClient)
		{
            magnitude = 3f;
        }

		if (this.entityTarget.entityId == this.OwnerID && a.sqrMagnitude < magnitude)
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update Entity is too close. Ending pathing.");
            pathCounter = 0;
            return;
        }

        if (a.sqrMagnitude < 1f)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 6");
            this.entityTargetVel = this.entityTargetVel * 0.7f + a * 0.3f;
		}
		this.entityTargetPos = vector2;
		this.attackTimeout--;

        this.theEntity.moveHelper.CalcIfUnreachablePos();
		float num2;
		float num3;

		ItemValue holdingItemItemValue = this.theEntity.inventory.holdingItemItemValue;
		int holdingItemIdx = this.theEntity.inventory.holdingItemIdx;
		ItemAction itemAction = holdingItemItemValue.ItemClass.Actions[holdingItemIdx];
		num2 = 1.095f;
		if (itemAction != null)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 7");
            num2 = itemAction.Range;
			if (num2 == 0f)
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 8");
                num2 = EffectManager.GetItemValue(PassiveEffects.MaxRange, holdingItemItemValue, 0f);
			}
		}
		num3 = Utils.FastMax(0.7f, num2 - 0.35f);

		float num4 = num3 * num3;
		float num5 = 4f;
		if (this.theEntity.IsFeral)
		{
			num5 = 8f;
		}
		num5 = base.RandomFloat * num5;
		float targetXZDistanceSq = this.GetTargetXZDistanceSq(num5);
		float num6 = vector2.y - this.theEntity.position.y;
		float num7 = Utils.FastAbs(num6);
		bool flag = targetXZDistanceSq <= num4 && num7 < 1f;
		if (!flag)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 9");
            if (num7 < 3f && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 10");
                PathEntity path = this.theEntity.navigator.getPath();
				if (path != null && path.NodeCountRemaining() <= 2)
				{
                    //Log.Out("EAIApproachAndAttackTargetCompanion-Update 11");
                    this.pathCounter = 0;
				}
			}
			int num = this.pathCounter - 1;
			this.pathCounter = num;
			if (num <= 0 && this.theEntity.CanNavigatePath() && !PathFinderThread.Instance.IsCalculatingPath(this.theEntity.entityId))
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 12");
                this.pathCounter = 6 + base.GetRandom(10);
				Vector3 moveToLocation = this.GetMoveToLocation(num3);
				if (moveToLocation.y - this.theEntity.position.y < -8f)
				{
                    //Log.Out("EAIApproachAndAttackTargetCompanion-Update 13");
                    this.pathCounter += 40;
					if (base.RandomFloat < 0.2f)
					{
                        //Log.Out("EAIApproachAndAttackTargetCompanion-Update 14");
                        this.seekPosOffset.x = this.seekPosOffset.x + (base.RandomFloat * 0.6f - 0.3f);
						this.seekPosOffset.y = this.seekPosOffset.y + (base.RandomFloat * 0.6f - 0.3f);
					}
					moveToLocation.x += this.seekPosOffset.x;
					moveToLocation.z += this.seekPosOffset.y;
				}
				else
				{
                    //Log.Out("EAIApproachAndAttackTargetCompanion-Update 15");
                    float num8 = (moveToLocation - this.theEntity.position).magnitude - 5f;
					if (num8 > 0f)
					{
                        //Log.Out("EAIApproachAndAttackTargetCompanion-Update 16");
                        if (num8 > 60f)
						{
                            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 17");
                            num8 = 60f;
						}
						this.pathCounter += (int)(num8 / 20f * 20f);
					}
				}
				this.theEntity.FindPath(moveToLocation, this.theEntity.GetMoveSpeedAggro(), true, this);
			}
		}
		if (this.theEntity.Climbing)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 18");
            return;
		}
		bool flag2 = this.theEntity.CanSee(this.entityTarget);

		if (this.OwnerID > 0 && !(this.entityTarget is EntityPlayer) && this.entityTarget.Buffs.GetCustomVar("$FuriousRamsayAttacking") == 1f || this.entityTarget.Buffs.GetCustomVar("$FuriousRamsayAttacked") == 1f)
		{
            flag2 = true;
        }

        //this.theEntity.SetLookPosition((flag2 && !this.theEntity.IsBreakingBlocks) ? this.entityTarget.getHeadPosition() : Vector3.zero);
        if (!flag)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 20");
            if (this.theEntity.navigator.noPathAndNotPlanningOne() && this.pathCounter > 0 && num6 < 2.1f)
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 21");
                Vector3 moveToLocation2 = this.GetMoveToLocation(num3);
				this.theEntity.moveHelper.SetMoveTo(moveToLocation2, true);
			}
		}
		else
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 22");
            this.theEntity.moveHelper.Stop();
			this.pathCounter = 0;
		}
        float num9 = num2 - 0.1f;
        float num10 = num9 * num9;
		if (targetXZDistanceSq > num10 || num7 >= 1.25f)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 23");
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update C entityClassName: " + this.entityTarget.EntityClass.entityClassName);
            return;
		}
		this.theEntity.IsBreakingBlocks = false;
		this.theEntity.IsBreakingDoors = false;
		if (this.theEntity.bodyDamage.HasLimbs && !this.theEntity.Electrocuted)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 24");
            this.theEntity.RotateTo(vector2.x, vector2.y, vector2.z, 30f, 30f);
		}
		if (this.theEntity.GetDamagedTarget() == this.entityTarget || (this.entityTarget != null && this.entityTarget.GetDamagedTarget() == this.theEntity))
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 25");
            this.homeTimeout = (this.theEntity.IsSleeper ? 90f : this.chaseTimeMax);
			if (this.blockTargetTask != null)
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 26");
                this.blockTargetTask.canExecute = false;
			}
			this.theEntity.ClearDamagedTarget();
			if (this.entityTarget)
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 27");
                this.entityTarget.ClearDamagedTarget();
			}
		}
		if (this.attackTimeout > 0)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 28");
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update D entityClassName: " + this.entityTarget.EntityClass.entityClassName);
            return;
		}
		if (this.manager.groupCircle > 0f)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 29");
            Entity targetIfAttackedNow = this.theEntity.GetTargetIfAttackedNow();
			if (targetIfAttackedNow != this.entityTarget && (!this.entityTarget.AttachedToEntity || this.entityTarget.AttachedToEntity != targetIfAttackedNow))
			{
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 30");
                if (targetIfAttackedNow != null)
				{
                    //Log.Out("EAIApproachAndAttackTargetCompanion-Update 31");
                    this.relocateTicks = 46;
					Vector3 vector3 = (this.theEntity.position - vector2).normalized * (num9 + 1.1f);
					float num11 = base.RandomFloat * 28f + 18f;
					if (base.RandomFloat < 0.5f)
					{
                        //Log.Out("EAIApproachAndAttackTargetCompanion-Update 32");
                        num11 = -num11;
					}
					vector3 = Quaternion.Euler(0f, num11, 0f) * vector3;
					Vector3 targetPos = vector2 + vector3;
					this.theEntity.FindPath(targetPos, this.theEntity.GetMoveSpeedAggro(), true, this);
				}
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update E entityClassName: " + this.entityTarget.EntityClass.entityClassName);
                //Log.Out("EAIApproachAndAttackTargetCompanion-Update 33");
                return;
			}
		}
		this.theEntity.SleeperSupressLivingSounds = false;

        //Log.Out("EAIApproachAndAttackTargetCompanion-Update 34");
        if (!(this.entityTarget is EntityPlayer) && this.theEntity.Attack(false))
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-Update 35");
            this.attackTimeout = this.theEntity.GetAttackTimeoutTicks();
			this.theEntity.Attack(true);
		}
	}

	private float GetTargetXZDistanceSq(float estimatedTicks)
	{
		Vector3 vector = this.entityTarget.position;
		vector += this.entityTargetVel * estimatedTicks;
		Vector3 vector2 = this.theEntity.position + this.theEntity.motion * estimatedTicks - vector;
		vector2.y = 0f;
		return vector2.sqrMagnitude;
	}

	private Vector3 GetMoveToLocation(float maxDist)
	{
		Vector3 vector = this.entityTarget.position;
		vector += this.entityTargetVel * 6f;
		vector = this.entityTarget.world.FindSupportingBlockPos(vector);
		if (maxDist > 0f)
		{
			Vector3 vector2 = new Vector3(this.theEntity.position.x, vector.y, this.theEntity.position.z);
			Vector3 vector3 = vector - vector2;
			float magnitude = vector3.magnitude;
			if (magnitude < 3f)
			{
				if (magnitude <= maxDist)
				{
					float num = vector.y - this.theEntity.position.y;
					if (num < -3f || num > 1.5f)
					{
						return vector;
					}
					return vector2;
				}
				else
				{
					vector3 *= maxDist / magnitude;
					Vector3 vector4 = vector - vector3;
					vector4.y += 0.51f;
					Vector3i pos = World.worldToBlockPos(vector4);
					BlockValue block = this.entityTarget.world.GetBlock(pos);
					Block block2 = block.Block;
					if (block2.PathType <= 0)
					{
						RaycastHit raycastHit;
						if (Physics.Raycast(vector4 - Origin.position, Vector3.down, out raycastHit, 1.02f, 1082195968))
						{
							vector4.y = raycastHit.point.y + Origin.position.y;
							return vector4;
						}
						if (block2.IsElevator((int)block.rotation))
						{
							vector4.y = vector.y;
							return vector4;
						}
					}
				}
			}
		}
		return vector;
	}

	public override string ToString()
	{
		ItemValue holdingItemItemValue = this.theEntity.inventory.holdingItemItemValue;
		int holdingItemIdx = this.theEntity.inventory.holdingItemIdx;
		ItemAction itemAction = holdingItemItemValue.ItemClass.Actions[holdingItemIdx];
		float num = 1.095f;
        if (itemAction != null)
        {
			num = itemAction.Range;
			if (num == 0f)
			{
				num = EffectManager.GetItemValue(PassiveEffects.MaxRange, holdingItemItemValue, 0f);
			}
		}
        float value = num - 0.1f;
        float targetXZDistanceSq = this.GetTargetXZDistanceSq(0f);
		return string.Format("{0}, {1}{2}{3}{4}{5} dist {6} rng {7} timeout {8}", new object[]
		{
			base.ToString(),
			this.entityTarget ? this.entityTarget.EntityName : "",
			this.theEntity.CanSee(this.entityTarget) ? "(see)" : "",
			this.theEntity.navigator.noPathAndNotPlanningOne() ? "(-path)" : (this.theEntity.navigator.noPath() ? "(!path)" : ""),
			this.isTargetToEat ? "(eat)" : "",
			this.isGoingHome ? "(home)" : "",
			Mathf.Sqrt(targetXZDistanceSq).ToCultureInvariantString("0.000"),
			value.ToCultureInvariantString("0.000"),
			this.homeTimeout.ToCultureInvariantString("0.00")
		});
	}

	private const float cSleeperChaseTime = 90f;
	private List<EAIApproachAndAttackTargetCompanion.TargetClass> targetClasses;
	private float chaseTimeMax;
	private bool hasHome;
	private bool isGoingHome;
	private float homeTimeout;
	private EntityAlive entityTarget;
	private Vector3 entityTargetPos;
	private Vector3 entityTargetVel;
	private int attackTimeout;
	private int pathCounter;
	private Vector2 seekPosOffset;
	private bool isTargetToEat;
	private bool isEating;
	private int eatCount;
	private EAIBlockingTargetTask blockTargetTask;
	private int relocateTicks;
	private int OwnerID = 0;

	private struct TargetClass
	{
		public Type type;
		public float chaseTimeMax;
	}
}
