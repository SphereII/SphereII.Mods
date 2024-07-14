using System.Collections.Generic;
using System.Globalization;

namespace UAI
{
    public class UAIConsiderationSelfNotHasCVar : UAIConsiderationSelfHasCVar
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationSelfHasCVar : UAIConsiderationTargetHasCVar
    {
        public override float GetScore(Context _context, object target)
        {
            return base.GetScore(_context, _context.Self);
        }
    }
}