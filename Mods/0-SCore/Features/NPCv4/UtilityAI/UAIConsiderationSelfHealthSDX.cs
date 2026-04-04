namespace UAI
{

    public class UAIConsiderationSelfHealthSDXV4 : UAIConsiderationTargetHealthSDXV4
    {
        public override float GetScore(Context _context, object target)
        {
            return base.GetScore(_context, _context.Self);
        }
    }
}