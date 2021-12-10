using System.Collections.Generic;
namespace UAI
{
    public class UAIConsiderationTargetNotHasTags : UAIConsiderationTargetHasTags
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;

        }
    }


    public class UAIConsiderationTargetHasTags : UAIConsiderationBase
    {
        public FastTags fastTags = FastTags.none;
        public override void Init(Dictionary<string, string> _parameters)
        {
            base.Init(_parameters);
            if (_parameters.ContainsKey("tags"))
                fastTags = FastTags.Parse(_parameters["tags"]);
        }

        public override float GetScore(Context _context, object target)
        {
            var targetEntity = UAIUtils.ConvertToEntityAlive(target);
            if (targetEntity == null)
                return 0f;

            return targetEntity.HasAnyTags(fastTags) ? 1f : 0f;
        }
    }
}