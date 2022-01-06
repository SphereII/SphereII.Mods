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
            SCoreUtils.CheckForClosedDoor(_context);
            base.Update(_context);
          
            if ( SCoreUtils.IsBlocked(_context))
                this.Stop(_context);

        }

        public override void Start(Context _context)
        {
            var isHidden = false;
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _position = entityAlive.position;
                isHidden = entityAlive.IsCrouching;
                                
                var sqrMagnitude2 = (_position - _context.Self.position).sqrMagnitude;
                if (sqrMagnitude2 > 90f)
                    _position = RandomPositionGenerator.CalcTowards(_context.Self, 2, 2, 2, _position);

                if (sqrMagnitude2 > 150f)
                    _position = RandomPositionGenerator.CalcTowards(_context.Self, 5, 5, 5, _position);

            }

            if (_context.ActionData.Target is Vector3 vector)
                _position = vector;

            // If we already know we have a place, let's go there.
            if (_context.Self.HasInvestigatePosition)
            {
                var sqrMagnitude = (_context.Self.InvestigatePosition - _context.Self.position).sqrMagnitude;
                if (sqrMagnitude > 2f)
                    _position = _context.Self.InvestigatePosition;
            }

            SCoreUtils.SetLookPosition(_context, _position);

            var speed = _context.Self.GetMoveSpeed();
            if (run)
                speed = _context.Self.GetMoveSpeedPanic();

            _context.Self.FindPath(_position, speed, true, null);


            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
            
        }
    }
}