using System.Globalization;

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
                _context.Self.FindPath(RandomPositionGenerator.CalcAway(_context.Self, 0, (int)this.maxFleeDistance, (int)this.maxFleeDistance, entityAlive.position), _context.Self.GetMoveSpeed(), false, null);
                return;
            }
            _context.ActionData.Failed = true;
        }
        public override void Update(Context _context)
        {
            SCoreUtils.SetCrouching(_context);
            base.Update(_context);
            if (entityAlive)
            {
                _context.Self.RotateTo(entityAlive, 30f, 30);
                _context.Self.SetLookPosition(entityAlive.position);
            }

            if (!_context.Self.getNavigator().noPathAndNotPlanningOne()) return;
            this.Stop(_context);

        }
    }
}