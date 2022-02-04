using GamePath;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToTargetSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToTargetSDX : UAITaskMoveToTarget
    {
        public Vector3 _position;

        public override void Start(Context _context)
        {
            // Reset crouch status to default
            SCoreUtils.SetCrouching(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _position = entityAlive.position;
                if (entityAlive.IsWalkTypeACrawl())
                    _position = entityAlive.getHeadPosition();
            }
            if (_context.ActionData.Target is Vector3 vector)
                _position = vector;

            SCoreUtils.FindPath(_context, _position, run);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }

        public override void Stop(Context _context)
        {
            // Did we lose our target? do we need to keep looking for it?
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null && entityAlive.IsAlive())
            {
                int num = _context.Self.CalcInvestigateTicks(Constants.cEnemySenseMemory * 20, entityAlive);
                _context.Self.SetInvestigatePosition(_position, 1200);
            }
            base.Stop(_context);
        }
        public override void Update(Context _context)
        {
            // We don't call base.Update() here, because it's checking if the entity has no path, then it stops the task.
            // However, at times, we do want the opportunity to repath without stopping the task. This stops the pauses the entity does.
            SCoreUtils.CheckForClosedDoor(_context);
            CheckProximityToPosition(_context);

            if (SCoreUtils.IsBlocked(_context))
            {
                // _position = _position = RandomPositionGenerator.CalcTowards(_context.Self, 2,2, 3, _position);
                //SCoreUtils.FindPath(_context, _position, run);
                Stop(_context);
                return;
            }


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

            SCoreUtils.SetLookPosition(_context, _position);

            // We've reached the target's position.
            var distance = Vector3.Distance(_context.Self.position, _position);
            if (distance < 1f)
            {
                _context.Self.navigator.clearPath();
                Stop(_context);
                return;
            }

            //// if there's not much distance between where we are aiming for, and where the entity now is, keep going.
            //var difference = Vector3.Distance(entityAlive.position, _position);
            //if (difference < 2f)
            //    return;



            // Update our position
            _position = entityAlive.position;

            if (_context.Self.navigator.noPathAndNotPlanningOne())
                SCoreUtils.FindPath(_context, _position, run);

            _context.Self.moveHelper.SetMoveTo(_position, true);

        }



    }
}