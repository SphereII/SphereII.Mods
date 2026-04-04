using System.Collections.Generic;
using System.Globalization;

namespace UAI
{
    public class UAIConsiderationSelfNotHasCVarV4 : UAIConsiderationSelfHasCVarV4
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationSelfHasCVarV4 : UAIConsiderationTargetHasCVarV4
    {
        public override float GetScore(Context _context, object target)
        {
            return base.GetScore(_context, _context.Self);
        }
    }
}