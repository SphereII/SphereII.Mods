using UnityEngine;

namespace UAI
{
    public class UAITaskLoot : UAITaskMoveToTarget
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        private string _targetTypes;
        private Vector3 _vector;

        private float _timeOut = 100f;
        private float _currentTimeout;
        private string _buff;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("TargetType")) _targetTypes = Parameters["TargetType"];
            if (Parameters.ContainsKey("timeout")) _timeOut = StringParsers.ParseFloat(Parameters["timeout"]);
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];

        }

        public override void Update(Context _context)
        {
            if (SCoreUtils.IsBlocked(_context))
                this.Stop(_context);

            if (SCoreUtils.CheckContainer(_context, _vector))
            {
                if (!_context.Self.Buffs.HasBuff(_buff))
                    _context.Self.Buffs.AddBuff(_buff);

                _currentTimeout--;
                if (_currentTimeout < 0) Stop(_context);
                return;
            }

           base.Update(_context);
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
                distance = 4f;

            _vector = paths[0];
            if (paths.Count > 3)
            {
                var randIndedx = _context.Self.rand.RandomRange(3);
                _vector = paths[randIndedx];
            }

            _currentTimeout = _timeOut;
            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Start Workstation: {_context.Self.EntityName} ( {_context.Self.entityId} Position: {_vector} ");
            SCoreUtils.FindPath(_context, _vector, false);
            _context.ActionData.Started = true;
            _context.ActionData.Executing = true;
        }
    }
}