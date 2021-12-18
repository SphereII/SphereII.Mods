using GamePath;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToTargetSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToTargetSDX : UAITaskMoveToTarget
    {
        private Vector3 _position;

        public override void Update(Context _context)
        {
            // We don't call base.Update() here, because it's checking if the entity has no path, then it stops the task.
            // However, at times, we do want the opportunity to repath without stopping the task. This stops the pauses the entity does.
            SCoreUtils.CheckForClosedDoor(_context);
            CheckProximityToPosition(_context);
        }

        public void CheckProximityToPosition(Context _context)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive == null) return;

            if (entityAlive.IsDead())
            {
                Stop(_context);
                return;
            }

            // Check if our entity has moved.
            _position = entityAlive.position;
            
            _context.Self.moveHelper.SetMoveTo(_position, true);
//            var speed = SCoreUtils.SetSpeed(_context, run);
        }

        public override void Start(Context _context)
        {
            // Reset crouch status to default
            SCoreUtils.SetCrouching(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
                _position = entityAlive.position;

            if (_context.ActionData.Target is Vector3 vector)
                _position = vector;

            _context.Self.RotateTo(_position.x, _position.y, _position.z, 30f, 30f);
            _context.Self.SetLookPosition(_position);

            SCoreUtils.FindPath(_context, _position, run);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }

    }
}