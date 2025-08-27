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
        // This method should only run on the server
        if (!ConnectionManager.Instance.IsServer)
        {
            // If it's a client, apply the effect and display the tooltip
            var entityAlive = _world.GetEntity(selfEntityID) as EntityPlayerLocal;
            if (entityAlive == null) return;

            var readingPlayer = _world.GetEntity(otherEntityID) as EntityPlayer;
            if (readingPlayer == null) return;

            ApplyMinEffect(entityAlive);
            DisplayTooltip(entityAlive, readingPlayer);
            return;
        }

        // Server-side logic to broadcast the message to all party members except the sender
        var readingPlayerServer = _world.GetEntity(selfEntityID) as EntityPlayer;
        if (readingPlayerServer?.Party == null) return;

        foreach (var member in readingPlayerServer.Party.MemberList)
        {
            // Skip the player who initiated the action (selfEntityID)
            if (otherEntityID == member.entityId) continue;

            if (member is EntityPlayerLocal localplayer)
            {
                ApplyMinEffect(localplayer);
                DisplayTooltip(localplayer, readingPlayerServer);
                continue;
            }
            var package = NetPackageManager.GetPackage<NetPackageMinEventSharedReading>();
            package.Setup(member.entityId, selfEntityID, MinEventTypes.onSelfSecondaryActionEnd, itemValue);
            ConnectionManager.Instance.SendPackage(package);
        }
    }


    public void DisplayTooltip(EntityPlayerLocal entityAlive, EntityAlive readingPlayer)
    {
        var unlock = itemValue.ItemClass.Properties.GetString("Unlocks");
        unlock = SCoreLocalizationHelper.GetLocalization(unlock);
        var toolTipDisplay = $"{Localization.Get("sharedReadingDesc")} {readingPlayer.EntityName} :: {unlock}";
        GameManager.ShowTooltip(entityAlive, toolTipDisplay);

    }
    public virtual void ApplyMinEffect(EntityPlayer entityAlive)
    {
        entityAlive.MinEventContext.Self = entityAlive;
        entityAlive.MinEventContext.ItemValue = itemValue;
        entityAlive.MinEventContext.ItemValue.FireEvent(MinEventTypes.onSelfPrimaryActionEnd, entityAlive.MinEventContext);

    }

    public override int GetLength()
    {
        return 32;
    }

}