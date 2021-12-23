namespace UAI
{

    public class UAIConsiderationIsAlerted : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            if (_context.Self.IsAlert && _context.Self.HasInvestigatePosition)
                return 1f;
            return 0f;
        }
    }
}