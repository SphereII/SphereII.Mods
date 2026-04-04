using System;
using System.Collections.Generic;
using System.Linq;
namespace UAI
{
    public class UAIConsiderationTargetNotHasBuffV4 : UAIConsiderationTargetHasBuffV4
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;

        }
    }
    public class UAIConsiderationTargetHasBuffV4 : UAIConsiderationBase
    {
        private string[] _buffList = System.Array.Empty<string>();
        private bool _matchAll = false;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("buffs"))
                _buffList = parameters["buffs"].ToLower().Split(',');

            if (parameters.ContainsKey("hasAll"))
                _matchAll = true;
        }

        public override float GetScore(Context _context, object target)
        {
            EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            if (_matchAll)
            {
                foreach (var buff in _buffList)
                {
                    if (!targetEntity.Buffs.HasBuff(buff))
                        return 0f;
                }
                return 1f;
            }

            foreach (var buff in _buffList)
            {
                if (targetEntity.Buffs.HasBuff(buff))
                    return 1f;
            }

            return 0f;
        }
    }
}