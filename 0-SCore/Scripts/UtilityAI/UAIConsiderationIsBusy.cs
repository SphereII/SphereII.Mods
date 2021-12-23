namespace UAI
{
    public class UAIConsiderationIsNotBusy : UAIConsiderationIsBusy
    {
        public override float GetScore(Context _context, object target)
        {
            return EntityUtilities.SameValue(base.GetScore(_context, target), 1f) ? 0f : 1f;
        }
    }
    public class UAIConsiderationIsBusy : UAIConsiderationBase
    {
        public override float GetScore(Context _context, object target)
        {
            _context.Self.emodel.avatarController.TryGetBool("IsBusy", out var isBusy);
            return isBusy ? 1f : 0f;
        }
    }
}