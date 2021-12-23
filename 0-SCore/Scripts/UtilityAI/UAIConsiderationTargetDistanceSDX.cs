using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationNotTargetDistanceSDX : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationTargetDistanceSDX : UAIConsiderationBase
    {
        private float min;
        private float max = 9126f;
        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("min"))
            {
                this.min = StringParsers.ParseFloat(_parameters["min"], 0, -1, NumberStyles.Any);
                this.min *= this.min;
            }
            if (_parameters.ContainsKey("max"))
            {
                this.max = StringParsers.ParseFloat(_parameters["max"], 0, -1, NumberStyles.Any);
                this.max *= this.max;
            }
        }

        public override float GetScore(Context _context, object target)
        {
            float result = 0f;
            EntityAlive entityAlive = UAIUtils.ConvertToEntityAlive(target);
            if (entityAlive != null)
            {
                if ( entityAlive.entityId == _context.Self.entityId)
                {
                    Debug.Log("it's me!");
                    return 0f;
                }
                float num = UAIUtils.DistanceSqr(_context.Self.position, entityAlive.position);
                result = Mathf.Clamp01(Mathf.Max(0f, num - this.min) / (this.max - this.min));
            }
            if (target.GetType() == typeof(Vector3))
            {
                Vector3 pointB = (Vector3)target;
                float num2 = UAIUtils.DistanceSqr(_context.Self.position, pointB);
                result = Mathf.Clamp01(Mathf.Max(0f, num2 - this.min) / (this.max - this.min));
            }
            return result;
        }
    }
}