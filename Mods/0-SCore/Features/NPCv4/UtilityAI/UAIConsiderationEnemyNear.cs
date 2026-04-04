using System.Collections.Generic;
using System.Globalization;

namespace UAI
{
    public class UAIConsiderationEnemyNotNearV4 : UAIConsiderationEnemyNearV4
    {
        public override float GetScore(Context context, object target)
        {
            return VisionUtils.IsEnemyNearby(context, Distance) ? 0f : 1f;
        }
    }

    public class UAIConsiderationEnemyNearV4 : UAIConsiderationBase
    {
        protected float Distance = 20f;
        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("distance"))
                this.Distance = StringParsers.ParseFloat(_parameters["distance"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context context, object target)
        {
            return VisionUtils.IsEnemyNearby(context, Distance) ? 1f : 0f;
        }
    }
}