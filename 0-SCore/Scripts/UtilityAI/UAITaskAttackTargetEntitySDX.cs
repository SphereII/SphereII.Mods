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
                    SCoreUtils.SetLookPosition(_context, Vector3.forward);
                    Stop(_context);
                    return;
                }
            }

            if (_context.ActionData.Target is Vector3 vector)
                SCoreUtils.SetLookPosition(_context, vector);

            // Reloading
            if (_context.Self.Buffs.HasBuff(_buffThrottle))
                return;


            attackTimeout--;
            if (attackTimeout > 0)
                return;

            //EntityUtilities.Stop(_context.Self.entityId);
            ItemActionRanged.ItemActionDataRanged itemActionData = null;
            // Check the range on the item action
            var itemAction = _context.Self.inventory.holdingItem.Actions[_actionIndex];
            var distance = ((itemAction != null) ? Utils.FastMax(0.8f, itemAction.Range - 0.35f) : 1.095f);
            if (itemAction is ItemActionRanged itemActionRanged)
            {
                itemActionData = _context.Self.inventory.holdingItemData.actionData[_actionIndex] as ItemActionRanged.ItemActionDataRanged;
                if (itemActionData != null)
                {
                    //if (sphereTest)
                    //{
                    //    // Empty, no rounds left in the chamber
                    //    if (itemActionData.invData.itemValue.Meta == 0)
                    //    {
                    //        _context.Self.OnReloadStart();
                    //        itemActionData.isReloading = true;
                    //        //itemActionRanged.ReloadGun(itemActionData);
                    //        return;
                    //    }
                    //    // Are we reloading?
                    //    if (itemActionData.isReloading) return;

                    //    // Is an action running?
                    //    if (itemAction.IsActionRunning(itemActionData)) return;

                    //}
                    var range = itemActionRanged.GetRange(itemActionData);
                    distance = Utils.FastMax(0.8f, range - 0.35f);
                }
            }
            var minDistance = distance * distance;
            var a = entityAlive.position - _context.Self.position;



            // not within range? qq
            if (a.sqrMagnitude > minDistance)
            {
                // If we are out of range, it's probably a very small amount, so this will step forward, but not if we are staying.
                if (EntityUtilities.GetCurrentOrder(_context.Self.entityId) != EntityUtilities.Orders.Stay)
                    _context.Self.moveHelper.SetMoveTo(entityAlive.position, true);
            }

            // Face the target right before hitting them.
            if (entityAlive != null)
                SCoreUtils.SetLookPosition(_context, entityAlive);

            // Action Index = 1 is Use, 0 is Attack.
            if (_actionIndex > 0)
            {
                if (!_context.Self.Use(false)) return;
                _context.Self.Use(true);
                _context.Self.SetAttackTarget(entityAlive, _targetTimeout);
//                if (itemActionData != null)
                    //itemActionData.invData.itemValue.Meta--;
            }
            else
            {
                if (!_context.Self.Attack(false)) return;
                _context.Self.Attack(true);
                _context.Self.SetAttackTarget(entityAlive, _targetTimeout);
            }

            this.attackTimeout = _context.Self.GetAttackTimeoutTicks();

            // Reset the attackTimeout, and allow another task to run.
            //  Stop(_context);
        }
    }
}