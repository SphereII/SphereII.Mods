using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationNotTargetDistanceSDX : UAIConsiderationTargetDistance
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationTargetDistanceSDX : UAIConsiderationTargetDistance
    {
        private float min;
        private float max = 9126f;

        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("min"))
                this.min = StringParsers.ParseFloat(_parameters["min"], 0, -1, NumberStyles.Any);

            if (_parameters.ContainsKey("max"))
                this.max = StringParsers.ParseFloat(_parameters["max"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context _context, object target)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(target);
            var result = 0f;
            if (entityAlive != null)
                result = UAIUtils.DistanceSqr(_context.Self.position, entityAlive.position);

            if (target is Vector3 vector3)
                result = UAIUtils.DistanceSqr(_context.Self.position, vector3);

            if (result > min && result < max)
                return 1f;
            return 0f;
        }
    }
}