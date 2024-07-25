using System.Collections.Generic;
using System.Globalization;

namespace UAI
{
    public class UAIConsiderationTargetNotHasCVar : UAIConsiderationTargetHasCVar
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;

        }
    }
    public class UAIConsiderationTargetHasCVar : UAIConsiderationBase
    {
        private string _cvar;
        private float _value = float.MaxValue;

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
            EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            // check for defined _cvar
            if (string.IsNullOrEmpty(_cvar))
                return 0f;

            // If the npc does not have the cvar, don't process.
            if (!_context.Self.Buffs.HasCustomVar(_cvar))
                return 0f;

            // If value is not defined, assume just the existence of the cvar is fine.
            if (EntityUtilities.SameValue(_value, float.MaxValue))
                return 1f;

            // if the cvar matches the expected value, return 1
            if (EntityUtilities.SameValue(_value, _context.Self.Buffs.GetCustomVar(_cvar)))
                return 1f;

            return 0f;
        }
    }
}