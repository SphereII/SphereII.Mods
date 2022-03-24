using System.Collections.Generic;
using System.Globalization;

namespace UAI
{
    public class UAIConsiderationEnemyNotNear : UAIConsiderationEnemyNear
    {
        public override float GetScore(Context _context, object target)
        {
            return SCoreUtils.IsEnemyNearby(_context, distance) ? 0f : 1f;
        }
    }

    public class UAIConsiderationEnemyNear : UAIConsiderationBase
    {
        public float distance = 20f;
        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("distance"))
                this.distance = StringParsers.ParseFloat(_parameters["distance"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context _context, object target)
        {
 

            return SCoreUtils.IsEnemyNearby(_context, distance) ? 1f : 0f;
        }
    }
}