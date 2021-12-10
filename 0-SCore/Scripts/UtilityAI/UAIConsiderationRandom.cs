using System.Collections.Generic;
using System.Globalization;

namespace UAI
{
    public class UAIConsiderationRandom : UAIConsiderationBase
    {
        private float _percent = 0.25f;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("percent"))
                _percent = StringParsers.ParseFloat(parameters["percent"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context _context, object target)
        {
            // return 1f;
            if (_context.Self.rand.RandomRange(0f, 1f) < _percent)
                return 1f;
            return 0;
        }
    }
}