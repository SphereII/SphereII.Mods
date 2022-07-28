using System.Xml;

public class MinEventActionRemoveFire : MinEventActionRemoveBuff
{

    // <triggered_effect trigger="onSelfDamagedBlock" action="RemoveFire, SCore" target="positionAOE" range="5"/> <!-- range is int -->

    public override void Execute(MinEventParams _params)
    {
        int range = (int)maxRange;
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                for (int y = -range; y <= range; y++)
                {
                    var vector = new Vector3i(_params.Position.x + x, _params.Position.y + y, _params.Position.z + z);
                    FireManager.Instance.Extinguish(vector);

                }
            }
        }


    }

}