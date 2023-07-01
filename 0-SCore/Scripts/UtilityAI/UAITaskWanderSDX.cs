using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="WanderSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskWanderSDX : UAITaskWander
    {
        private string _interest;
        private Vector3 _position;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (this.Parameters.ContainsKey("interest"))
            {
                _interest = Parameters["interest"];
            }
        }

        public override void Stop(Context _context)
        {
            _context.Self.getNavigator().clearPath();
            _context.Self.moveHelper.CanBreakBlocks = true;
            base.Stop(_context);
        }

        public override void Start(Context _context)
        {
            SCoreUtils.SetCrouching(_context);

            _context.Self.moveHelper.CanBreakBlocks = false;

            // Start the action here, since we are just over-riding start, and not calling the base, as the base is calculating a hard coded 10 block path.
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;

            // If max wander distance isn't set, set it now for 10.
            if (maxWanderDistance == 0)
                maxWanderDistance = 20;

            // The y is lower than max wander, since they tend to try to climb up steep hills.
            _position = RandomPositionGenerator.Calc(_context.Self, (int) maxWanderDistance, 10);
            _position.y = (int) GameManager.Instance.World.GetHeightAt(_position.x, _position.z) + 1;

            // If interests points have been specified, random roll to see if the npc will path towards them or not.
            if (!string.IsNullOrEmpty(_interest))
            {
                var rand = _context.Self.rand.RandomRange(0.0f, 1.0f);
                if (rand < 0.3)
                {
                    var paths = SCoreUtils.ScanForTileEntities(_context, _interest);
                    if (paths.Count > 0)
                    {
                        var index = _context.Self.rand.RandomRange(0, paths.Count);
                        _position = paths[index];
                    }
                }
            }

            SCoreUtils.FindPath(_context, _position, false);
        }


        public override void Update(Context _context)
        {
            if (SCoreUtils.IsBlocked(_context))
                this.Stop(_context);

            _context.Self.SetLookPosition(_position);

            base.Update(_context);
            var distance = Vector3.Distance(_context.Self.position, _position);
            if (distance < 0.5f)
            {
                Stop(_context);
            }
        }
    }
}