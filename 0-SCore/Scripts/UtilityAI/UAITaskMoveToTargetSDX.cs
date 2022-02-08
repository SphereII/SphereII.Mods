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
                var num = _context.Self.CalcInvestigateTicks(Constants.cEnemySenseMemory * 20, entityAlive);
                _context.Self.SetInvestigatePosition(_position, num);
            }
            base.Stop(_context);
        }
        public override void Update(Context _context)
        {
            // We don't call base.Update() here, because it's checking if the entity has no path, then it stops the task.
            // However, at times, we do want the opportunity to repath without stopping the task. This stops the pauses the entity does.
            SCoreUtils.CheckForClosedDoor(_context);
            CheckProximityToPosition(_context);

            if (_context.ActionData.Target.GetType() == typeof(Vector3))
            {
                if (_context.Self.getNavigator().noPathAndNotPlanningOne())
                {
                    this.Stop(_context);
                }
            }

            if (SCoreUtils.IsBlocked(_context))
            {
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

            if (!_context.Self.navigator.noPathAndNotPlanningOne())
            {
                // The entity hasn't moved very much, keep going.
                if (Vector3.Distance(entityAlive.position, _position) < 2f)
                    return;

                // I'm close enough to the entity.
                if (Vector3.Distance(_context.Self.position, entityAlive.position) < 2f)
                    return;
            }
            
            _position = entityAlive.position;
            if ( SCoreUtils.CanSee(_context.Self, entityAlive) )
                _context.Self.moveHelper.SetMoveTo(_position, true);
            else
                SCoreUtils.FindPath(_context, _position, run);

            //// Our target has moved too much.
            //if (Vector3.Distance(entityAlive.position, _position) > 2f)
            //{
            //    _position = entityAlive.position;
            //    _context.Self.navigator.clearPath();
            //    SCoreUtils.FindPath(_context, _position, run);
            //}

            //_context.Self.moveHelper.SetMoveTo(_position, true);
        }



    }
}