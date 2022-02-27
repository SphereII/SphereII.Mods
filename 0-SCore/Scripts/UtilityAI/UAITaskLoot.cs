using GamePath;
using UnityEngine;

namespace UAI
{
    public class UAITaskLoot : UAITaskMoveToTarget
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        private string _targetTypes;
        private Vector3 _vector;
        private string _buff;

        private bool hadBuff = false;
        private EntityAlive _leader;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("TargetType")) _targetTypes = Parameters["TargetType"];
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];

        }

        public void ForceStop(Context _context)
        {
            _context.Self.Buffs.RemoveBuff(_buff);
            Stop(_context);

        }
        public override void Stop(Context _context)
        {
            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff)) return;

            SphereCache.RemovePath(_context.Self.entityId, _vector);
            BlockUtilitiesSDX.removeParticles(new Vector3i(_vector));
            base.Stop(_context);
        }
        public override void Update(Context _context)
        {
            // If we have the activity buff, just wait until it wears off
            if (_context.Self.Buffs.HasBuff(_buff)) return;

            // If we had it, and it's gone, then we are done with this location.
            if ( hadBuff)
            {
                Stop(_context);
                return;
            }

            if (SCoreUtils.IsBlocked(_context))
                ForceStop(_context);

            if ( _leader)
                SCoreUtils.SetCrouching(_context, _leader.IsCrouching);

            if (SCoreUtils.CheckContainer(_context, _vector))
            {
                // If the NPC does not have the buff anymore, check to see if they ever had it for this task
                if (!_context.Self.Buffs.HasBuff(_buff))
                {
                    _context.Self.Buffs.AddBuff(_buff);
                    hadBuff = true;
                }
            }

           base.Update(_context);
        }


        public override void Start(Context _context)
        {
            var paths = SphereCache.GetPaths(_context.Self.entityId);
            if (paths == null || paths.Count == 0)
            {
                Stop(_context);
                return;
            }
            if (distance == 0)
                distance = 4f;

            hadBuff = false;
            // sort
            paths.Sort(new SCoreUtils.NearestPathSorter(_context.Self));
            _vector = paths[0];
            if (paths.Count > 5 )
            {
                var index = _context.Self.rand.RandomRange(0, 4);
                _vector = paths[index];
            }
            
            //if ( (_context.Self.position - _vector).sqrMagnitude > 500)
            //{
            //    Log.Out($"TaskLoot(): Start:  Force Stopping due to Tile Entity being too far away {_vector} {_targetTypes}");

            //    SphereCache.RemovePaths(_context.Self.entityId);
            //    ForceStop(_context);
            //    return;
            //}
            if (!GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) )
                BlockUtilitiesSDX.addParticles("", new Vector3i(_vector));

            _leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId) as EntityAlive;

            SCoreUtils.FindPath(_context, _vector, false);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }
    }
}