using GamePath;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToAttackTargetSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToAttackTargetSDX : UAITaskMoveToTargetSDX
    {
        private int _actionIndex = 0;
        private int _targetTimeout = 20;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("action_index")) _actionIndex = int.Parse(Parameters["action_index"]);
            if (Parameters.ContainsKey("target_timeout")) _targetTimeout = int.Parse(Parameters["target_timeout"]);

        }
        public override void Stop(Context _context)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive == null) return;
            
            // Check the range on the item action
            var itemAction = _context.Self.inventory.holdingItem.Actions[_actionIndex];
            var distance = ((itemAction != null) ? Utils.FastMax(0.8f, itemAction.Range) : 1.095f);
            var minDistance = distance * distance;
            var a = entityAlive.position - _context.Self.position;
            // if within range, attack!
            if (a.sqrMagnitude <= minDistance)
            {
                if (!_context.Self.onGround || _context.Self.Climbing || _context.Self.Jumping)
                {
                    _context.Self.SetAttackTarget(entityAlive, _targetTimeout);
                    base.Stop(_context);
                    return;
                }
                
                // Add a check to see if we are facing or can see the target.
                var isInViewCone = _context.Self.IsInViewCone(entityAlive.position);
                var isInFrontOf = _context.Self.IsInFrontOfMe(entityAlive.getHeadPosition());
                if ( isInViewCone || isInFrontOf)
                    _context.Self.Attack(true);
            }

            _context.Self.SetAttackTarget(entityAlive, _targetTimeout);
            base.Stop(_context);
        }
    }
}