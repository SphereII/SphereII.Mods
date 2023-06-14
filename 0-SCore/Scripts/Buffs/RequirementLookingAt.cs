using System.Xml;
using System.Xml.Linq;

// 	<requirement name="RequirementLookingAt, SCore" block="Campfire" />
// 	<requirement name="RequirementLookingAt, SCore" block="Campfire, Workstation" cvar="focusBlockLocation" />
public class RequirementLookingAt : RequirementBase
{
    public string blocks = "";
    public string cvar = "focusBlockLocation";
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
                    var position = tileEntity.ToWorldPos().ToVector3();
                    var distanceSq = _params.Self.GetDistanceSq(position);

                    // If the TileEntity is greater than 2 away, don't bother checking.
                    if (distanceSq > 2) continue;
                    if (blocks.Contains(tileEntity.GetTileEntityType().ToString()))
                    {
                        // lower than my feet
                        if (position.y < _params.Self.position.y)
                        {
                            _params.Self.Buffs.AddCustomVar(cvar, -1);
                        }
                        // at my feet
                        else if (position.y == _params.Self.position.y + 0.1f)
                        {
                            _params.Self.Buffs.AddCustomVar(cvar, 0);
                        }
                        // at my eye level
                        else if (position.y <= _params.Self.GetEyeHeight())
                        {
                            _params.Self.Buffs.AddCustomVar(cvar, 1);
                        }
                        else if (position.y > _params.Self.GetHeight())
                        {
                            _params.Self.Buffs.AddCustomVar(cvar, 2);
                        }
                        else
                            _params.Self.Buffs.AddCustomVar(cvar, 0);
                        return true;
                    }
                }
            }
        }

        _params.Self.Buffs.RemoveCustomVar(cvar);
        return false;
    }

    public override bool ParseXAttribute(XAttribute _attribute)
    {
        var name = _attribute.Name.LocalName;
        if (name == "cvar")
        {
            cvar = _attribute.Value;
        }
        else if (name == "block")
        {
            blocks = _attribute.Value;
        }
        else
            return base.ParseXAttribute(_attribute);

        return true;
    }
}