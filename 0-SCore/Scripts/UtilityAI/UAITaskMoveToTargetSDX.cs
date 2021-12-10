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

            // Add in a blocked time condition.
            if (SCoreUtils.IsBlocked(_context))
                this.Stop(_context);

            CheckProximityToEnemy(_context);

        }

        public void CheckProximityToEnemy(Context _context)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive == null || entityAlive.IsDead())
            {
                Stop(_context);
                return;
            }

            _context.Self.RotateTo(entityAlive, 15f, 15f);
            _context.Self.SetLookPosition(entityAlive.position);

            if (!_context.Self.navigator.noPathAndNotPlanningOne())
            {
                var dist = Vector3.Distance(entityAlive.position, _position);
                if (dist < distance)
                    return;
            }

            // If the leader has moved quite a bit, re-position.
            _position = entityAlive.position;
            SCoreUtils.FindPath(_context, _position, true);

        }

        public override void Start(Context _context)
        {
            SCoreUtils.SetCrouching(_context);
            base.Start(_context);

            Debug.Log($"{_context.Self.EntityName}  Boundary: {_context.Self.boundingBox.ToString()}");

            // if distance is set correctly, set it to 1.5f
            if (distance < 0.001)
                distance = 1.5f;
        }

    }
}