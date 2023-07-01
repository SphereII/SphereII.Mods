using System.Threading.Tasks;
using System.Xml.Linq;
using JetBrains.Annotations;
using UnityEngine;
/// <summary>
/// Set's a single block on fire.
/// </summary>
/// <remarks>
/// This effect will set a block on fire, using the target's position, and the range. There is an optional delay time, which will delay the fire from starting.
/// Example:
///     <triggered_effect trigger="onSelfDamagedBlock" action="AddFireDamage, SCore" target="positionAOE" range="5" delayTime="10" />
/// </remarks>
[UsedImplicitly]
public class MinEventActionAddFireDamage : MinEventActionRemoveBuff
{
    private static readonly string AdvFeatureClass = "FireManagement";
    private float _delayTime;

    public override void Execute(MinEventParams @params)
    {
        if (FireManager.Instance == null) return;
        if (FireManager.Instance.Enabled == false) return;

        var position = @params.Position;
        if (targetType != TargetTypes.positionAOE)
        {
            if (Voxel.voxelRayHitInfo.bHitValid)
            {
                var hitInfo = Voxel.voxelRayHitInfo;
                if (hitInfo == null) return;

                // modification from FuriousRamsay to fix issue where the lookray is behind a door, which is setting fire to blocks on the other side of metal doors.
                var itemAction = @params.Self.inventory.holdingItemData.item.Actions[0];
                if (itemAction is ItemActionMelee)
                {
                    float blockRange = 0;
                    if (itemAction.Properties.Values.ContainsKey("Block_range"))
                    {
                        blockRange = StringParsers.ParseFloat(itemAction.Properties.Values["Block_range"]);
                    }

                    if ((blockRange > 0) && (Vector3.Distance(hitInfo.hit.blockPos, @params.Position) > blockRange))
                    {
                        return;
                    }
                }

                position = hitInfo.hit.blockPos;
            }
        }

        AdvLogging.DisplayLog(AdvFeatureClass, $"Executing AddFireDamage() at {position}  Self: {@params.Self.position} Range: {maxRange}  Delay: {_delayTime}");
        Task.Delay((int) _delayTime)
            .ContinueWith(_ => AddFire(position));
    }

    private void AddFire(Vector3 position)
    {
        var range = (int) maxRange;
        for (var x = -range; x <= range; x++)
        {
            for (var z = -range; z <= range; z++)
            {
                for (var y = -range; y <= range; y++)
                {
                    var vector = new Vector3i(position.x + x, position.y + y, position.z + z);
                    FireManager.Instance.Add(vector);
                }
            }
        }
    }

    public override bool ParseXmlAttribute(XAttribute attribute)
    {
        var flag = base.ParseXmlAttribute(attribute);
        if (flag) return true;
        var name = attribute.Name.LocalName;
        if (name != "delayTime") return false;
        _delayTime = StringParsers.ParseFloat(attribute.Value);
        return true;
    }
}