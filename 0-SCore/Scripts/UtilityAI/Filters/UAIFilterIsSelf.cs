namespace UAI
{
    /// <summary>
    /// Filter that tests whether the target entity is the targeting entity.
    /// </summary>
    public class UAIFilterIsSelf : IUAITargetFilter<Entity>
    {
        private readonly EntityAlive self;

        public UAIFilterIsSelf(EntityAlive self)
        {
            this.self = self;
        }

        public bool Test(Entity target)
        {
            return self.entityId == target.entityId;
        }
    }
}
