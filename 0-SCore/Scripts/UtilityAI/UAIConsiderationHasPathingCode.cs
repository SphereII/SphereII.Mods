using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationHasPathingCode : UAIConsiderationTargetType
    {
        public override float GetScore(Context _context, object target)
        {
            var position = EntityUtilities.GetNewPositon(_context.Self.entityId);
            if (position == Vector3.zero)
                return 0f;
            return 1f;
        }
    }
}