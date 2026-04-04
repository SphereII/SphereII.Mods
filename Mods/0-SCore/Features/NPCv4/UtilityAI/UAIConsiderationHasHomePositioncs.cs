namespace UAI
{
    public class UAIConsiderationHasHomePositionV4 : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            return _context.Self.hasHome() == false ? 0f : 1f;
        }
    }
    
    public class UAIConsiderationNotHasHomePositionV4 : UAIConsiderationHasHomePositionV4
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
}