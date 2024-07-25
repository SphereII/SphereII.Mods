using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAI
{
    public class UAIFilterIsPlayer : IUAITargetFilter<Entity>
    {
        public UAIFilterIsPlayer(EntityAlive self)
        {
            // self is a parameter so we can use Activator.CreateInstance, but we don't need it
        }

        public bool Test(Entity target)
        {
            return target is EntityPlayer;
        }
    }
}
