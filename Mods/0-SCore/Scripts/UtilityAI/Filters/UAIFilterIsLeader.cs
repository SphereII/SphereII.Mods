using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAI
{
    public class UAIFilterIsLeader : IUAITargetFilter<Entity>
    {
        private readonly EntityAlive self;
        private readonly IEntityAliveSDX selfSDX;

        public UAIFilterIsLeader(EntityAlive self)
        {
            this.self = self;
            selfSDX = self as IEntityAliveSDX;
        }

        public bool Test(Entity target)
        {
            // If its an SDX entity, do a quick check to see if its hirable or not.
            if (selfSDX != null && !selfSDX.IsHirable) return false;

            var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
            if (myLeader == null)
                return false;

            return target.entityId == myLeader.entityId;
        }
    }
}
