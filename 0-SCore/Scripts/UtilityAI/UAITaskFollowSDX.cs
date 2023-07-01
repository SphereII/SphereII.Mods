using System.ComponentModel.Design.Serialization;
using GamePath;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="FollowSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskFollowSDX : UAITaskBase
    {
        private EntityAlive _leader;
        private float _distance = 2f;
        private int _maxDistance = 25;
        private Vector3 _position;

        private float _timeOut = 100f;
        private float _currentTimeout;
        protected override void initializeParameters()
        {
            if (Parameters.ContainsKey("stop_distance")) _distance = StringParsers.ParseFloat(Parameters["stop_distance"]);
            if (Parameters.ContainsKey("max_distance")) _maxDistance = (int)StringParsers.ParseFloat(Parameters["max_distance"]);
            if (Parameters.ContainsKey("teleportTime")) _timeOut = StringParsers.ParseFloat(Parameters["teleportTime"]);
        }

        public override void Start(Context context)
        {
            base.Start(context);
            _currentTimeout = _timeOut;

            _leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
            if (_leader == null) // No leader? You shouldn't be following.
            {
                Stop(context);
                return;
            }

            SCoreUtils.SetCrouching(context, _leader.IsCrouching);
            EntityUtilities.ClearAttackTargets(context.Self.entityId);
                
            // Sets up the original position of the leader.
            _position = _leader.position;

         //   SCoreUtils.SetLookPosition(_context, _leader);
            SCoreUtils.FindPath(context, _position, true);
        }


        public override void Update(Context _context)
        {
            base.Update(_context);
            SCoreUtils.SetSpeed(_context, true);
            CheckProximityToLeader(_context);

            // Are we blocked? Can we see our leader? If not, start counting down. This should slow down aggressive teleports to the leader, while
            // also helping keep the NPC close to the leader.
            if (SCoreUtils.IsBlocked(_context))
            {
                _currentTimeout--;
                if (_currentTimeout > 0) return;

                SCoreUtils.TeleportToLeader(_context);

                Stop(_context);
                return;
            }

            // Reset the timeout.
            _currentTimeout = _timeOut;
        }

        // Contains logic to determine if the NPC should be move towards its leader, etc.
        private void CheckProximityToLeader(Context context)
        {
            // If we lost our leader, check to see if we have one. If we don't, end the task.
            if (_leader == null)
            {
                _leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
                if ( _leader == null )
                {
                    Stop(context);
                   return;
                }
            }
            SCoreUtils.SetCrouching(context, _leader.IsCrouching);

            var distanceToLeader = Vector3.Distance(context.Self.position, _leader.position);

            // If we are close to the leader, stop.
            if (distanceToLeader > _distance && distanceToLeader < _distance * 2)
            {
                context.Self.RotateTo(_leader.position.x, _leader.position.y, _leader.position.z, 8f,8f);
                Stop(context);
                return;
            }

            // If they are too far away, then teleport.
            if (distanceToLeader > _maxDistance)
            {
                SCoreUtils.TeleportToLeader(context, false);
                Stop(context);
            }
            // If we have a path, check to see if the player has moved.
            if (!context.Self.navigator.noPathAndNotPlanningOne())
            {
                // If the leader hasn't moved much, don't repath.
                var dist = Vector3.Distance(_leader.position, _position);
                if (dist < _distance * 2)
                    return;
            }

            // If the leader has moved quite a bit, re-position.
            _position = _leader.position;
            SCoreUtils.FindPath(context, _position, true);

        }

    }
}