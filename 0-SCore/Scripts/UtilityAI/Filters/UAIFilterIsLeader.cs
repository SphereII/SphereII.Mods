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
        private readonly EntityAliveSDX selfSDX;

        public UAIFilterIsLeader(EntityAlive self)
        {
            this.self = self;
            selfSDX = self as EntityAliveSDX;
        }

        public bool Test(Entity target)
        {
            // If its the EntityAliveSDX, do a quick check to see if its hirable or not.
            if (selfSDX != null && !selfSDX.isHirable) return false;

            var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
            if (myLeader == null)
                return false;

            return target.entityId == myLeader.entityId;
        }
    }
}
