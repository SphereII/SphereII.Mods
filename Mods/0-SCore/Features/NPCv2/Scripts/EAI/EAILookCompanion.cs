using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAILookCompanion : EAIBase
{
	public EAILookCompanion()
	{
		this.MutexBits = 1;
	}

	public override bool CanExecute()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAILookCompanion-CanExecute START");

        if (this.theEntity.Buffs.GetCustomVar("Leader") == 0f)
		{
			return false;
		}

        if (this.theEntity.Buffs.HasBuff("FuriousRamsayStandStill"))
        {
            return false;
        }

        return this.manager.lookTime > 0f;
	}

	public override void Start()
	{
		this.waitTicks = (int)(this.manager.lookTime * 20f);
		this.manager.lookTime = 0f;
		this.theEntity.GetEntitySenses().Clear();
		this.viewTicks = 0;
		this.theEntity.Jumping = false;
		this.theEntity.moveHelper.Stop();
	}

	public override bool Continue()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAILookCompanion-Continue START");

        if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		this.waitTicks--;
		if (this.waitTicks <= 0)
		{
			return false;
		}
		this.viewTicks--;
		if (this.viewTicks <= 0)
		{
			this.viewTicks = 40;
			Vector3 headPosition = this.theEntity.getHeadPosition();
			Vector3 vector = this.theEntity.GetForwardVector();
			vector = Quaternion.Euler(base.RandomFloat * 60f - 30f, base.RandomFloat * 120f - 60f, 0f) * vector;
			this.theEntity.SetLookPosition(headPosition + vector);
		}
		return true;
	}

	public override void Reset()
	{
		this.theEntity.SetLookPosition(Vector3.zero);
	}

	public override string ToString()
	{
		return string.Format("{0}, wait {1}", base.ToString(), ((float)this.waitTicks / 20f).ToCultureInvariantString());
	}

	private int waitTicks;
	private int viewTicks;
}
