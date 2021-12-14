// using this namespace is necessary for Utilities AI Tasks
//       <task class="IdleSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.

using UnityEngine;

namespace UAI
{
    public class UAITaskIdleSDX : UAITaskBase
    {
        public override void Update(Context _context)
        {
            base.Update(_context);

            // Don't do anything until the entity touches the ground; avoid the free in mid-air scenario.
            if (!_context.Self.onGround) return;

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _context.Self.RotateTo(entityAlive, 15f, 15f);
                _context.Self.SetLookPosition(entityAlive.getHeadPosition());
                var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId);
                if ( leader != null && entityAlive.entityId == leader.entityId)
                    SCoreUtils.SetCrouching(_context, entityAlive.Crouching);
                else
                    SCoreUtils.SetCrouching(_context);
            }

            EntityUtilities.Stop(_context.Self.entityId);
            if (SCoreUtils.IsEnemyNearby(_context))
                Stop(_context);
        }
    }
}