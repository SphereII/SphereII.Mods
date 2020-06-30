using System;
using System.Collections.Generic;
using UnityEngine;

public class EAIBreakBlockSDX : EAIBreakBlock
{
	public int attackDelay;
	public float damageBoostPercent;
	public List<Entity> allies = new List<Entity>();


	public override void Update()
	{
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		if (this.attackDelay > 0)
		{
			this.attackDelay--;
		}
		if (this.attackDelay <= 0)
		{
			this.AttackBlock();
		}
	}

	private WorldRayHitInfo GetHitInfo(out float damageScale)
	{
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		damageScale = moveHelper.DamageScale + this.damageBoostPercent;
		return moveHelper.HitInfo;
	}

	private void AttackBlock()
	{
		this.theEntity.SetLookPosition(Vector3.zero);
		ItemActionAttackData itemActionAttackData = this.theEntity.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
		if (itemActionAttackData == null)
		{
			return;
		}
		this.damageBoostPercent = 0f;
		Bounds bb = new Bounds(this.theEntity.position, new Vector3(1.7f, 1.5f, 1.7f));
		this.theEntity.world.GetEntitiesInBounds(typeof(EntityZombie), bb, this.allies);
		for (int i = this.allies.Count - 1; i >= 0; i--)
		{
			if ((EntityZombie)this.allies[i] != this.theEntity)
			{
				this.damageBoostPercent += 0.2f;
			}
		}
		this.allies.Clear();
		itemActionAttackData.hitDelegate = new ItemActionAttackData.HitDelegate(this.GetHitInfo);
		if (this.theEntity.Attack(false))
		{
			this.theEntity.IsBreakingBlocks = true;
			float num = 0.25f + base.RandomFloat * 0.8f;
			if (this.theEntity.moveHelper.IsUnreachableAbove)
			{
				num *= 0.5f;
			}
			this.attackDelay = (int)((num + 0.75f) * 20f);
			this.theEntity.Attack(true);
		}
	}
}
