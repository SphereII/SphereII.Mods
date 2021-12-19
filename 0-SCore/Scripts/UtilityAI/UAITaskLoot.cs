using UnityEngine;

namespace UAI
{
    public class UAITaskLoot : UAITaskMoveToTarget
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        private string _targetTypes;
        private Vector3 _vector;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("TargetType")) _targetTypes = Parameters["TargetType"];
        }

        public override void Update(Context _context)
        {
            var dist = Vector3.Distance(_vector, _context.Self.position);
            if (dist <= distance)
                SCoreUtils.CheckContainer(_context, _vector);

           // base.Update(_context);
            SCoreUtils.CheckForClosedDoor(_context);
        }
        public override void Start(Context _context)
        {
            var paths = SCoreUtils.ScanForTileEntities(_context, _targetTypes);
            if (paths.Count == 0)
            {
                Stop(_context);
                return;
            }

            if (distance == 0)
                distance = 1f;
            _vector = paths[0];
            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Start Workstation: {_context.Self.EntityName} ( {_context.Self.entityId} Position: {_vector} ");
            SCoreUtils.FindPath(_context, _vector + Vector3.forward, false);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }
    }
}