namespace UAI
{
    /// <summary>
    /// Filter that tests whether the target entity is an ally of the targeting entity.
    /// An ally is an entity that is your leader/owner, or has the same leader/owner as you.
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
            // TODO should we also include entities that are of the same faction?
            return SCoreUtils.IsAlly(self, target);
        }
    }
}
