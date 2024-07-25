public class RequirementIsUnderGround : TargetedCompareRequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        var result = false;

        var position = new Vector3i(_params.Self.position);
        if (position.y < GameManager.Instance.World.GetTerrainHeight(position.x, position.z))
            result = true;

        if (invert)
            return !result;
        return result;
    }
}
