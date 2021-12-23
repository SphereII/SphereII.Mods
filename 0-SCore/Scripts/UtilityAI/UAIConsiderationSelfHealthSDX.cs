namespace UAI
{

    public class UAIConsiderationSelfHealthSDX : UAIConsiderationTargetHealthSDX
    {
        public override float GetScore(Context _context, object target)
        {
            return base.GetScore(_context, _context.Self);
        }
    }
}