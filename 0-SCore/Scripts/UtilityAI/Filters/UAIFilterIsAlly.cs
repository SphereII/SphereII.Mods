namespace UAI
{
    /// <summary>
    /// Filter that tests whether the target entity is an ally of the targeting entity.
    /// </summary>
    public class UAIFilterIsAlly : IUAITargetFilter<Entity>
    {
        private readonly EntityAlive self;

        public UAIFilterIsAlly(EntityAlive self)
        {
            this.self = self;
        }

        public bool Test(Entity target)
        {
            // We can't use IsAlly because that only returns true if you have the same leader as
            // the target, so will always fail for NPCs that don't have a leader.
            return EntityTargetingUtilities.IsFriend(self, target);
        }
    }
}
