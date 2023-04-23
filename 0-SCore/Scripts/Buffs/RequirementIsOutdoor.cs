public class RequirementIsOutdoor : TargetedCompareRequirementBase
{
    public override bool ParamsValid(MinEventParams _params)
    {
        var result = false;

        var position = new Vector3i(_params.Self.position);
        position.y += 5;
        if (position.y > 250)
            position.y = 250;

        if (_params.Self.world.GetBlock(position).type == 0 && _params.Self.world.GetTerrainHeight(position.x, position.z) > position.y)
            result = true;

        if (invert)
            return !result;
        return result;
    }
}