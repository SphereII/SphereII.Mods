

using System;

namespace UAI
{

    public class UAITaskFireBuff : UAITaskBase
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";
        private String _buffs;
        private String _currentBuff;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("buffs"))
                _buffs = Parameters["buffs"];
        }

        public override void Start(Context _context)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Start: {_context.Self.EntityName} ( {_context.Self.entityId}");
            var buffArray = _buffs.Split(',');

            _currentBuff = buffArray[UnityEngine.Random.Range(0, buffArray.Length)];
            _context.Self.Buffs.AddBuff(_currentBuff);

            base.Start(_context);
        }

        public override void Stop(Context _context)
        {
            if (EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null)
            {
                if (_context.Self.Buffs.HasBuff(_currentBuff))
                    return;
            }

            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $" {GetType()} Stop: {_context.Self.EntityName} ( {_context.Self.entityId}");
            _context.Self.Buffs.RemoveBuff(_currentBuff);
            base.Stop(_context);
        }

        public override void Reset(Context _context)
        {
            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Reset: {_context.Self.EntityName} ( {_context.Self.entityId}");
            base.Reset(_context);
        }

        public override void Update(Context _context)
        {
            if (!_context.Self.Buffs.HasBuff(_currentBuff))
                Stop(_context);
        }
    }
}