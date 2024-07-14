namespace UAI
{
    public class UAIConsiderationHasHomePosition : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            if (_context.Self.isWithinHomeDistanceCurrentPosition())
                return 0f;

            return 1f;
        }
    }
}