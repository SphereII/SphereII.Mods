namespace UAI
{
    public class UAIConsiderationNotHasTargetSDX : UAIConsiderationHasTargetSDX
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null ? 1f : 0f;
        }
    }

    public class UAIConsiderationHasTargetSDX : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null ? 0f : 1f;
        }
    }
}