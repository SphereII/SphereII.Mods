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
                if (_context.Self.GetDistance(entityAlive) < 1.5)
                {
                    var moveTo = entityAlive.GetLookVector();
                    //_context.Self.RotateTo(moveTo.x, moveTo.y, moveTo.z, 30f,30f);                    
                    _context.Self.moveHelper.SetMoveTo(moveTo, true);
                    //SCoreUtils.MoveBack(_context, _context.Self.position + Vector3.back);
                    return;
                }

                _context.Self.RotateTo(entityAlive, 15f, 15f);
                _context.Self.SetLookPosition(entityAlive.getHeadPosition());


                SCoreUtils.SetCrouching(_context, entityAlive.Crouching);
            }


            EntityUtilities.Stop(_context.Self.entityId);
            if (SCoreUtils.IsEnemyNearby(_context))
                Stop(_context);
        }
    }
}