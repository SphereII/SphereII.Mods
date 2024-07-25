namespace UAI
{
    /// <summary>
    /// Utility AI task to approach a spot, as determined by the investigate position.
    /// The name and functionality is based on <see cref="EAIApproachSpot"/>.
    /// </summary>
    public class UAITaskApproachSpotSDX : UAITaskBase
    {
        // Use the same feature class as EntityUtilities
        private static readonly string AdvFeatureClass = "AdvancedNPCFeatures";

        public override void Start(Context _context)
        {
            base.Start(_context);

            if (!_context.Self.HasInvestigatePosition)
            {
                Stop(_context);
                return;
            }

            if (_context.Self.IsSleeping)
            {
                ClearAndStop(_context);
                return;
            }

            SCoreUtils.SetCrouching(_context);

            _context.Self.moveHelper.CanBreakBlocks = false;

            UpdatePath(_context);
        }

        public override void Stop(Context _context)
        {
            _context.Self.getNavigator().clearPath();

            _context.Self.moveHelper.CanBreakBlocks = true;

            base.Stop(_context);
        }

        public override void Update(Context _context)
        {
            base.Update(_context);

            // If we're investigating, we can open doors
            SCoreUtils.CheckForClosedDoor(_context);

            // If the path is blocked, we need to clear the investigate position so the NPC
            // doesn't try to keep going if the task is restarted
            if (IsBlocked(_context))
            {
                AdvLogging.DisplayLog(
                    AdvFeatureClass,
                    $"{_context.Self} is blocked for {_context.Self.moveHelper.BlockedTime}");
                _context.Self.PlayGiveUpSound();
                ClearAndStop(_context);
                return;
            }

            if (HasReachedDestination(_context))
            {
                AdvLogging.DisplayLog(
                    AdvFeatureClass,
                    $"{_context.Self} has reached destination: {_context.Self.InvestigatePosition}");
                ClearAndStop(_context);
                return;
            }

            if (NeedsPath(_context))
            {
                AdvLogging.DisplayLog(
                    AdvFeatureClass,
                    $"{_context.Self} needs path, finding one to {_context.Self.InvestigatePosition}");
                UpdatePath(_context);
            }
        }

        // We only want to clear the investigate position if we stopped ourselves, not if another
        // task gained higher priority, so don't clear the investigate position in Stop()
        private void ClearAndStop(Context context)
        {
            context.Self.ClearInvestigatePosition();
            Stop(context);
        }

        private static bool HasReachedDestination(Context context)
        {
            // Use square magnitude, since it's computationally less expensive
            return (context.Self.position - context.Self.InvestigatePosition).sqrMagnitude < 4;
        }

        private static bool IsBlocked(Context _context)
        {
            return _context.Self.moveHelper.IsBlocked
                && _context.Self.moveHelper.BlockedTime > 0.35f;
        }

        private static bool NeedsPath(Context context)
        {
            return context.Self.getNavigator()?.noPathAndNotPlanningOne() ?? false;
        }

        private void UpdatePath(Context context)
        {
            var lookPosition = context.Self.InvestigatePosition;
            lookPosition.y += 0.8f;
            context.Self.SetLookPosition(lookPosition);

            context.Self.FindPath(
                // EAIApproachSpot uses World.FindSupportingBlockPos(InvestigatePosition), but if
                // we do that we won't be able to check if we've reached that position.
                context.Self.InvestigatePosition,
                context.Self.GetMoveSpeedAggro(),
                false,
                null);
        }
    }
}
