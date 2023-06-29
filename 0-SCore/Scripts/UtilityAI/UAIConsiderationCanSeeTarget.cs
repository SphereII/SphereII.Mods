using System.Collections.Generic;
using System.Globalization;
using Harmony.ZombieFeatures;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationCanNotSeeTarget : UAIConsiderationCanSeeTarget
    {
        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity != null && targetEntity.IsDead())
                return 0f;

            return (base.GetScore(_context, target) == 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationCanSeeTarget: UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;
            
            if (InertEntity.IsInert(targetEntity)) return 0f;

            return SCoreUtils.CanSee(_context.Self, targetEntity) ? 1f : 0f;
        }
    }
}