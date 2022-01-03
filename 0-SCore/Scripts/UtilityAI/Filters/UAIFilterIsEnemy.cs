namespace UAI
{
    /// <summary>
    /// Filter that tests whether the target entity is an enemy of the targeting entity.
    /// </summary>
    public class UAIFilterIsEnemy : IUAITargetFilter<Entity>
    {
        private readonly EntityAlive self;

        public UAIFilterIsEnemy(EntityAlive self)
        {
            this.self = self;
        }

        public bool Test(Entity target)
        {
            return EntityTargetingUtilities.IsEnemy(self, target);
        }
    }
}
