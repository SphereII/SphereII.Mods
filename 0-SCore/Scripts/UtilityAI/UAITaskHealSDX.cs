// using this namespace is necessary for Utilities AI Tasks
//       <task class="HealSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.

using System.Collections;
using UnityEngine;

namespace UAI
{
    public class UAITaskHealSDX : UAITaskBase
    {
        public override void Start(Context _context)
        {
            // If the NPC doesn't have this cvar, give it an initial value, so it can heal somewhat from a bandage that it found in its inventory.
            if (!_context.Self.Buffs.HasCustomVar("medRegHealthIncSpeed"))
                _context.Self.Buffs.SetCustomVar("medRegHealthIncSpeed", 1f, true);

            base.Start(_context);
        }

        public override void Update(Context _context)
        {
            base.Update(_context);
            Stop(_context);
        }
    }
}