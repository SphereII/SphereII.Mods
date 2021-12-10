
namespace UAI
{
    public class UAIConsiderationTargetSameClass : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            if (targetEntity.entityClass == _context.Self.entityClass)
                return 1f;

            return 0f;
        }
    }
}