
using System;
using UnityEngine;

public class RequirementIsUnderGround : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {

        bool result = false;
        
        Vector3i position = new Vector3i(_params.Self.position);
        if (position.y < GameManager.Instance.World.GetTerrainHeight(position.x, position.z))
            result = true;

        if (this.invert)
            return !result;
        else
            return result;
    }

}
public class RequirementIsOutdoor : TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {

        bool result =false;


        Vector3i position = new Vector3i(_params.Self.position);
        position.y += 5;
        if (position.y > 250)
            position.y = 250;

        if (_params.Self.world.GetBlock(position).type == 0 && _params.Self.world.GetTerrainHeight(position.x, position.z) > position.y )
            result = true;

        if (this.invert)
            return !result;
        else
            return result;
    }

}
