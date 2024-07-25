using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationHasPath : UAIConsiderationTargetType
    {
        private string _targetTypes;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("TargetType")) _targetTypes = parameters["TargetType"];
        }

        public override float GetScore(Context _context, object target)
        {
            var paths = SphereCache.GetPaths(_context.Self.entityId);
            if (paths == null || paths.Count == 0)
            {
                paths = SCoreUtils.ScanForTileEntities(_context, _targetTypes);
                if (paths.Count == 0) return 0f;
            }

            SphereCache.AddPaths(_context.Self.entityId, paths);
            return 1f;
        }
    }
}