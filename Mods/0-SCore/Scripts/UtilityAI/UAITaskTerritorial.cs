using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="WanderSDX, SCore" interest="LandClaim, Loot, VendingMachine, Forge, Campfire, Workstation, PowerSource"/>
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskTerritorialSDX : UAITaskWander
    {
        private Vector3 _position;

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

            ChunkCoordinates homePosition = _context.Self.getHomePosition();
            Vector3 vector = RandomPositionGenerator.CalcTowards(_context.Self, 5, (int)maxWanderDistance, 7,
                homePosition.position.ToVector3());
            if (vector.Equals(Vector3.zero))
            {
                Stop(_context);
                return;
            }

            _position = vector;

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