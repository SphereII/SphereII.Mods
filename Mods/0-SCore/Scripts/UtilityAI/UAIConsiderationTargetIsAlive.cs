using Harmony.ZombieFeatures;

namespace UAI
{
    public class UAIConsiderationTargetIsDead : UAIConsiderationTargetIsAlive
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;

        }
    }
    public class UAIConsiderationTargetIsAlive : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;
            if (InertEntity.IsInert(targetEntity)) return 0f;
            
            return targetEntity.IsAlive() ? 1f : 0f;
        }
    }
}