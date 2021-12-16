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
            SCoreUtils.SetCrouching(_context);
        }

        public override void Update(Context _context)
        {
            // if the NPC is on the ground, don't attack.
            switch (_context.Self.bodyDamage.CurrentStun)
            {
                case EnumEntityStunType.Getup:
                case EnumEntityStunType.Kneel:
                case EnumEntityStunType.Prone:
                    return;
            }

            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                if (entityAlive.IsDead())
                {
                    Stop(_context);
                    return;
                }

                SCoreUtils.SetCrouching(_context, entityAlive.IsWalkTypeACrawl());
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
            this.attackTimeout = _context.Self.GetAttackTimeoutTicks();
            Stop(_context);
        }
    }
}