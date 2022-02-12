using System;
using System.Collections;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToHealTarget, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToHealTargetSDX : UAITaskMoveToTarget
    {
        private Vector3 _position;
        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (distance == 0)
                distance = 3;

        }

        public override void Update(Context _context)
        {
            base.Update(_context);
            SCoreUtils.CheckForClosedDoor(_context);

            var dist = Vector3.Distance(_position, _context.Self.position);
            if (dist > distance) return;

           // SCoreUtils.SetLookPosition(_context, _position);
            EntityUtilities.Stop(_context.Self.entityId);
            Stop(_context);
        }

  
        public override void Start(Context _context)
        {
            SCoreUtils.SetCrouching(_context);

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
                _position = entityAlive.position;

            if (_context.ActionData.Target is Vector3 vector)
                _position = vector;

            SCoreUtils.FindPath(_context, _position, run);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }

    }
}