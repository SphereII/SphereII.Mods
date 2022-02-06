using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationPathBlocked : UAIConsiderationBase
    {
		public override float GetScore(Context _context, object target)
		{
			Vector3i zero = Vector3i.zero;
			if (this.IsPathUsageBlocked(_context) && CanAttackBlocks(_context.Self, out zero))
			{
				_context.ConsiderationData.WaypointTargets.Add(zero.ToVector3());
				return 1f;
			}
			return 0f;

			//Vector3i zero = Vector3i.zero;

			//EntityMoveHelper moveHelper = _context.Self.moveHelper;
			//if ((_context.Self.Jumping && !moveHelper.IsDestroyArea) || _context.Self.bodyDamage.CurrentStun != EnumEntityStunType.None)
			//{
			//	return 0f;
			//}
			//if (moveHelper.BlockedTime > 0.35f && moveHelper.CanBreakBlocks)
			//{
			//	if (moveHelper != null && moveHelper.HitInfo != null)
			//	{
			//		Vector3i blockPos = moveHelper.HitInfo.hit.blockPos;
			//		if (_context.Self.world.GetBlock(blockPos).Equals(BlockValue.Air))
			//		{
			//			return 0f;
			//		}
			//		float num = moveHelper.CalcBlockedDistanceSq();
			//		float num2 = _context.Self.m_characterController.GetRadius() + 0.6f;
			//		if (num <= num2 * num2)
			//		{
			//			Log.Out($"Target BLock: {_context.Self.world.GetBlock(blockPos).Block.GetBlockName()}");
			//			_context.ConsiderationData.WaypointTargets.Add(blockPos.ToVector3());
			//			return 1f;
			//		}
			//	}
			//}
			//return 0f;
		}

		public bool IsPathUsageBlocked(Context _context)
		{
			EntityAlive self = _context.Self;
			if (self.getNavigator() == null)
			{
				return false;
			}
			if (self.getNavigator().getPath() == null)
			{
				return false;
			}
			Vector3 targetPos = GetTargetPos(self);
			Vector3i vector3i = World.worldToBlockPos(targetPos);
			float distanceSq = self.getNavigator().getPath().GetEndPoint().GetDistanceSq(vector3i.x, vector3i.y, vector3i.z);
			if (distanceSq < 2.1f)
			{
				return false;
			}
			float distanceSq2 = self.GetDistanceSq(targetPos);
			return self.GetDistanceSq(targetPos) < 256f || distanceSq > distanceSq2;
		}

		public static bool CanAttackBlocks(EntityAlive theEntity, out Vector3i attackPos)
		{
			float num;
			BlockValue blockValue;
			return CanAttackBlocks(theEntity, GetTargetYaw(theEntity), out num, out attackPos, out blockValue);
		}

		public static bool CanAttackBlocks(EntityAlive theEntity, float yawAngle, out float attackAngle, out Vector3i attackAddPos, out BlockValue attackBlockValue)
		{
			int num = Utils.Fastfloor(theEntity.position.x );
			int num2 = Utils.Fastfloor(theEntity.position.y + 0.5f);
			int num3 = Utils.Fastfloor(theEntity.position.z);
			attackAddPos = new Vector3i(0, 1, 0);
			bool flag = isPosBlocked(theEntity, num, num2 + 1, num3, out attackBlockValue);
			attackAngle = 0f;
			if (!flag)
			{
				attackAddPos = new Vector3i(0, 0, 0);
				flag = isPosBlocked(theEntity, num, num2, num3, out attackBlockValue);
				attackAngle = -65f;
			}
			if (!flag)
			{
				int num4 = 0;
				int num5 = 0;
				float f = -Mathf.Sin(yawAngle * 0.0175f - 3.1415927f);
				float f2 = -Mathf.Cos(yawAngle * 0.0175f - 3.1415927f);
				if (Mathf.Abs(f) > 0.1f)
				{
					num4 = (int)Mathf.Sign(f);
				}
				if (Mathf.Abs(f2) > 0.1f)
				{
					num5 = (int)Mathf.Sign(f2);
				}
				if (num4 < 0)
					num4 = 1;
				attackAddPos = new Vector3i(num4, 1, num5);
				flag = isPosBlocked(theEntity, num + num4, num2 + 1, num3 + num5, out attackBlockValue);
				attackAngle = 0f;
				Log.Out($"1 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				if (!flag)
				{
					attackAddPos = new Vector3i(num4, 0, num5);
					flag = isPosBlocked(theEntity, num + num4, num2, num3 + num5, out attackBlockValue);
					attackAngle = -45f;
					Log.Out($"2 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
				if (!flag)
				{
					attackAddPos = new Vector3i(2 * num4, 1, 2 * num5);
					flag = isPosBlocked(theEntity, num + 2 * num4, num2 + 1, num3 + 2 * num5, out attackBlockValue);
					attackAngle = 0f;
					Log.Out($"3 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
				if (!flag)
				{
					attackAddPos = new Vector3i(2 * num4, 0, 2 * num5);
					flag = isPosBlocked(theEntity, num + 2 * num4, num2, num3 + 2 * num5, out attackBlockValue);
					attackAngle = -45f;
					Log.Out($"4 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
				if (!flag)
				{
					attackAddPos = new Vector3i(num4, 0, 0);
					flag = isPosBlocked(theEntity, num + num4, num2, num3, out attackBlockValue);
					attackAngle = -45f;
					Log.Out($"5 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
				if (!flag)
				{
					attackAddPos = new Vector3i(2 * num4, 1, 0);
					flag = isPosBlocked(theEntity, num + 2 * num4, num2 + 1, num3, out attackBlockValue);
					attackAngle = 0f;
					Log.Out($"6 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
				if (!flag)
				{
					attackAddPos = new Vector3i(0, 0, num5);
					flag = isPosBlocked(theEntity, num, num2, num3 + num5, out attackBlockValue);
					attackAngle = -45f;
					Log.Out($"7 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
				if (!flag)
				{
					attackAddPos = new Vector3i(0, 1, 2 * num5);
					flag = isPosBlocked(theEntity, num, num2 + 1, num3 + 2 * num5, out attackBlockValue);
					attackAngle = 0f;
					Log.Out($"8 {attackAddPos} {attackAngle} {attackBlockValue.Block.GetBlockName()}");
				}
			}
			attackAddPos += new Vector3i(num, num2, num3);
			return flag;
		}

		private static bool isPosBlocked(EntityAlive theEntity, int _x, int _y, int _z, out BlockValue attackBlockValue)
		{
			attackBlockValue = theEntity.world.GetBlock(_x, _y, _z);
			
			var results= attackBlockValue.Block.IsMovementBlocked(theEntity.world, new Vector3i(_x, _y, _z), attackBlockValue, BlockFace.None);
			Log.Out($"isPosBlocked: {attackBlockValue.Block.GetBlockName()} Position: {new Vector3i(_x, _y, _z)} : Results: {results}");
			return results;
		}

		private static float GetTargetYaw(EntityAlive theEntity)
		{
			if (theEntity.GetAttackTarget() != null)
			{
				return theEntity.YawForTarget(theEntity.GetAttackTarget());
			}
			return theEntity.YawForTarget(GetTargetPos(theEntity));
		}

		protected static Vector3 GetTargetPos(EntityAlive theEntity)
		{
			if (theEntity.GetAttackTarget() != null)
			{
				return theEntity.GetAttackTarget().GetPosition();
			}
			return theEntity.InvestigatePosition;
		}
	}
}
