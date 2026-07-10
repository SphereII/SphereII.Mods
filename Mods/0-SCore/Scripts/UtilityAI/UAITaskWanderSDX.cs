using System.Collections.Generic;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="WanderSDX, SCore" interest="LandClaim, Loot, VendingMachine, Forge, Campfire, Workstation, PowerSource"/>
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskWanderSDX : UAITaskWander
    {
        private string _interest;

        // Task instances are singletons: one UAITaskWanderSDX object is created per <task> XML
        // element at utilityai.xml parse time and shared by every entity using this package -
        // there is no per-entity instance. Per-entity state (wander target, stuck count) must be
        // keyed by entityId here, not stored as plain fields, or it gets overwritten/shared
        // across every NPC wandering at the same time.
        private readonly Dictionary<int, Vector3> _positions = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, int> _stuckCounts = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _redirectCooldowns = new Dictionary<int, int>();
        private const int RedirectCooldownTicks = 30; // ~1.5s at Update()'s per-frame call rate - starting estimate, tune from in-game test if it feels too short/long

        public override void initializeParameters()
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
            var position = RandomPositionGenerator.CalcAround(_context.Self, (int)maxWanderDistance, 10);

            // CalcAround returns Vector3.zero when no valid position exists (e.g. unloaded chunks at
            // biome spawn time). Pathing to world-origin always fails → BlockedTime rises → IsBlocked
            // fires → Stop → UAI restarts Wander → tight freeze loop. Bail early instead.
            if (position == Vector3.zero)
            {
                Stop(_context);
                return;
            }

            // CalcAround already validated air + solid ground within 10 blocks below.
            // Do NOT override Y with GetHeightAt — that returns natural terrain height only
            // (ignores placed blocks), silently dragging the target down to ground level and
            // routing the NPC off ledges/structures to reach it.

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
                        position = paths[index];
                    }
                }
            }

            _positions[_context.Self.entityId] = position;
            SCoreUtils.FindPath(_context, position, false);
        }


        public override void Update(Context _context)
        {
            int entityId = _context.Self.entityId;

            bool blocked = false;
            if (_redirectCooldowns.TryGetValue(entityId, out int cooldown) && cooldown > 0)
            {
                _redirectCooldowns[entityId] = cooldown - 1;

                // CheckJump can clear our path via EntityUtilities.Stop() when it detects an edge ahead.
                // Only cut the cooldown short if CheckJump *actually* cleared a previously-valid path —
                // not if the path was already dead from some other cause (which would just re-trigger
                // the redirect loop and cause micro-turning jitter).
                bool hadPath = !_context.Self.getNavigator().noPathAndNotPlanningOne();
                SCoreUtils.CheckJump(_context);
                if (hadPath && _context.Self.getNavigator().noPathAndNotPlanningOne())
                {
                    // CheckJump just cleared our path — re-issue it toward the same target instead
                    // of picking a new one. The target itself is fine, we just need to keep moving.
                    if (_positions.TryGetValue(entityId, out var currentTarget))
                    {
                        SCoreUtils.FindPath(_context, currentTarget, false);
                    }
                }
            }
            else
            {
                // Two independent ways this gets stuck: SCoreUtils.IsBlocked() (collision/raycast -
                // walked into something solid) and the navigator finding no path at all (e.g. an
                // unreachable same-elevation target on a ledge). The vanilla base class
                // (UAITaskWander.Update()) handled the second case with its own un-cooled Stop() call -
                // we no longer call base.Update() (UAITaskBase.Update() is empty, so nothing else is
                // lost) so BOTH cases funnel through the same cooldown-protected redirect logic
                // instead of one bypassing it and re-triggering the Stop/Start whiplash.
                blocked = SCoreUtils.IsBlocked(_context) || _context.Self.getNavigator().noPathAndNotPlanningOne();
            }

            if (blocked)
            {
                _stuckCounts.TryGetValue(entityId, out int stuckCount);
                stuckCount++;
                _stuckCounts[entityId] = stuckCount;

                Vector3 forward = _context.Self.GetForwardVector();
                Vector3 newPosition = Vector3.zero;
                bool foundGoodDirection = false;

                // Widen the vertical search range on repeat blocks too - a ledge NPC can get stuck
                // because every candidate within the normal Y range is at the same unreachable
                // elevation. Capped so a persistently-stuck NPC doesn't end up searching wildly far
                // above/below itself.
                int yRange = 10 + Mathf.Min(stuckCount - 1, 4) * 5; // 10, 15, 20, 25, 30

                int maxAttempts = stuckCount > 1 ? 5 : 1;
                for (int attempt = 0; attempt < maxAttempts && !foundGoodDirection; attempt++)
                {
                    var candidate = RandomPositionGenerator.CalcAround(_context.Self, (int)maxWanderDistance, yRange);
                    if (candidate == Vector3.zero) continue;

                    newPosition = candidate; // keep the most recent valid candidate as fallback

                    var toCandidate = candidate - _context.Self.position;
                    toCandidate.y = 0f;
                    if (toCandidate == Vector3.zero) continue;
                    foundGoodDirection = Vector3.Dot(toCandidate.normalized, forward) <= 0.3f;
                }

                if (newPosition == Vector3.zero)
                {
                    Stop(_context);
                    return;
                }

                // Keep CalcAround's Y as-is — it already validated air + ground below. Overriding
                // with GetHeightAt forces ground-level targets, which routes the NPC off ledges
                // to reach them. Let the pathfinder work with the elevation CalcAround found.
                _positions[entityId] = newPosition;
                SCoreUtils.FindPath(_context, newPosition, false);
                _redirectCooldowns[entityId] = RedirectCooldownTicks;
                return;
            }

            if (!_positions.TryGetValue(entityId, out var position))
                return;

            _context.Self.SetLookPosition(position);

            var distance = Vector3.Distance(_context.Self.position, position);
            if (distance < 0.5f)
            {
                _stuckCounts[entityId] = 0; // reached destination cleanly - reset escalation
                _redirectCooldowns[entityId] = 0;
                Stop(_context);
            }
        }
    }
}
