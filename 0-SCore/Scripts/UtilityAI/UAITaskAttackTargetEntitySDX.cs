using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="AttackTargetEntitySDX, SCore" action_index="0" /> 
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskAttackTargetEntitySDX : UAITaskAttackTargetEntity
    {
        // Default to action 0.
        private int _actionIndex = 0;
        private string _buffThrottle = "buffReload2";

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("action_index")) _actionIndex = int.Parse(Parameters["action_index"]);
            if (Parameters.ContainsKey("buff_throttle")) _buffThrottle = Parameters["buff_throttle"];
        }

        public override void Start(Context _context)
        {
            base.Start(_context);
        }

        public override void Update(Context _context)
        {
            if (!_context.Self.onGround || _context.Self.Climbing)
                return;

            // if the NPC is on the ground, don't attack.
            //switch (_context.Self.bodyDamage.CurrentStun)
            //{
            //    case EnumEntityStunType.Getup:
            //    case EnumEntityStunType.Kneel:
            //    case EnumEntityStunType.Prone:
            //        return;
            //}

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                // Am I the target?
                if (entityAlive.entityId == _context.Self.entityId)
                {
                    // Am I being attacked? 
                    var attacker = EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) as EntityAlive;
                    if (attacker == null)
                    {
                        Stop(_context);
                        return;
                    }

                    entityAlive = attacker;
                }
                if (entityAlive.IsDead())
                {
                    Stop(_context);
                    return;
                }

                if ( entityAlive.IsWalkTypeACrawl())
                    SCoreUtils.SetCrouching(_context, entityAlive.IsWalkTypeACrawl());

                if ( entityAlive.height < 1.1f)
                    SCoreUtils.SetCrouching(_context, true);

                _context.Self.RotateTo(entityAlive, 30f, 30f);
                _context.Self.SetLookPosition(entityAlive.getHeadPosition());
            }

            if (_context.ActionData.Target is Vector3 vector)
            {
                _context.Self.RotateTo(vector.x, vector.y, vector.z, 30f, 30);
                _context.Self.SetLookPosition(vector);
            }

            // Reloading
            if (_context.Self.Buffs.HasBuff(_buffThrottle))
                return;

            EntityUtilities.Stop(_context.Self.entityId);

            // Check the range on the item action
            var itemAction = _context.Self.inventory.holdingItem.Actions[_actionIndex];
            var distance = ((itemAction != null) ? Utils.FastMax(0.8f, itemAction.Range - 0.35f) : 1.095f);
            if (itemAction is ItemActionRanged itemActionRanged)
            {
                var itemActionData = _context.Self.inventory.holdingItemData.actionData[_actionIndex] as ItemActionRanged.ItemActionDataRanged;
                if (itemActionData != null)
                {
                    var range = itemActionRanged.GetRange(itemActionData);
                    distance = Utils.FastMax(0.8f, range - 0.35f);
                }
            }
            var minDistance = distance * distance;
            var a = entityAlive.position - _context.Self.position;

            // not within range?
            if (a.sqrMagnitude > minDistance)
            {
                Stop(_context);
                return;
            }


            // Action Index = 1 is Use, 0 is Attack.
            if (_actionIndex > 0)
            {
                if (!_context.Self.Use(false)) return;
                _context.Self.Use(true);
            }
            else
            {
                if (!_context.Self.Attack(false)) return;
                _context.Self.Attack(true);
            }

            // Reset the attackTimeout, and allow another task to run.
            // this.attackTimeout = _context.Self.GetAttackTimeoutTicks();
            Stop(_context);
        }
    }
}