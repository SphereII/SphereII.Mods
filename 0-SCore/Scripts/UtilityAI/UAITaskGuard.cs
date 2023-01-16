// using this namespace is necessary for Utility AI Tasks
//       <task class="Guard, SCore" />
// The game adds UAI.UAITask to the class name for discovery.
namespace UAI
{
    /// <summary>
    /// Guards a position. If the entity is not at its guard position, it will path its way back to it.
    /// The guard position must already be set, for example from <see cref="MinEventActionGuardHere"/>.
    ///
    /// <example>
    /// <code>
    /// &lt;task class="Guard, SCore" /&gt;
    /// </code>
    /// </example>
    /// </summary>
    public class UAITaskGuard : UAITaskBase
    {
        private float _timeOut = 100f;
        private float _currentTimeout;

        public override void Start(Context _context)
        {
            base.Start(_context);
            _currentTimeout = _timeOut;

            // Clear out any pathing on the target that was set by other tasks.
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (entityAlive is IEntityOrderReceiverSDX)
            {
                entityAlive.moveHelper.Stop();
                entityAlive.navigator.clearPath();
            }
        }

        public override void Update(Context _context)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target);
            if (!(entityAlive is IEntityOrderReceiverSDX entityOrderReceiver))
            {
                Stop(_context);
                return;
            }

            base.Update(_context);

            // Don't do anything until the entity touches the ground; avoid the free in mid-air scenario.
            if (!entityAlive.onGround) return;

            // If we don't have a path, then either we started this task away from the guard
            // position and need to find our way back, or we are already there.
            if (entityAlive.navigator.noPathAndNotPlanningOne())
            {
                // Compare the block centers of the current position and guard position to avoid
                // repathing for tiny position changes.
                var entityPosition = EntityUtilities.CenterPosition(entityOrderReceiver.Position);
                var guardPosition = EntityUtilities.CenterPosition(entityOrderReceiver.GuardPosition);
                if (guardPosition != entityPosition)
                {
                    // Even if NPCs can't break blocks, setting this to false results in weird pathing.
                    var canBreakBlocks = true;

                    entityAlive.moveHelper.SetMoveTo(entityOrderReceiver.GuardPosition, canBreakBlocks);

                    entityAlive.FindPath(
                        entityOrderReceiver.GuardPosition,
                        entityAlive.GetMoveSpeedPanic(),
                        canBreakBlocks,
                        null);

                    return;
                }
                else
                {
                    // Turn around and face the same way as when the order was given.

                    entityAlive.SetLookPosition(entityOrderReceiver.GuardLookPosition);

                    entityAlive.RotateTo(
                        entityOrderReceiver.GuardLookPosition.x,
                        entityOrderReceiver.GuardLookPosition.y,
                        entityOrderReceiver.GuardLookPosition.z,
                        30f,
                        30f);
                }
            }
          
            // Check if a player is in your bounds, and face them if they are.
            var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId) as EntityAlive;
            SCoreUtils.TurnToFaceEntity(_context, leader);

            _currentTimeout--;
            if (_currentTimeout < 0) Stop(_context);
        }
    }
}