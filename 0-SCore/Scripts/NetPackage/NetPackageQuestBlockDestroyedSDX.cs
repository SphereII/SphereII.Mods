using System;


public class NetPackageQuestBlockDestroyedSDX : NetPackage
{
    public NetPackageQuestBlockDestroyedSDX Setup(int _entityID, int _questCode)
    {
        this.senderEntityID = _entityID;
        this.questCode = _questCode;
        this.blockPos = Vector3i.zero;
        return this;
    }
    public NetPackageQuestBlockDestroyedSDX Setup(int _entityID, int _questCode, Vector3i _blockPos)
    {
        this.senderEntityID = _entityID;
        this.questCode = _questCode;
        this.blockPos = _blockPos;
        return this;
    }

    public override void read(PooledBinaryReader _reader)
    {
        this.senderEntityID = _reader.ReadInt32();
        this.questCode = _reader.ReadInt32();
        this.blockPos = StreamUtils.ReadVector3i(_reader);
    }

    public override void write(PooledBinaryWriter _writer)
    {
        base.write(_writer);
        _writer.Write(this.senderEntityID);
        _writer.Write(this.questCode);
        StreamUtils.Write(_writer, this.blockPos);
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null)
        {
            return;
        }
        if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
            this.HandlePlayer(_world, primaryPlayer);
            return;
        }
        EntityPlayer entityPlayer = _world.GetEntity(this.senderEntityID) as EntityPlayer;
        if (entityPlayer == null || entityPlayer.Party == null)
        {
            return;
        }
        for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
        {
            EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[i];
            if (entityPlayer2 != entityPlayer)
            {
                if (entityPlayer2 is EntityPlayerLocal)
                {
                    this.HandlePlayer(_world, entityPlayer2 as EntityPlayerLocal);
                }
                else
                {
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestBlockDestroyedSDX>().Setup(this.senderEntityID, this.questCode), false, entityPlayer2.entityId, -1, -1, -1);
                }
            }
        }
        return;

    }

    private void HandlePlayer(World _world, EntityPlayerLocal localPlayer)
    {
        EntityPlayer entityPlayer = _world.GetEntity(this.senderEntityID) as EntityPlayer;
        Quest quest = localPlayer.QuestJournal.FindActiveQuest(this.questCode);
        if (quest != null && entityPlayer.GetDistance(localPlayer) < 50f)
        {
            for (int i = 0; i < quest.Objectives.Count; i++)
            {
                if (!quest.Objectives[i].Complete && quest.Objectives[i] is ObjectiveBlockDestroySDX)
                {
                    (quest.Objectives[i] as ObjectiveBlockDestroySDX).AddDestroyedBlock();
                    return;
                }
            }
            return;
        }
    }

    public override int GetLength()
    {
        return 20;
    }

    private int senderEntityID;
    private int questCode;
    private Vector3i blockPos;

}
