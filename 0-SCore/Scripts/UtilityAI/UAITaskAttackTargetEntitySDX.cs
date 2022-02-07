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
        private int _targetTimeout = 20;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("action_index")) _actionIndex = int.Parse(Parameters["action_index"]);
            if (Parameters.ContainsKey("buff_throttle")) _buffThrottle = Parameters["buff_throttle"];
            if (Parameters.ContainsKey("target_timeout")) _targetTimeout = int.Parse(Parameters["target_timeout"]);

        }

        public override void Start(Context _context)
        {
            // Reset crouching.
            SCoreUtils.SetCrouching(_context);
            this.attackTimeout = 0;

            EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _context.Self.SetLookPosition(_context.Self.CanSee(entityAlive) ? entityAlive.getHeadPosition() : Vector3.zero);
                if (_context.Self.bodyDamage.HasLimbs)
                {
                    _context.Self.RotateTo(entityAlive.position.x, entityAlive.position.y, entityAlive.position.z, 30f, 30f);
                }
                this.attackTimeout = _context.Self.GetAttackTimeoutTicks();
            }

            if (_context.ActionData.Target.GetType() == typeof(Vector3))
            {
                this.attackTimeout = _context.Self.GetAttackTimeoutTicks();
                Vector3 vector = (Vector3)_context.ActionData.Target;
                _context.Self.IsBreakingBlocks = true;
                BlockUtilitiesSDX.addParticles("", new Vector3i(vector));
                var block = GameManager.Instance.World.GetBlock(new Vector3i(vector));
                _context.Self.SetLookPosition(vector );
                if (_context.Self.bodyDamage.HasLimbs)
                {
                    _context.Self.RotateTo(vector.x, vector.y, vector.z, 30f, 30f);
                }
            }

            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }

        public override void Stop(Context _context)
        {
            if (_context.ActionData.Target is Vector3 vector)
            {
                _context.Self.IsBreakingBlocks = false;
                BlockUtilitiesSDX.removeParticles(new Vector3i(vector));
            }
            base.Stop(_context);
        }
        public override void Update(Context _context)
        {

            if (!_context.Self.onGround || _context.Self.Climbing)
                return;

            Vector3 position = Vector3.zero;
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
                    SCoreUtils.SetLookPosition(_context, Vector3.forward);
                    Stop(_context);
                    return;
                }
                position = entityAlive.position;
            }

            if (_context.ActionData.Target is Vector3 vector)
            {
                position = vector;
                _context.Self.SetLookPosition( position );
                var targetType = GameManager.Instance.World.GetBlock(new Vector3i(position));
                if (targetType.Equals(BlockValue.Air))
                {
                    _context.Self.SetLookPosition(Vector3.zero);

                    this.Stop(_context);
                    return;
                }
            }

            // Reloading
            if (_context.Self.Buffs.HasBuff(_buffThrottle))
                return;


            attackTimeout--;
            if (attackTimeout > 0)
                return;

            // Check the range on the item action
            ItemActionRanged.ItemActionDataRanged itemActionData = null;
            var itemAction = _context.Self.inventory.holdingItem.Actions[_actionIndex];
            var distance = ((itemAction != null) ? Utils.FastMax(0.8f, itemAction.Range) : 1.095f);
            if (itemAction is ItemActionRanged itemActionRanged)
            {
                itemActionData = _context.Self.inventory.holdingItemData.actionData[_actionIndex] as ItemActionRanged.ItemActionDataRanged;
                if (itemActionData != null)
                {
                    var range = itemActionRanged.GetRange(itemActionData);
                    //distance = Utils.FastMax(0.8f, range - 0.35f);
                    distance = Utils.FastMax(0.8f, range);

                }
            }
            var minDistance = distance * distance;
            var a = position - _context.Self.position;

            // not within range? 
            if (a.sqrMagnitude > minDistance)
            {
                // If we are out of range, it's probably a very small amount, so this will step forward, but not if we are staying.
                if (EntityUtilities.GetCurrentOrder(_context.Self.entityId) != EntityUtilities.Orders.Stay)
                    _context.Self.moveHelper.SetMoveTo(position, true);
            }

            if (a.sqrMagnitude < 0.5)
                _context.Self.moveHelper.SetMoveTo(position + Vector3.back, true);

            if (_context.Self.bodyDamage.HasLimbs)
                _context.Self.RotateTo(position.x, position.y, position.z, 30f, 30f);

            // Action Index = 1 is Use, 0 is Attack.
            switch (_actionIndex)
            {
                case 0:
                    if (!_context.Self.Attack(false)) return;
                    _context.Self.Attack(true);
                    break;
                case 1:
                    if (!_context.Self.Use(false)) return;
                    _context.Self.Use(true);
                    break;
                default:
                    var entityAliveSDX = _context.Self as EntityAliveSDX;
                    if (entityAliveSDX)
                    {
                        if (!entityAliveSDX.ExecuteAction(false, _actionIndex)) return;
                        entityAliveSDX.ExecuteAction(true, _actionIndex);
                    }
                    break;

            }

            if (entityAlive != null)
                _context.Self.SetAttackTarget(entityAlive, _targetTimeout);

            this.attackTimeout = _context.Self.GetAttackTimeoutTicks();

            // Reset the attackTimeout, and allow another task to run.
            //  Stop(_context);
        }
    }
}