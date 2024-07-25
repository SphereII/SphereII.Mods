using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace UAI
{
    public class UAIConsiderationSelfNotAttackTarget : UAIConsiderationSelfAttackTarget
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationSelfAttackTarget: UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {

            // Do we have an attack target?
            EntityAlive attackTarget = UAIUtils.ConvertToEntityAlive(_context.Self.GetAttackTarget());
            if (attackTarget == null) return 0f;

            // Clamp the max timeout to 100.
            var attackTimeOut = (float)_context.Self.GetAttackTimeoutTicks();
            if (attackTimeOut > 100)
                attackTimeOut = 100;

            // Is our consideration target an entity alive?
            EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
            if (entityAlive != null)
            {
                // Are they the same entity?
                if ( attackTarget.entityId == entityAlive.entityId )    
                    return Mathf.Clamp01(Mathf.Max(attackTimeOut / 100));
            }
            return 0f;
        }
    }
}