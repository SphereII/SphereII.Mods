// using this namespace is necessary for Utilities AI Tasks
//       <task class="MoveToInvestigate, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskMoveToInvestigate : UAITaskMoveToTarget
    {
        public override void Update(Context _context)
        {
            base.Update(_context);

            // Add in a blocked time condition.
            if (_context.Self.moveHelper.BlockedTime >= 0.3f)
                this.Stop(_context);

        }

        public override void Start(Context _context)
        {
            SCoreUtils.SetCrouching(_context);
            base.Start(_context);
            SCoreUtils.FindPath(_context, _context.Self.InvestigatePosition, true);
        }
    }
}