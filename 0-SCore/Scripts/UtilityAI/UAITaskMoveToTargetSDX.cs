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

            // If we're a sleeper, we can make sounds now
            _context.Self.SleeperSupressLivingSounds = false;

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _position = entityAlive.position;
                if (entityAlive.IsWalkTypeACrawl())
                    _position = entityAlive.getHeadPosition();
                _context.Self.SetInvestigatePosition(_position, 1200);

                var isEnemy = EntityTargetingUtilities.IsEnemy(_context.Self, entityAlive);

                // If the target is a new enemy, play the "alert" sound.
                if (isEnemy && EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) != entityAlive)
                {
                    _context.Self.PlayOneShot(_context.Self.GetSoundAlert());
                }

                // We don't set the attack target yet, but the game should know if we're
                // approaching an enemy or player.
                _context.Self.ApproachingEnemy = isEnemy;
                _context.Self.ApproachingPlayer = entityAlive is EntityPlayer;
            }

            if (_context.ActionData.Target is Vector3 vector)
                _position = vector;

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
            if (SCoreUtils.IsBlocked(_context))
            {
                var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
                if (entityAlive != null && entityAlive.IsAlive())
                {
                    _context.Self.RotateTo(entityAlive, 90f,90f);
                }

                Stop(_context);
                return;
            }

            CheckProximityToPosition(_context);
        }

        private void CheckProximityToPosition(Context context)
        {
            context.Self.SetLookPosition(Vector3.zero);
            if (context.ActionData.Target is Vector3)
            {
                if (context.Self.getNavigator().noPathAndNotPlanningOne())
                    Stop(context);

                var distance = Vector3.Distance(context.Self.position, _position);
                if (distance < 2f)
                {
                    Stop(context);
                    return;
                }
            }

            var entityAlive = UAIUtils.ConvertToEntityAlive(context.ActionData.Target);
            if (entityAlive == null) return;
            if (entityAlive.IsDead())
            {
                Stop(context);
                return;
            }

            // The entity hasn't moved very much, keep going.
            if (Vector3.Distance(entityAlive.position, _position) < 2f)
            {
                context.Self.SetLookPosition(_position);
                context.Self.RotateTo(entityAlive, 30f, 30f);
                context.Self.moveHelper.SetMoveTo(_position, true);
                return;
            }

            // We are close enough
            if (Vector3.Distance(context.Self.position, entityAlive.position) < 1.2)
            {
                Stop(context);
                return;
            }

            _position = entityAlive.position;
            SCoreUtils.FindPath(context, _position, run);
            // 
            // SCoreUtils.FindPath(_context, _position, run);
        }
    }
}