using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIWanderCompanion : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
	}

	public override bool CanExecute()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute START");

        if (this.theEntity.Buffs.GetCustomVar("Leader") == 0f)
        {
            return false;
        }

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            return false;
        }       

        float currentOrder = this.theEntity.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 1");
            return false;
        }
        
		if (this.theEntity.sleepingOrWakingUp)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 2");
            return false;
		}
		if (this.theEntity.GetTicksNoPlayerAdjacent() >= 120)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 3");
            return false;
		}
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 4");
            return false;
		}
		int num = (int)(200f * this.executeWaitTime);

        //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 200f * this.executeWaitTime: " + 200f * this.executeWaitTime);
        
		//Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute num: " + num);
        
		if (base.GetRandom(1000) >= num)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 5");
            return false;
		}
		if (this.manager.lookTime > 0f)
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 6");
            return false;
		}
		int num2 = (int)this.manager.interestDistance;
		Vector3 vector;
		if (this.theEntity.IsAlert)
		{
			num2 *= 2;
			vector = RandomPositionGenerator.CalcAway(this.theEntity, 0, num2, num2, this.theEntity.LastTargetPos);
		}
		else
		{
			vector = RandomPositionGenerator.Calc(this.theEntity, num2, num2);
		}
		if (vector.Equals(Vector3.zero))
		{
            //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute 7");
            return false;
		}
		this.position = vector;
        //Log.Out("EAIApproachAndAttackTargetCompanion-CanExecute END");
        return true;
	}

	public override void Start()
	{
		this.time = 0f;
		this.theEntity.FindPath(this.position, this.theEntity.GetMoveSpeed(), false, this);
	}

	public override bool Continue()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAIApproachAndAttackTargetCompanion-Continue START");

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            return false;
        }

        bool result = this.theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && this.theEntity.moveHelper.BlockedTime <= 0.3f && this.time <= 30f && !this.theEntity.navigator.noPathAndNotPlanningOne();

        //Log.Out("EAIApproachAndAttackTargetCompanion-Continue result: " + result);
        
		return result;
	}

	public override void Update()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return;
        }

        //Log.Out("EAIApproachAndAttackTargetCompanion-Update START");

        this.time += 0.05f;
        //Log.Out("EAIApproachAndAttackTargetCompanion-Update this.time: " + this.time);
    }

    public override void Reset()
	{
		this.manager.lookTime = base.RandomFloat * 3f;
		this.theEntity.moveHelper.Stop();
	}

	private const float cLookTimeMax = 3f;
	private Vector3 position;
	private float time;
}
