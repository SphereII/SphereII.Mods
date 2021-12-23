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
            return SCoreUtils.IsEnemy(self, target);
            //if (!(target is EntityAlive entity))
            //    return false;

            //return !EntityUtilities.CheckFaction(self.entityId, entity);
        }
    }
}
