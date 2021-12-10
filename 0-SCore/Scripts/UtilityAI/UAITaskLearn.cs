// using this namespace is necessary for Utilities AI Tasks
//       <task class="Learn, SCore" />
// The game adds UAI.UAITask to the class name for discover.

namespace UAI
{
    public class UAITaskLearn : UAITaskBase
    {
        private string _property = "Unlocks";
        private int _actionIndex;

        protected override void initializeParameters()
        {
            if (Parameters.ContainsKey("property")) _property = Parameters["property"];
            if (Parameters.ContainsKey("action_index")) _actionIndex = int.Parse(Parameters["action_index"]);
        }
        public override void Start(Context _context)
        {
            base.Start(_context);
            var stack = EntityUtilities.GetItemStackByProperty(_context.Self.entityId, _property);
            if (!Equals(stack, ItemStack.Empty))
                SCoreUtils.SimulateActionInstantExecution(_context, _actionIndex, stack);
            Stop(_context);
        }

    }
}