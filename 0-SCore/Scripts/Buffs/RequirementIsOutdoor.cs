public class RequirementIsOutdoor : TargetedCompareRequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        var result = false;

        var position = new Vector3i(_params.Self.position);
        position.y += 5;
        if (position.y > 250)
            position.y = 250;

        if (GameManager.Instance.World.IsOpenSkyAbove(0, position.x, position.y, position.z))
            result = true;

        if (invert)
            return !result;
        return result;
    }
}