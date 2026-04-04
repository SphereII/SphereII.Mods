namespace UAI
{
    /// <summary>
    /// Grabs the score from the UAIConsiderationIsAlertedV4, and flips it.
    /// </summary>
    public class UAIConsiderationIsNotAlertedV4 : UAIConsiderationIsAlertedV4
    {
        public override float GetScore(Context context, object target)
        {
            var score = base.GetScore(context, target);
            return score == 0f ? 1f : 0f;
        }
    }

    /// <summary>
    /// Checks the current entity to see if the IsAlert is toggled on, which could indicate that the entity
    /// is aware that something is around it.
    ///
    /// This consideration also checks to see if the entity has an attack / revenge target, and if that target is dead.
    ///
    /// A dead target will cause this consideration to fail, even if the entity is alerted.
    /// </summary>
    public class UAIConsiderationIsAlertedV4 : UAIConsiderationBase
    {
        public override float GetScore(Context context, object target)
        {
            EntityAlive attackTarget = null;
            EntityAlive revengeTarget = null;
            
            if (!EntityUtilities.GetAttackAndRevengeTarget(context.Self.entityId, ref attackTarget, ref revengeTarget))
                return !context.Self.IsAlert ? 0f : 1f;
            
            if (attackTarget != null && attackTarget.IsAlive()) return 1f;

            if (revengeTarget != null && revengeTarget.IsAlive()) return 1f;

            return !context.Self.IsAlert ? 0f : 1f;
        }
    }
}