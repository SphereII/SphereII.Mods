// using this namespace is necessary for Utilities AI Tasks
//       <task class="Patrol, SCore" />
// The game adds UAI.UAITask to the class name for discover.

using GamePath;
using UnityEngine;

namespace UAI
{
    public class UAITaskPatrol : UAITaskBase
    {
        private float _timeOut = 100f;
        private float _currentTimeout;
        private Vector3 position;
        private string _buff = "";
        private bool hadBuff = true;
        private bool _run = false; // "run" parameter: panic speed between patrol points instead of a walk.

        public override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];
            if (Parameters.TryGetValue("run", out var run)) _run = StringParsers.ParseBool(run);
        }
        public override void Start(Context _context)
        {
            // Grab or index the new position.
            position = EntityUtilities.GetNewPositon(_context.Self.entityId);
            if ( position == Vector3.zero)
            {
                Stop(_context);
                return;
            }

            if (SphereCache.LastBlock.ContainsKey(_context.Self.entityId))
            {
                    var supportBlock = GameManager.Instance.World.FindSupportingBlockPos(position);
                    BlockUtilitiesSDX.removeParticles(new Vector3i(supportBlock));

                SphereCache.LastBlock[_context.Self.entityId] = position;
            }
            else
            {
                // Store the LastBlock position here, so we know what we can remove next time.
                SphereCache.LastBlock.Add(_context.Self.entityId, position);
            }

            PathFinderThread.Instance.FindPath(_context.Self, position, _context.Self.GetMoveSpeed(), true, null);
            base.Start(_context);
            hadBuff = false;
            _currentTimeout = _timeOut;
        }
        public override void Update(Context _context)
        {
            // Don't do anything until the entity touches the ground; avoid the free in mid-air scenario.
            if (!_context.Self.onGround) return;

            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff))
            {
                EntityUtilities.Stop(_context.Self.entityId);
                return;
            }
            if ( hadBuff )
            {
                Stop(_context);
                return;
            }
            _currentTimeout--;
            if (_currentTimeout < 0)
            {
                _context.Self.Buffs.RemoveBuff(_buff);
                Stop(_context);
            }

            base.Update(_context);
            // Pass the speed: the two-argument overload uses the aggro speed, so patrols jogged.
            var speed = _run ? _context.Self.GetMoveSpeedPanic() : _context.Self.GetMoveSpeed();
            _context.Self.moveHelper.SetMoveTo(position, true, speed);

            var dist = Vector3.Distance(position, _context.Self.position);
            if (dist < 1f)
            {
                _context.Self.Buffs.AddBuff(_buff);
                hadBuff = true;
                return;
            }

        }

        public override void Stop(Context _context)
        {
            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff)) return;

            SphereCache.RemovePath(_context.Self.entityId, position);
            base.Stop(_context);
        }
    }
}