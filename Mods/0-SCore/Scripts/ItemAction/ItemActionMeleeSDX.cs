using UnityEngine;



class ItemActionMeleeSDX : ItemActionMelee
{
	// This is a copy of the GetExecuteActionTarget() minus the EntityEnemey check on the line 32 ( holdingEntity.IsBreakingBlocks
	public override WorldRayHitInfo GetExecuteActionTarget(ItemActionData _actionData)
	{
		ItemActionMelee.InventoryDataMelee inventoryDataMelee = (ItemActionMelee.InventoryDataMelee)_actionData;
		EntityAlive holdingEntity = inventoryDataMelee.invData.holdingEntity;
		inventoryDataMelee.ray = holdingEntity.GetLookRay();
		if (holdingEntity.IsBreakingBlocks)
		{
			if (inventoryDataMelee.ray.direction.y < 0f)
			{
				inventoryDataMelee.ray.direction = new Vector3(inventoryDataMelee.ray.direction.x, 0f, inventoryDataMelee.ray.direction.z);
				ItemActionMelee.InventoryDataMelee inventoryDataMelee2 = inventoryDataMelee;
				inventoryDataMelee2.ray.origin = inventoryDataMelee2.ray.origin + new Vector3(0f, -0.7f, 0f);
			}
		}
		else if (holdingEntity.GetAttackTarget() != null)
		{
			Vector3 direction = holdingEntity.GetAttackTargetHitPosition() - inventoryDataMelee.ray.origin;
			inventoryDataMelee.ray = new Ray(inventoryDataMelee.ray.origin, direction);
		}
		ItemActionMelee.InventoryDataMelee inventoryDataMelee3 = inventoryDataMelee;
		inventoryDataMelee3.ray.origin = inventoryDataMelee3.ray.origin - 0.15f * inventoryDataMelee.ray.direction;
		int modelLayer = holdingEntity.GetModelLayer();
		holdingEntity.SetModelLayer(2, false);
		float distance = Utils.FastMax(this.Range, this.BlockRange) + 0.15f;
		if ( holdingEntity.IsBreakingBlocks)
		{
			Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, 1073807360, 128, 0.4f);
		}
		else
		{
			EntityAlive x = null;
			int layerMask = -538767381;
			if (Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, layerMask, 128, this.SphereRadius))
			{
				x = (ItemActionAttack.GetEntityFromHit(Voxel.voxelRayHitInfo) as EntityAlive);
			}
			if (x == null)
			{
				Voxel.Raycast(inventoryDataMelee.invData.world, inventoryDataMelee.ray, distance, -538488837, 128, this.SphereRadius);
			}
		}
		holdingEntity.SetModelLayer(modelLayer, false);
		return _actionData.GetUpdatedHitInfo();
	}
}