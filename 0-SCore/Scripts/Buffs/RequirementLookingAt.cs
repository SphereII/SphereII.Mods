using System.Xml;

// 	<requirement name="RequirementLookingAt, SCore" block="Campfire" />
// 	<requirement name="RequirementLookingAt, SCore" block="Campfire, Workstation" />
public class RequirementLookingAt : RequirementBase
{
    public string blocks = "";

    public override bool ParamsValid(MinEventParams _params)
    {
        var blockPosition = _params.Self.GetBlockPosition();
        var num = World.toChunkXZ(blockPosition.x);
        var num2 = World.toChunkXZ(blockPosition.z);

        if (blocks.Contains("Any") || string.IsNullOrEmpty(blocks))
            return true;

        var world = _params.Self.world;
        for (var i = -1; i < 2; i++)
        {
            for (var j = -1; j < 2; j++)
            {
                var chunk = (Chunk)world.GetChunkSync(num + j, num2 + i);
                if (chunk == null) continue;

                var tileEntities = chunk.GetTileEntities();
                foreach (var tileEntity in tileEntities.list)
                {
                    var distanceSq = _params.Self.GetDistanceSq(tileEntity.ToWorldPos().ToVector3());

                    // If the TileEntity is greater than 2 away, don't bother checking.
                    if (distanceSq > 2) continue;
                    if (blocks.Contains(tileEntity.GetTileEntityType().ToString()))
                        return true;
                }
            }
        }

        return false;
    }

    public override bool ParseXmlAttribute(XmlAttribute _attribute)
    {
        var name = _attribute.Name;
        if (name != "block") return base.ParseXmlAttribute(_attribute);
        blocks = _attribute.Value;
        return true;
    }
}