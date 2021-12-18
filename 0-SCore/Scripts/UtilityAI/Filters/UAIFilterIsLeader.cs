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

        public UAIFilterIsLeader(EntityAlive self)
        {
            this.self = self;
        }

        public bool Test(Entity target)
        {
            var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
            if (myLeader == null)
                return false;

            return target.entityId == myLeader.entityId;
        }
    }
}
