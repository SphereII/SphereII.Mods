using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAISetAsTargetIfHurtCompanion : EAITarget
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity, 0f, false);
		this.MutexBits = 1;
	}

	public override void SetData(DictionarySave<string, string> data)
	{
		base.SetData(data);
		this.targetClasses = new List<EAISetAsTargetIfHurtCompanion.TargetClass>();
		string text;
		if (data.TryGetValue("class", out text))
		{
			string[] array = text.Split(',', StringSplitOptions.None);
			for (int i = 0; i < array.Length; i++)
			{
				EAISetAsTargetIfHurtCompanion.TargetClass item = default(EAISetAsTargetIfHurtCompanion.TargetClass);
				item.type = EntityFactory.GetEntityType(array[i]);
				this.targetClasses.Add(item);
			}
		}
	}

	public override bool CanExecute()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute START");

        EntityAlive revengeTarget = this.theEntity.GetRevengeTarget();
		EntityAlive attackTarget = this.theEntity.GetAttackTarget();

		if (revengeTarget == null)
		{
            //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute revengeTarget IS NULL");
        }
        else
		{
            //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute revengeTarget: " + revengeTarget.EntityClass.entityClassName);
        }
        if (attackTarget == null)
        {
            //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute attackTarget IS NULL");
        }
        else
        {
            //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute attackTarget: " + attackTarget.EntityClass.entityClassName);
        }

        bool bIsValidTarget = true;

        if (revengeTarget)
        {
			//Log.Out("EAISetAsTargetIfHurtRebirth-CanExecute revengeTarget.EntityName: " + revengeTarget.EntityName);
			//Log.Out("EAISetAsTargetIfHurtRebirth-CanExecute revengeTarget.entityType: " + revengeTarget.entityType);
			//Log.Out("EAISetAsTargetIfHurtRebirth-CanExecute this.theEntity.entityType: " + this.theEntity.entityType);

			bIsValidTarget = !EntityTargetingUtilities.IsAlly(this.theEntity, revengeTarget);
            //Log.Out("EAISetAsTargetIfHurtRebirth-CanExecute bIsValidTarget: " + bIsValidTarget);
        }

        if (revengeTarget && revengeTarget != attackTarget && bIsValidTarget)
		{
            //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute 1");
            if (this.targetClasses != null)
			{
                //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute 2");
                bool flag = false;
				Type type = revengeTarget.GetType();
				for (int i = 0; i < this.targetClasses.Count; i++)
				{
					EAISetAsTargetIfHurtCompanion.TargetClass targetClass = this.targetClasses[i];
					if (targetClass.type != null && targetClass.type.IsAssignableFrom(type))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
                    //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute 3");
                    return false;
				}
			}
			if (attackTarget != null && attackTarget.IsAlive() && base.RandomFloat < 0.66f)
			{
                //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute 4");
                this.theEntity.SetRevengeTarget(null);
				return false;
			}
			if (base.check(revengeTarget))
			{
                //Log.Out("EAISetAsTargetIfHurtCompanion-CanExecute 5");
                return true;
			}
			Vector3 vector = this.theEntity.position - revengeTarget.position;
			float num = EntityClass.list[this.theEntity.entityClass].SearchArea * 0.5f;
			vector = revengeTarget.position + vector.normalized * (num * 0.5f);
			Vector3 vector2 = this.manager.random.RandomOnUnitSphere * num;
			vector.x += vector2.x;
			vector.z += vector2.z;
			Vector3i vector3i = World.worldToBlockPos(vector);
			int height = (int)this.theEntity.world.GetHeight(vector3i.x, vector3i.z);
			if (height > 0)
			{
				vector.y = (float)height;
			}
			int num2 = this.theEntity.CalcInvestigateTicks(1200, revengeTarget);
			this.theEntity.SetInvestigatePosition(vector, num2, true);
			if (this.theEntity.entityType == EntityType.Zombie)
			{
				num2 /= 6;
			}
			this.theEntity.SetAlertTicks(num2);
			this.theEntity.SetRevengeTarget(null);
		}
		return false;
	}

	public override void Start()
	{
		this.theEntity.SetAttackTarget(this.theEntity.GetRevengeTarget(), 400);
		this.viewAngleSave = this.theEntity.GetMaxViewAngle();
		this.theEntity.SetMaxViewAngle(270f);
		this.viewAngleRestoreCounter = 100;
		base.Start();
	}

	public override void Update()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return;
        }

        //Log.Out("EAISetAsTargetIfHurtCompanion-Update START");

        if (this.viewAngleRestoreCounter > 0)
		{
			this.viewAngleRestoreCounter--;
			if (this.viewAngleRestoreCounter == 0)
			{
				this.restoreViewAngle();
			}
		}
	}

	public override bool Continue()
	{
        if (this.theEntity.Buffs.GetCustomVar("onMission") == 1f)
        {
            return false;
        }

        //Log.Out("EAISetAsTargetIfHurtCompanion-Continue START");

        bool revengeTargetIsNull = (this.theEntity.GetRevengeTarget() != null);
        bool revengeAndAttackTargetDifferent = (this.theEntity.GetAttackTarget() != this.theEntity.GetRevengeTarget());
		bool baseContinue = base.Continue();

        //Log.Out("EAISetAsTargetIfHurtCompanion-Continue revengeTargetIsNull: " + revengeTargetIsNull);
        //Log.Out("EAISetAsTargetIfHurtCompanion-Continue revengeAndAttackTargetDifferent: " + revengeAndAttackTargetDifferent);
        //Log.Out("EAISetAsTargetIfHurtCompanion-Continue baseContinue: " + baseContinue);

        return (!revengeTargetIsNull || !revengeAndAttackTargetDifferent) && baseContinue;
	}

	public override void Reset()
	{
		base.Reset();
		this.restoreViewAngle();
	}

	private void restoreViewAngle()
	{
		if (this.viewAngleSave > 0f)
		{
			this.theEntity.SetMaxViewAngle(this.viewAngleSave);
			this.viewAngleSave = 0f;
			this.viewAngleRestoreCounter = 0;
		}
	}

	private List<EAISetAsTargetIfHurtCompanion.TargetClass> targetClasses;
	private float viewAngleSave;
	private int viewAngleRestoreCounter;
	private struct TargetClass
	{
		public Type type;
	}
}
