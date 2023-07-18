// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToInvestigate, SCore" />
// The game adds UAI.UAITask to the class name for discover.
using UnityEngine;

namespace UAI
{
    public class UAITaskMoveToInvestigate : UAITaskMoveToTarget
    {
        private Vector3 _position;
        public override void Update(Context _context)
        {
            if (SCoreUtils.IsBlocked(_context))
            {
                // We're giving up
                _context.Self.PlayGiveUpSound();
                this.Stop(_context);
            }

            base.Update(_context);
        }

        public override void Stop(Context _context)
        {
            // Clear the investigation position, which may be causing entities to run away randomly.
            //_context.Self.SetInvestigatePosition(Vector3.zero, 0);
            base.Stop(_context);
        }

        // Set up random positions to go investigate, if we are tracing an entity.
        public void  SetRandomPosition(Context _context)
        {
            // If we have the exact vector, go there.
            if (_context.ActionData.Target is Vector3 vector)
            {
                _position = vector;
                return;
            }

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
                _position = entityAlive.position;

            EntityPlayer entityPlayer = entityAlive as EntityPlayer;
            if (entityPlayer)
                _position = entityPlayer.GetBreadcrumbPos(3 * _context.Self.rand.RandomFloat);

            // Pick a random range to give a variety of locations
            var num = _context.Self.rand.RandomRange(0, 10);
            if (num < 7)
                if (_context.Self.HasInvestigatePosition)
                    _position = _context.Self.InvestigatePosition;
            if (num < 5)
                _position = RandomPositionGenerator.CalcTowards(_context.Self, 5, 5, 5, _position);

            if (num < 2)
                _position = RandomPositionGenerator.CalcTowards(_context.Self, 2, 2, 2, _position);

        }
        public override void Start(Context _context)
        {
            SetRandomPosition(_context);

            var speed = _context.Self.GetMoveSpeed();
            if (run)
                speed = _context.Self.GetMoveSpeedPanic();

            _context.Self.SetLookPosition(_position);
            
            SCoreUtils.FindPath(_context, _position, run);

            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;

            // If we are investigating a new enemy, play the "sense" sound.
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) != entityAlive
                && EntityTargetingUtilities.IsEnemy(_context.Self, entityAlive))
            {
                _context.Self.PlayOneShot(_context.Self.soundSense);
            }
        }
    }
}