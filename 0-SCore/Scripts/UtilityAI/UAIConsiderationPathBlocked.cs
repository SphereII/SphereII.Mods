using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationPathBlockedSDX : UAIConsiderationPathBlocked
    {
        public override float GetScore(Context _context, object target)
        {
            //SphereCache.BreakingBlockCache.TryGetValue(_context.Self.entityId, out var previousTarget) ;
            //if ( previousTarget != Vector3i.zero)
            //{
            //    if (!GameManager.Instance.World.GetBlock(previousTarget).Equals(BlockValue.Air))
            //    {
            //        _context.ConsiderationData.WaypointTargets.Add(previousTarget.ToVector3());
            //        return 1f;
            //    }
            //    SphereCache.BreakingBlockCache[_context.Self.entityId] = Vector3i.zero;
            //}
            if (_context.Self.moveHelper.BlockedTime > 0.35f && _context.Self.moveHelper.CanBreakBlocks)
            {

                Vector3i blockPos = Vector3i.zero;
                if (_context.Self.moveHelper != null && _context.Self.moveHelper.HitInfo != null)
                {
                    blockPos = _context.Self.moveHelper.HitInfo.hit.blockPos;
                    if (GameManager.Instance.World.GetBlock(blockPos).Equals(BlockValue.Air))
                    {
                        return 0f;
                    }
                }
                float num = _context.Self.moveHelper.CalcBlockedDistanceSq();
                float num2 = _context.Self.m_characterController.GetRadius() + 0.6f;
                if (num <= num2 * num2)
                {
                    if (blockPos != Vector3i.zero)
                    {
                        if (GameManager.Instance.World.GetBlock(blockPos).Equals(BlockValue.Air))
                            return 0;

                        SphereCache.BreakingBlockCache[_context.Self.entityId] = new Vector3i(blockPos);
                        _context.ConsiderationData.WaypointTargets.Add(new Vector3i(blockPos));

                        return 1f;
                    }
                }
            }
            return 0f;

            //Vector3i zero = Vector3i.zero;
            //if (this.IsPathUsageBlocked(_context) && UAIConsiderationPathBlocked.CanAttackBlocks(_context.Self, out zero))
            //{
            //    _context.ConsiderationData.WaypointTargets.Add(zero.ToVector3());
            //    return 1f;
            //}
            //return 0f;
        }

    }
}