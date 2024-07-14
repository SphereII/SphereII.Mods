namespace UAI
{
    public class UAIConsiderationIsNotTargeted : UAIConsiderationIsTargeted
    {
        public override float GetScore(Context _context, object target)
        {
            return (base.GetScore(_context, target) == 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationIsTargeted : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            var attackTarget = EntityUtilities.GetAttackOrRevengeTarget(targetEntity.entityId);
            if (attackTarget == null)
                return 0f;

            return (attackTarget.entityId == _context.Self.entityId) ? 1f : 0f;
        }
    }
}