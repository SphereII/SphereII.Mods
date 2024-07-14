namespace UAI
{
    public class UAIConsiderationIsNotInside : UAIConsiderationIsInside
    {
        public override float GetScore(Context _context, object target)
        {
            return (base.GetScore(_context, target) == 1f) ? 0f : 1f;
        }
    }

    public class UAIConsiderationIsInside: UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            if (!_context.Self.world.IsOpenSkyAbove(0, (int)_context.Self.position.x, (int)_context.Self.position.y + 1, (int)_context.Self.position.z))
                return 1f;
            return 0f;
        }
    }
}