using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using GamePath;
namespace UAI
{

    public class UAIConsiderationTrapped : UAIConsiderationPathBlocked
    {
        public override float GetScore(Context _context, object target)
        {
			EntityMoveHelper moveHelper = _context.Self.moveHelper;
			
			// if you can't break blocks, don't.
			if (!moveHelper.CanBreakBlocks) return 0f;

			if (_context.Self.IsBreakingBlocks)
			{
				if (SphereCache.BreakingBlockCache.ContainsKey(_context.Self.entityId))
				{
					var blockPos = SphereCache.BreakingBlockCache[_context.Self.entityId];
					if (!GameManager.Instance.World.GetBlock(blockPos).Equals(BlockValue.Air))
					{
						_context.ConsiderationData.WaypointTargets.Add(new Vector3i(blockPos));
						return 1f;
					}
					SphereCache.BreakingBlockCache.Remove(_context.Self.entityId);
				}

				return 1f;
			}
			if (moveHelper.BlockedTime > 0.35f && moveHelper.CanBreakBlocks)
			{
				if (SphereCache.BreakingBlockCache.ContainsKey(_context.Self.entityId))
					SphereCache.BreakingBlockCache.Remove(_context.Self.entityId);

				if (_context.Self.moveHelper != null && _context.Self.moveHelper.HitInfo != null)
				{
					Vector3i blockPos = _context.Self.moveHelper.HitInfo.hit.blockPos;
					if (_context.Self.world.GetBlock(blockPos).Equals(BlockValue.Air)) return 0f;

					float num = moveHelper.CalcBlockedDistanceSq();
					float num2 = _context.Self.m_characterController.GetRadius() + 0.6f;
					if (num <= num2 * num2)
					{
						Log.Out($"Blocked at {blockPos}");
						SphereCache.BreakingBlockCache[_context.Self.entityId] = new Vector3i(blockPos);
						_context.ConsiderationData.WaypointTargets.Add(new Vector3i(blockPos));
						return 1f;
					}
				}
			}

			return 0f;
		
		}

    }
}