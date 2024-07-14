namespace UAI
{
    /// <summary>
    /// Utility AI task to determine if the target entity has an investigate position.
    /// </summary>
    public class UAIConsiderationHasInvestigatePosition : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            if (target is not EntityAlive entityAlive)
                return 0f;

            return entityAlive.HasInvestigatePosition ? 1f : 0f;
        }
    }

    /// <summary>
    /// Utility AI task to determine if the target entity does not have an investigate position.
    /// </summary>
    public class UAIConsiderationHasNoInvestigatePosition : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            if (target is not EntityAlive entityAlive)
                return 0f;

            return entityAlive.HasInvestigatePosition ? 0f : 1f;
        }
    }
}
