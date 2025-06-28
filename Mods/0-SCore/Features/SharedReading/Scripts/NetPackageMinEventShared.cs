using UnityEngine;

public class NetPackageMinEventSharedReading : NetPackage
{
    public MinEventTypes EventType;

    private int selfEntityID;
    private int otherEntityID;
    private ItemValue itemValue;
    public NetPackageMinEventSharedReading Setup(int selfEntityID, int sourceEntityId, MinEventTypes eventType, ItemValue itemValue)
    {
        this.selfEntityID = selfEntityID;
        this.itemValue = itemValue;
        EventType = eventType;
        otherEntityID = sourceEntityId;
        return this;
    }

    public override void read(PooledBinaryReader _br)
    {
        selfEntityID = _br.ReadInt32();
        otherEntityID = _br.ReadInt32();
        itemValue = new ItemValue();
        itemValue.Read(_br);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(selfEntityID);
        _bw.Write(otherEntityID);
        itemValue.Write(_bw);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }
        
        var entityAlive = _world.GetEntity(selfEntityID) as EntityPlayer;
        if (entityAlive == null) return;
        
        var readingPlayer = _world.GetEntity(otherEntityID) as EntityPlayer;
        if (readingPlayer == null) return;


        if (ConnectionManager.Instance.IsServer)
        {
            foreach (var member in readingPlayer.Party.MemberList)
            {
                if (readingPlayer.entityId == member.entityId) continue;
                var package = NetPackageManager.GetPackage<NetPackageMinEventSharedReading>();
                package.Setup(member.entityId, readingPlayer.entityId, MinEventTypes.onSelfSecondaryActionEnd, itemValue);
                ConnectionManager.Instance.SendPackage(package);
            }
            return;
        }

        if (readingPlayer is EntityPlayerLocal localplayer) return;
        
        entityAlive.MinEventContext.Self = entityAlive;
        entityAlive.MinEventContext.ItemValue = itemValue;
        entityAlive.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, entityAlive.MinEventContext);

        
        var unlock = itemValue.ItemClass.Properties.GetString("Unlocks");
        unlock = SCoreLocalizationHelper.GetLocalization(unlock);
        var toolTipDisplay = $"{Localization.Get("sharedReadingDesc")} {readingPlayer.EntityName} :: {unlock}";
        GameManager.ShowTooltip(entityAlive as EntityPlayerLocal, toolTipDisplay);
    }
    public override int GetLength()
    {
        return 32;
    }

}