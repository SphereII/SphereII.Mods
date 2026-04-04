using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationNotTargetDistanceSDXV4 : UAIConsiderationTargetDistanceSDXV4
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationTargetDistanceSDXV4 : UAIConsiderationTargetDistance
    {
        public override float GetScore(Context _context, object target)
        {
            var score = base.GetScore(_context, target);
            if (score < 0.01) return 0;
            
            return score;
        }
    }
}