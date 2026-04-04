using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

namespace UAI
{
    [Preserve]
    public class UAITaskFollowSDXV4 : UAITaskBase
    {
        private EntityAlive _leader;
        
        // Configurable Parameters
        private float _minDistance = 2f;      // Stop moving when this close
        private float _maxDistance = 25f;     // Teleport if this far
        private float _teleportDelay = 5f;    // How long to stay stuck before teleporting
        
        // State Variables
        private Vector3 _lastLeaderPos;
        private float _stuckTimer;
        private float _pathRecalculateTimer;
        private const float PathRecalculateInterval = 0.5f; // Only pathfind every 0.5 seconds max

        public override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("stop_distance")) _minDistance = StringParsers.ParseFloat(Parameters["stop_distance"]);
            if (Parameters.ContainsKey("max_distance")) _maxDistance = StringParsers.ParseFloat(Parameters["max_distance"]);
            if (Parameters.ContainsKey("teleport_time")) _teleportDelay = StringParsers.ParseFloat(Parameters["teleport_time"]);
        }

        public override void Start(Context context)
        {
            base.Start(context);
            
            _leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
            if (_leader == null)
            {
                Stop(context);
                return;
            }

            // Reset timers
            _stuckTimer = 0f;
            _pathRecalculateTimer = 0f;
            _lastLeaderPos = _leader.position;

            CombatUtils.SetCrouching(context, _leader.IsCrouching);
            EntityUtilities.ClearAttackTargets(context.Self.entityId);

            // Initial Move
            MoveToLeader(context);
        }

        public override void Update(Context context)
        {
            base.Update(context);

            // Safety Check: Leader might have died or disconnected since Start()
            if (_leader == null || _leader.IsDead())
            {
                Stop(context);
                return;
            }

            PathingUtils.SetSpeed(context, true);
            CombatUtils.SetCrouching(context, _leader.IsCrouching);

            float distToLeader = Vector3.Distance(context.Self.position, _leader.position);

            // 1. TELEPORT CHECKS
            // Condition A: Too far away
            if (distToLeader > _maxDistance)
            {
                PathingUtils.TeleportToLeader(context, false);
                Stop(context); // Reset task after teleport to force re-evaluation
                return;
            }

            // Condition B: Blocked / Stuck logic
            if (PathingUtils.IsBlocked(context))
            {
                _stuckTimer += Time.deltaTime;
                if (_stuckTimer >= _teleportDelay)
                {
                    PathingUtils.TeleportToLeader(context, false);
                    _stuckTimer = 0f;
                }
                // If we are stuck, we don't want to run the movement logic below
                return;
            }
            else
            {
                // Reset stuck timer if we are moving freely
                _stuckTimer = 0f;
            }

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
                context.Self.RotateTo(_leader.position.x, _leader.position.y, _leader.position.z, 8f, 8f);
            }
            else
            {
                // We are too far, check if we need to update our path
                UpdatePathing(context);
            }
        }

        private void MoveToLeader(Context context)
        {
            _lastLeaderPos = _leader.position;
            // Use boolean 'false' for run to prevent constant sprinting if you want them to match speed, 
            // otherwise 'true' forces sprint. SCoreUtils usually handles speed based on distance.
            PathingUtils.FindPath(context, _lastLeaderPos, false); 
        }

        private void UpdatePathing(Context context)
        {
            _pathRecalculateTimer -= Time.deltaTime;

            // Don't recalc if we are on cooldown
            if (_pathRecalculateTimer > 0f) return;

            // Don't recalc if the leader hasn't moved much since our last path
            float leaderMovement = Vector3.Distance(_leader.position, _lastLeaderPos);
            if (leaderMovement < 1f)
            {
                // Leader is roughly in the same spot, do we still have a path?
                if (context.Self.navigator.getPath() != null) return;
            }

            // Leader moved > 1m OR we have no path. Recalculate.
            MoveToLeader(context);
            
            // Reset cooldown
            _pathRecalculateTimer = PathRecalculateInterval;
        }
    }
}