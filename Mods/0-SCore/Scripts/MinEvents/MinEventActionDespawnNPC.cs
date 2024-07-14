using System.Xml;

//        <triggered_effect trigger = "onSelfBuffUpdate" action="DespawnNPC, SCore" />
public class MinEventActionDespawnNPC : MinEventActionTargetedBase
{
    public override void Execute(MinEventParams _params)
    {
        var entity = _params.Self as EntityAlive;
        if (entity == null)
            return;

        GameManager.Instance.World.RemoveEntity(entity.entityId, EnumRemoveEntityReason.Unloaded);
    }
}