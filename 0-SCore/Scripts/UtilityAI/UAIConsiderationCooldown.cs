using System.Collections.Generic;
using System.Globalization;

namespace UAI
{

    public class UAIConsiderationCooldown : UAIConsiderationBase
    {
        private string _cvar;
        private float _value = 10f;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("cvar"))
                _cvar = parameters["cvar"];

            if (parameters.ContainsKey("value"))
                _value = StringParsers.ParseFloat(parameters["value"], 0, -1, NumberStyles.Any);
        }

        public override float GetScore(Context _context, object target)
        {
            // check for defined _cvar
            if (string.IsNullOrEmpty(_cvar))
                return 0f;


            var currentTime = GameManager.Instance.World.GetWorldTime();
            if (!_context.Self.Buffs.HasCustomVar(_cvar))
                _context.Self.Buffs.SetCustomVar(_cvar, currentTime + _value);

            UnityEngine.Debug.Log($"Baker: {_context.Self.Buffs.GetCustomVar(_cvar)} Current Time: {currentTime}");
            if (_context.Self.Buffs.GetCustomVar(_cvar) > currentTime) return 0f;

            _context.Self.Buffs.SetCustomVar(_cvar, currentTime + _value);
            return 1f;
        }
    }
}