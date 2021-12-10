
namespace UAI
{
    public class UAIConsiderationTargetLeader : UAIConsiderationBase
    {
        private readonly string value = "Leader";

        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            if (!_context.Self.Buffs.HasCustomVar(value))
                return 0f;

            if (targetEntity.entityId == (int)_context.Self.Buffs.GetCustomVar(value))
                return 1f;

            return 0f;
        }
    }
}