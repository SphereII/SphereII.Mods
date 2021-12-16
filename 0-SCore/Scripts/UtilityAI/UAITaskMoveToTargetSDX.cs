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
            base.Update(_context);

            SCoreUtils.CheckForClosedDoor(_context);
            CheckProximityToPosition(_context);
        }

        public override void Stop(Context _context)
        {
            _context.Self.RotateTo(_position.x, _position.y, _position.z, 30f, 30f);
            _context.Self.SetLookPosition(_position);
            base.Stop(_context);
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

            if (!_context.Self.navigator.noPathAndNotPlanningOne())
            {
                var dist = Vector3.Distance(entityAlive.position, _position);
                if (dist < distance)
                    return;
            }

            // If the leader has moved quite a bit, re-position.
            _position = entityAlive.position;
            SCoreUtils.FindPath(_context, _position, run);
        }

        public override void Start(Context _context)
        {
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