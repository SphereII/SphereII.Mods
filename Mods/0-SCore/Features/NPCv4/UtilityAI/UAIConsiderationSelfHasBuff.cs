using System;
using System.Collections.Generic;
using UnityEngine;

namespace UAI
{

    public class UAIConsiderationSelfNotHasBuffV4 : UAIConsiderationSelfHasBuffV4
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationSelfHasBuffV4 : UAIConsiderationBase
    {
        private string[] _buffList = System.Array.Empty<string>();

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("buffs"))
                _buffList = parameters["buffs"].Split(',');
        }

        public override float GetScore(Context context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(context.Self);
            if (targetEntity == null)
                return 0f;

            foreach (var buff in _buffList)
            {
                if (targetEntity.Buffs.HasBuff(buff))
                    return 1f;
            }
            return 0f;
        }
    }
}