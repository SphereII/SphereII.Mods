using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
    [Preserve]
    public class UAITaskFollowSDX : UAITaskBase
    {
        // Configurable Parameters
        private float _minDistance = 2f;      // Stop moving when this close
        private float _maxDistance = 25f;     // Teleport if this far
        private float _teleportDelay = 5f;    // How long to stay stuck before teleporting
        private float _runDistance = 10f;     // Run to catch up when farther than this

        private const float PathRecalculateInterval = 0.5f; // Only pathfind every 0.5 seconds max
        private const float PathRetryInterval = 1f; // while stuck, retry pathing this often

        private static readonly FastTags<TagGroup.Global> RunningTag = FastTags<TagGroup.Global>.Parse("running");

        /// <summary>
        /// Per-NPC state. The game keeps one copy of each AI package for every NPC that uses it, so
        /// this task object is shared; anything stored in a plain field would be overwritten by
        /// the next NPC that runs it (e.g. two followers swapping leaders and stuck timers).
        /// </summary>
        private class FollowState
        {
            public EntityAlive Leader;
            public Vector3 LastLeaderPos;
            public float StuckTimer;
            public float PathRecalculateTimer;
            public float PathRetryTimer;
        }

        private readonly Dictionary<int, FollowState> _states = new Dictionary<int, FollowState>();

        public override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("stop_distance")) _minDistance = StringParsers.ParseFloat(Parameters["stop_distance"]);
            if (Parameters.ContainsKey("max_distance")) _maxDistance = StringParsers.ParseFloat(Parameters["max_distance"]);
            if (Parameters.ContainsKey("teleport_time")) _teleportDelay = StringParsers.ParseFloat(Parameters["teleport_time"]);
            if (Parameters.ContainsKey("run_distance")) _runDistance = StringParsers.ParseFloat(Parameters["run_distance"]);
        }

        public override void Start(Context context)
        {
            base.Start(context);

            var leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
            if (leader == null)
            {
                Stop(context);
                return;
            }

            var state = new FollowState { Leader = leader, LastLeaderPos = leader.position };
            _states[context.Self.entityId] = state;

            SCoreUtils.SetCrouching(context, leader.IsCrouching);
            EntityUtilities.ClearAttackTargets(context.Self.entityId);

            // Initial Move
            MoveToLeader(context, state);
        }

        public override void Stop(Context context)
        {
            _states.Remove(context.Self.entityId);
            base.Stop(context);
        }

        public override void Reset(Context context)
        {
            _states.Remove(context.Self.entityId);
            base.Reset(context);
        }

        public override void Update(Context context)
        {
            base.Update(context);

            // Safety Check: Leader might have died or disconnected since Start()
            if (!_states.TryGetValue(context.Self.entityId, out var state) || state.Leader == null || state.Leader.IsDead())
            {
                Stop(context);
                return;
            }

            var leader = state.Leader;
            float distToLeader = Vector3.Distance(context.Self.position, leader.position);

            SCoreUtils.SetSpeed(context, ShouldRun(leader, distToLeader));
            SCoreUtils.SetCrouching(context, leader.IsCrouching);

            // 1. TELEPORT CHECKS
            // Condition A: Too far away
            if (distToLeader > _maxDistance)
            {
                SCoreUtils.TeleportToLeader(context, false);
                Stop(context); // Reset task after teleport to force re-evaluation
                return;
            }

            // Condition B: Blocked / Stuck logic
            if (SCoreUtils.IsBlocked(context))
            {
                state.StuckTimer += Time.deltaTime;

                // While stuck, retry pathing toward the leader more aggressively than the normal
                // 0.5s/1m-moved gate in UpdatePathing() - the cached path may itself be why we're
                // stuck (a locally bad route), and that gate would otherwise never recalculate it
                // as long as the leader stays still.
                state.PathRetryTimer -= Time.deltaTime;
                if (state.PathRetryTimer <= 0f)
                {
                    MoveToLeader(context, state);
                    state.PathRetryTimer = PathRetryInterval;
                }

                if (state.StuckTimer >= _teleportDelay)
                {
                    SCoreUtils.TeleportToLeader(context, false);
                    state.StuckTimer = 0f;
                }
                // If we are stuck, we don't want to run the movement logic below
                return;
            }

            // Reset stuck timer if we are moving freely
            state.StuckTimer = 0f;
            state.PathRetryTimer = 0f; // reset so the next stuck episode retries immediately

            // 2. MOVEMENT LOGIC
            if (distToLeader <= _minDistance)
            {
                // We are close enough. Stop moving, but KEEP THE TASK RUNNING.
                // Stopping the task here causes the AI to "stutter" (Start -> Stop -> Start).
                if (context.Self.navigator.getPath() != null)
                {
                    context.Self.navigator.clearPath();
                }

                // Face the leader while waiting
                // V4 models show the stored body pitch as a whole-body lean, so don't pitch them.
                context.Self.RotateTo(leader.position.x, leader.position.y, leader.position.z, 8f, context.Self is EntityAliveSDXV4 ? 0f : 8f);
            }
            else
            {
                // We are too far, check if we need to update our path
                UpdatePathing(context, state);
            }
        }

        /// <summary>
        /// Match the leader's pace: run when they run, or to catch up when we've fallen behind.
        /// This used to pass panic=true every tick, which overrode the pace matching in
        /// SCoreUtils.FindPath and made followers sprint all the time.
        /// </summary>
        private bool ShouldRun(EntityAlive leader, float distToLeader)
        {
            if (distToLeader > _runDistance) return true;
            return leader.CurrentMovementTag.Test_AllSet(RunningTag);
        }

        private void MoveToLeader(Context context, FollowState state)
        {
            state.LastLeaderPos = state.Leader.position;
            SCoreUtils.FindPath(context, state.LastLeaderPos, false);
        }

        private void UpdatePathing(Context context, FollowState state)
        {
            state.PathRecalculateTimer -= Time.deltaTime;

            // Don't recalc if we are on cooldown
            if (state.PathRecalculateTimer > 0f) return;

            // Don't recalc if the leader hasn't moved much since our last path
            float leaderMovement = Vector3.Distance(state.Leader.position, state.LastLeaderPos);
            if (leaderMovement < 1f)
            {
                // Leader is roughly in the same spot, do we still have a path?
                if (context.Self.navigator.getPath() != null) return;
            }

            // Leader moved > 1m OR we have no path. Recalculate.
            MoveToLeader(context, state);

            // Reset cooldown
            state.PathRecalculateTimer = PathRecalculateInterval;
        }
    }
}
