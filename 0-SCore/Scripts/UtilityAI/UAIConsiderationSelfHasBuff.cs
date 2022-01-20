using System;
using System.Collections.Generic;
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
        private String _buffs;

        public override void Init(Dictionary<string, string> parameters)
        {
            base.Init(parameters);
            if (parameters.ContainsKey("buffs"))
                _buffs = parameters["buffs"].ToLower();
        }

        public override float GetScore(Context _context, object target)
        {
            EntityAlive targetEntity = UAIUtils.ConvertToEntityAlive(_context.Self);
            if (targetEntity == null)
                return 0f;

            //// If there's no comma, it's just one tag
            //if (!_buffs.Contains(","))
            //{
            //    if (targetEntity.Buffs.HasBuff(_buffs))
            //        return 1f;
            //}

            var buffs = _buffs.Split(',');
            for (int x = 0; x < buffs.Length; x++)
            {
                if (targetEntity.Buffs.HasBuff(buffs[x]))
                    return 1f;
            }

            return 0f;
        }
    }
}