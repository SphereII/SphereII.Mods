using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationNotPathBlockedSDX : UAIConsiderationPathBlockedSDX
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationPathBlockedSDX : UAIConsiderationPathBlocked
    {
        // Over-riding the IsPathUsage because of the Door Busting task will destroy the block and potentially stop the pathing, causing the path to be null afterwards.
        public new bool IsPathUsageBlocked(Context _context)
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
            Vector3 targetPos = UAIConsiderationPathBlocked.GetTargetPos(self);
            Vector3i vector3i = World.worldToBlockPos(targetPos);
            var path = self.getNavigator().getPath();
            if (path == null) return false;
            var endPoint = path.GetEndPoint();
            if (endPoint == null) return false;
            float distanceSq = endPoint.GetDistanceSq(vector3i.x, vector3i.y, vector3i.z);
            if (distanceSq < 2.1f)
            {
                return false;
            }
            float distanceSq2 = self.GetDistanceSq(targetPos);
            return self.GetDistanceSq(targetPos) < 256f || distanceSq > distanceSq2;
        }

        public override float GetScore(Context _context, object target)
        {
            if (_context.Self.IsBreakingDoors)
                return 1f;

            if (_context.Self.moveHelper.CanBreakBlocks == false) return 0f;

            // Check to see if the base allows us to run. If not, do the extra checks.
            Vector3i zero = Vector3i.zero;
            if (this.IsPathUsageBlocked(_context) && UAIConsiderationPathBlocked.CanAttackBlocks(_context.Self, out zero))
            {
                _context.ConsiderationData.WaypointTargets.Add(zero.ToVector3());
                return 1f;
            }

            if (_context.Self.moveHelper.BlockedTime >= 0.35f )
            {
                Vector3i blockPos = Vector3i.zero;
                if (_context.Self.moveHelper != null && _context.Self.moveHelper.HitInfo != null)
                {
                    blockPos = _context.Self.moveHelper.HitInfo.hit.blockPos;
                    if (GameManager.Instance.World.GetBlock(blockPos).Equals(BlockValue.Air))
                        return 0f;

                    float num = _context.Self.moveHelper.CalcBlockedDistanceSq();
                    float num2 = _context.Self.m_characterController.GetRadius() + 0.6f;
                    if (num <= num2 * num2)
                    {
                        if (blockPos != Vector3i.zero)
                        {
                            if (GameManager.Instance.World.GetBlock(blockPos).Equals(BlockValue.Air))
                            {
                                //  Log.Out("Block is air. 2");
                                _context.Self.IsBreakingBlocks = false;

                                return 0f;
                            }
                            // Log.Out("I am blocked.");
                            _context.Self.IsBreakingBlocks = true;
                            _context.ConsiderationData.WaypointTargets.Add(new Vector3i(blockPos));
                            return 1f;
                        }
                    }
                }

                

            }
            return 0f;
        }
    }
}