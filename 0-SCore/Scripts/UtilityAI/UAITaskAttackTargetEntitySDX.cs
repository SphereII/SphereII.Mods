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
            this.attackTimeout = _context.Self.GetAttackTimeoutTicks();

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _context.Self.SetLookPosition( entityAlive.getHeadPosition());
                var targetPosition = entityAlive.getHeadPosition();
                _context.Self.RotateTo(targetPosition.x, targetPosition.y, targetPosition.y, 30f, 30f);
                _context.Self.SleeperSupressLivingSounds = false;
                _context.Self.SetAttackTarget(entityAlive, 1200);
            }

            if (_context.ActionData.Target is Vector3 vector)
            {
                // Center the vector so its looking directly at the middle.
                vector = EntityUtilities.CenterPosition(vector);
                _context.Self.IsBreakingBlocks = true;
                _context.Self.SetLookPosition(vector );
                _context.Self.RotateTo(vector.x, vector.y, vector.z, 45f, 45f);
                EntityUtilities.Stop(_context.Self.entityId);
            }

            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }

        public override void Stop(Context _context)
        {
            _context.Self.IsBreakingBlocks = false;
            _context.Self.IsBreakingDoors = false;
            base.Stop(_context);
        }

        public override void Update(Context _context)
        {

            if (!_context.Self.onGround || _context.Self.Climbing || _context.Self.Jumping)
                return;

            var position = Vector3.zero;
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                // Am I the target? Check if I have an attack or revenge target
                if (entityAlive.entityId == _context.Self.entityId)
                {
                    //  Am I being attacked? 
                    var attacker = EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) as EntityAlive;
                    if (attacker != null)
                    {
                        entityAlive = attacker;
                    }
                }

                if (entityAlive.IsDead())
                {
                    Stop(_context);
                    return;
                }

                _context.Self.SetLookPosition(entityAlive.getHeadPosition());
                position = entityAlive.getBellyPosition();

                _context.Self.RotateTo(position.x, position.y, position.z, 45f,30f);
                if (!_context.Self.IsInViewCone(position))
                {
                    attackTimeout = 5;
                    return;
                }

                // The default IsInFrontOfMe() uses the max view angle, which may be too wide for the desired effect here.
                // We want to limit the angle in which the entity is facing, before firing. 
                var headPosition = _context.Self.getHeadPosition();
                var dir = position - headPosition;
                var forwardVector = _context.Self.GetForwardVector();
                var angleBetween = Utils.GetAngleBetween(dir, forwardVector);
                var num2 = 70 * 0.5f;
                var isInFront = (angleBetween >= -num2 && angleBetween <= num2);
                if (!isInFront)
                {
                    attackTimeout = 15; // Introduce a small delay before it attacks, even if everything is lined up.
                    return;
                }

                // if ( ! _context.Self.IsInFrontOfMe(entityAlive.getHeadPosition()))
                // {
                //     // Reset the attack time out if the angle isn't right, to give them a pause before shooting.
                //     attackTimeout = 5;
                //     return;
                // }
                //
           
                // They may be in our viewcone, but the view cone may be huge, so let's check our angles.
                // var headPosition = _context.Self.getHeadPosition();
                // var dir = position - headPosition;
                // var forwardVector = _context.Self.GetForwardVector();
                // var angleBetween = Utils.GetAngleBetween(forwardVector, dir);
                // if (angleBetween is > 30 or < -30)
                // {
                //     Debug.Log($"Not Attacking: Angle: {angleBetween}");
                //     // Reset the attack time out if the angle isn't right, to give them a pause before shooting.
                //     attackTimeout = 10;
                //     return;
                // }
            }

            if (_context.ActionData.Target is Vector3 vector)
            {
                position = vector;
                _context.Self.SetLookPosition(position);
                var targetType = GameManager.Instance.World.GetBlock(new Vector3i(position));
                if (targetType.Equals(BlockValue.Air))
                {
                    this.Stop(_context);
                    return;
                }
            }

            // Reloading
            if (_context.Self.Buffs.HasBuff(_buffThrottle))
            {
              //  Debug.Log($"Not Attacking:  Buff Throttle: {_buffThrottle}");
                return;
            }

            //var num = UAIUtils.DistanceSqr(_context.Self.position, position);
            var entityAliveSdx = _context.Self as EntityAliveSDX;
    
            // Check the range on the item action
            ItemActionRanged.ItemActionDataRanged itemActionData = null;
            var itemAction = _context.Self.inventory.holdingItem.Actions[_actionIndex];
            //var distance = ((itemAction != null) ? Utils.FastMax(0.8f, itemAction.Range) : 1.095f);
            if (itemAction is ItemActionRanged itemActionRanged)
            {
                itemActionData = _context.Self.inventory.holdingItemData.actionData[_actionIndex] as ItemActionRanged.ItemActionDataRanged;
                if (itemActionData != null)
                {
                    //var range = itemActionRanged.GetRange(itemActionData);
                    //distance = Utils.FastMax(0.8f, range);
                    
                    // Check if we are already running.
                    if (itemAction.IsActionRunning(itemActionData))
                        return;
                }
            }
            //var minDistance = distance * distance;
            //var a = position - _context.Self.position;

            //not within range ?
            // if (a.sqrMagnitude > minDistance)
            // {
            //     // If we are out of range, it's probably a very small amount, so this will step forward, but not if we are staying.
            //     if (EntityUtilities.GetCurrentOrder(_context.Self.entityId) != EntityUtilities.Orders.Stay)
            //         _context.Self.moveHelper.SetMoveTo(position, true);
            //     Stop(_context);
            // }

            attackTimeout--;
            if (attackTimeout > 0)
                return;

    
            this.attackTimeout = _context.Self.GetAttackTimeoutTicks();

            // Action Index = 1 is Use, 0 is Attack.
            switch (_actionIndex)
            {
                case 0:
                    if (!_context.Self.Attack(false)) return;
                    _context.Self.Attack(true);
                    break;
                case 1:

                    // use, much like attack, goes through a few additional checks that can return false, including making sure that the 
                    // entity can attack / use. Conditions like if they are stunned, electrocuted, or its already running, will return false.
                    // Normally the Use fails briefly, but we likely don't want to trigger the event needlessly, just to cancel it.
                    if (!_context.Self.Use(false))
                    {
                        //_context.Self.emodel.avatarController.CancelEvent("WeaponFire");
                        return;
                    }

                    // Let's check to make sure it's a ranged action. 
                    if (itemActionData != null)
                    {
                        _context.Self.emodel.avatarController.TriggerEvent("WeaponFire");
                    }

                    _context.Self.Use(true);
                    break;
                default:
                    if (entityAliveSdx)
                    {
                        if (!entityAliveSdx.ExecuteAction(false, _actionIndex)) return;
                        entityAliveSdx.ExecuteAction(true, _actionIndex);
                    }
                    break;
            }

           // Stop(_context);
        }
    }
}