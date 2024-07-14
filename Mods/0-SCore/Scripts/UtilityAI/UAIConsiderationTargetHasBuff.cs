using System;
using System.Collections.Generic;
using System.Linq;
namespace UAI
{
    public class UAIConsiderationTargetNotHasBuff : UAIConsiderationTargetHasBuff
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;

        }
    }
    public class UAIConsiderationTargetHasBuff : UAIConsiderationBase
    {
        private String _buffs;
        private bool matchAll = false;
        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("buffs"))
            {
                _buffs = parameters["buffs"].ToLower();
            }

            if (parameters.ContainsKey("hasAll")) matchAll = true;
            

        }

        public override float GetScore(Context _context, object target)
        {
            EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            var buffs = _buffs.Split(',');
            if ( matchAll)
            {
                // If any of the buffs do not exist, then exit with a failure.
                for (int x = 0; x < buffs.Length; x++)
                {
                    if (!targetEntity.Buffs.HasBuff(buffs[x]))
                        return 0f;
                }
                return 1f;
            }
            for (int x = 0; x < buffs.Length; x++)
            {
                if (targetEntity.Buffs.HasBuff(buffs[x]))
                    return 1f;
            }


            return 0f;
        }
    }
}