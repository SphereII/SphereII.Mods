using System.Globalization;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="BackupFromTargetSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskBackupFromTargetSDX : UAITaskBase
    {
        private EntityAlive entityAlive;
        private float maxFleeDistance;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (this.Parameters.ContainsKey("max_distance"))
            {
                this.maxFleeDistance = StringParsers.ParseFloat(this.Parameters["max_distance"], 0, -1, NumberStyles.Any);
            }
        }

        public override void Start(Context _context)
        {
            base.Start(_context);
            entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive != null)
            {
                _context.Self.detachHome();
                var position = RandomPositionGenerator.CalcAway(_context.Self, 2, (int)this.maxFleeDistance, (int)this.maxFleeDistance, entityAlive.position);

                // If the flee distance is small, we are just backing away from the entity in a friendly way
                if ( maxFleeDistance < 5)
                {
                    Vector3 dist = entityAlive.position - _context.Self.position;
                    if (Vector3.Distance(entityAlive.position, _context.Self.position) < 3)
                    {
                        // Calculate a position behind the npc, and move there.
                        position = dist.normalized * -3f;
                        position += _context.Self.position;
                        _context.Self.moveHelper.SetMoveTo(position, true);
                        return;
                    }
                }

                
                SCoreUtils.FindPath(_context, position, true);
                return;
            }
            _context.ActionData.Failed = true;
        }
        public override void Update(Context _context)
        {
            SCoreUtils.SetCrouching(_context);
            base.Update(_context);
            if (!_context.Self.getNavigator().noPathAndNotPlanningOne()) return;

            if (entityAlive)
            {
                _context.Self.SetLookPosition(entityAlive.getHeadPosition());
                _context.Self.RotateTo(entityAlive, 30f,30);
                //SCoreUtils.SetLookPosition(_context, entityAlive);
            }

            this.Stop(_context);

        }
    }
}