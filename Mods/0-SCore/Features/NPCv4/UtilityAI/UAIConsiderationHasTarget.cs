namespace UAI
{
    public class UAIConsiderationNotHasTargetSDXV4 : UAIConsiderationHasTargetSDXV4
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null ? 1f : 0f;
        }
    }

    public class UAIConsiderationHasTargetSDXV4 : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null ? 0f : 1f;
        }
    }
}