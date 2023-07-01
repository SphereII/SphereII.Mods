using System;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationSelfNotHasBuff : UAIConsiderationSelfHasBuff
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationSelfHasBuff : UAIConsiderationBase
    {
        private string _buffs;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("buffs"))
                _buffs = parameters["buffs"];
        }

        public override float GetScore(Context context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(context.Self);
            if (targetEntity == null)
                return 0f;

            var buffs = _buffs.Split(',');
            foreach (var t in buffs)
            {
                if (targetEntity.Buffs.HasBuff(t))
                    return 1f;

            }
            return 0f;
        }
    }
}