﻿using UnityEngine;

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

        private float _timeOut = 200f;
        private float _currentTimeout;
        protected override void initializeParameters()
        {
            if (Parameters.ContainsKey("stop_distance")) _distance = StringParsers.ParseFloat(Parameters["stop_distance"]);
            if (Parameters.ContainsKey("max_distance")) _maxDistance = (int)StringParsers.ParseFloat(Parameters["max_distance"]);
            if (Parameters.ContainsKey("teleportTime")) _timeOut = StringParsers.ParseFloat(Parameters["teleportTime"]);

        }



        public override void Start(Context _context)
        {
            base.Start(_context);
            _currentTimeout = _timeOut;

            _leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId) as EntityAlive;
            if (_leader == null) // No leader? You shouldn't be following.
            {
                Stop(_context);
                return;
            }

            // Sets up the original position of the leader.
            _position = _leader.position;

            // If we are close enough, we don't need to path.
            CheckProximityToLeader(_context);
        }

        public override void Update(Context _context)
        {

            base.Update(_context);

            // Are we blocked? Can we see our leader? If not, start counting down. This should slow down aggressive teleports to the leader, while
            // also helping keep the NPC close to the leader.
            if (SCoreUtils.IsBlocked(_context))
            {
                if (!_context.Self.CanSee(_leader) && !_context.Self.CanEntityBeSeen(_leader))
                {
                    _currentTimeout--;
                    if (_currentTimeout > 0) return;

                    SCoreUtils.TeleportToLeader(_context);
                }

                Stop(_context);
                return;
            }


            // Reset the timeout.
            _currentTimeout = _timeOut;

            CheckProximityToLeader(_context);


        }

        // Contains logic to determine if the NPC should be move towards its leader, etc.
        public void CheckProximityToLeader(Context _context)
        {
            SCoreUtils.SetCrouching(_context, _leader.IsCrouching);

            // Are we too close?  // Handled on EntityAliveSDX.
            var distanceToLeader = Vector3.Distance(_context.Self.position, _leader.position);

            // If we are close to the leader, stop.
            if (distanceToLeader > _distance && distanceToLeader < _distance * 2)
            {
                _context.Self.SetLookPosition(_position);
                _context.Self.RotateTo(_leader, 45f, 45);
                Stop(_context);
                return;
            }

            // If they are too far away, then teleport.
            if (distanceToLeader > _maxDistance)
            {
                SCoreUtils.TeleportToLeader(_context);
                Stop(_context);
                return;
            }

            // If we have a path, check to see if the player has moved.
            if (!_context.Self.navigator.noPathAndNotPlanningOne())
            {
                // If the leader hasn't moved much, don't repath.
                var dist = Vector3.Distance(_leader.position, _position);
                if (dist < _distance * 2)
                    return;
            }

            // If the leader has moved quite a bit, re-position.
            _position = _leader.position;
            SCoreUtils.FindPath(_context, _position, true);

        }

    }
}