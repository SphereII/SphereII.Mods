namespace UAI
{

    public class UAIConsiderationTargetIsAlly : UAIConsiderationTargetIsEnemy
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationTargetIsFriendly : UAIConsiderationTargetIsEnemy
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationTargetIsEnemy : UAIConsiderationTargetFactionStanding
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILogging";

        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            if (EntityTargetingUtilities.IsEnemy(_context.Self, targetEntity)) 
                return 1f;

            // [Karl] Commented out because this check is done in IsEnemy.
            // If the target entity is attacking our allies, target them too.
            //var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId);
            //if (EntityTargetingUtilities.IsFightingFollowers(leader, targetEntity))
            //    return 1f;

            var myRelationship = FactionManager.Instance.GetRelationshipTier(_context.Self, targetEntity);
            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"\tChecking Relationship between: {_context.Self.EntityName} (me) and {targetEntity.EntityName} (them) is {myRelationship} ");
            switch (myRelationship)
            {
                case FactionManager.Relationship.Hate:
                    return 1f;
                case FactionManager.Relationship.Dislike:  
                    return 0.5f;
                case FactionManager.Relationship.Neutral:
                case FactionManager.Relationship.Like:
                case FactionManager.Relationship.Love:
                case FactionManager.Relationship.Leader:
                default:
                    return 0f;
            }
        }
    }
}