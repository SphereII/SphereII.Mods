using UnityEngine;

namespace UAI
{
    public class UAITaskLoot : UAITaskMoveToTarget
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        private string _buff;
        private string _targetTypes;
        private bool _hadBuff;
        private Vector3 _vector;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];
            if (Parameters.ContainsKey("TargetType")) _targetTypes = Parameters["TargetType"];
        }

        public override void Update(Context _context)
        {
            // true if you looted it.
            if (SCoreUtils.CheckContainer(_context, _vector))
            {
                Stop(_context);
                return;
            }

            base.Update(_context);
            SCoreUtils.CheckForClosedDoor(_context);

            if (SCoreUtils.IsBlocked(_context))
            {
                EntityUtilities.Stop(_context.Self.entityId);
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Am Blocked.: {_context.Self.EntityName} ( {_context.Self.entityId} ");
                Stop(_context);
                _context.Self.getNavigator().clearPath();
            }
        }

        public override void Reset(Context _context)
        {
            _hadBuff = false;
            _vector = Vector3.zero;
            base.Reset(_context);
        }

        public override void Stop(Context _context)
        {
            if (SCoreUtils.HasBuff(_context, _buff))
            {
                EntityUtilities.Stop(_context.Self.entityId);
                // Interrupt the buff we are being attacked.
                if (EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null)
                    return;
            }

            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Stop: {_context.Self.EntityName} ( {_context.Self.entityId} ");
            _vector = Vector3.zero;
            base.Stop(_context);
        }

        public override void Start(Context _context)
        {
            var paths = SCoreUtils.ScanForTileEntities(_context, _targetTypes);
            if (paths.Count == 0)
            {

                Stop(_context);
                return;
            }

            _vector = paths[0];
            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Start Workstation: {_context.Self.EntityName} ( {_context.Self.entityId} Position: {_vector} ");
            SCoreUtils.FindPath(_context, _vector + Vector3.forward, false);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }
    }
}