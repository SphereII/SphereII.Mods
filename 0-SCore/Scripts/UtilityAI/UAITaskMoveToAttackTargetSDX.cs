﻿using GamePath;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToAttackTargetSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToAttackTargetSDX : UAITaskMoveToTargetSDX
    {
        private int _actionIndex = 0;
        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("action_index")) _actionIndex = int.Parse(Parameters["action_index"]);
        }
        public override void Stop(Context _context)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                // Check the range on the item action
                var itemAction = _context.Self.inventory.holdingItem.Actions[_actionIndex];
                var distance = ((itemAction != null) ? Utils.FastMax(0.8f, itemAction.Range - 0.35f) : 1.095f);
                var minDistance = distance * distance;
                var a = entityAlive.position - _context.Self.position;
                // if within range, attack!
                if (a.sqrMagnitude <= minDistance)
                    _context.Self.Attack(true);
                base.Stop(_context);
            }
        }
    }
}