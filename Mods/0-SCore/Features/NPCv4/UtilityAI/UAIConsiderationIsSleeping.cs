using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationIsNotSleepingV4 : UAIConsiderationCanHearTarget
    {
        public override float GetScore(Context _context, object target)
        {
            return (base.GetScore(_context, target) == 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationIsSleepingV4 : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target) as EntityPlayer;
            if (targetEntity == null)
                return 0f;

            if (_context.Self.IsSleeping)
                return 1f;
            return 0f;

        }
    }
}