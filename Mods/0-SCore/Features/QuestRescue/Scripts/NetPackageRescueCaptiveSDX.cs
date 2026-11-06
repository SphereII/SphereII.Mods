using UnityEngine;

/// <summary>
/// Rescue quest messages.
/// <list type="bullet">
///   <item>Request (client → server): the objective asks for its POI's captive.</item>
///   <item>Assigned (server → that client): the captive's entity ID, or nothing if not ready.</item>
///   <item>Release (client → server): the quest ended before the captive was freed.</item>
///   <item>Freed (server → all clients): the captive was freed; complete the objective.</item>
///   <item>FreeRequest (client → server): a player freed the captive through dialog.</item>
///   <item>EscortStart / EscortEnd (client → server): an escort objective started or finished.</item>
///   <item>Leave (client → server): the escort finished with on_complete="leave".</item>
/// </list>
/// </summary>
public class NetPackageRescueCaptiveSDX : NetPackage
{
    public enum MessageTypes : byte
    {
        Request,
        Assigned,
        Release,
        Freed,
        FreeRequest,
        EscortStart,
        EscortEnd,
        Leave
    }

    private MessageTypes messageType;
    private int playerId;
    private int questCode;
    private Vector3 poiPosition;
    private string entityGroup = string.Empty;
    private string spawnBlocks = string.Empty;
    private string captiveBuff = string.Empty;
    private int captiveId = -1;
    private int attempt;
    private Vector3 captivePosition;

    public NetPackageRescueCaptiveSDX SetupRequest(int _playerId, int _questCode, Vector3 _poiPosition,
        string _entityGroup, string _spawnBlocks, string _captiveBuff, int _knownCaptiveId, int _attempt)
    {
        messageType = MessageTypes.Request;
        playerId = _playerId;
        questCode = _questCode;
        poiPosition = _poiPosition;
        entityGroup = _entityGroup ?? string.Empty;
        spawnBlocks = _spawnBlocks ?? string.Empty;
        captiveBuff = _captiveBuff ?? string.Empty;
        captiveId = _knownCaptiveId;
        attempt = _attempt;
        return this;
    }

    public NetPackageRescueCaptiveSDX SetupAssigned(int _playerId, int _questCode, int _captiveId, Vector3 _captivePosition)
    {
        messageType = MessageTypes.Assigned;
        playerId = _playerId;
        questCode = _questCode;
        captiveId = _captiveId;
        captivePosition = _captivePosition;
        return this;
    }

    public NetPackageRescueCaptiveSDX SetupRelease(int _playerId, Vector3 _poiPosition, int _captiveId)
    {
        messageType = MessageTypes.Release;
        playerId = _playerId;
        poiPosition = _poiPosition;
        captiveId = _captiveId;
        return this;
    }

    public NetPackageRescueCaptiveSDX SetupFreeRequest(int _playerId, int _captiveId)
    {
        messageType = MessageTypes.FreeRequest;
        playerId = _playerId;
        captiveId = _captiveId;
        return this;
    }

    public NetPackageRescueCaptiveSDX SetupEscort(int _playerId, int _captiveId, bool _escorting)
    {
        messageType = _escorting ? MessageTypes.EscortStart : MessageTypes.EscortEnd;
        playerId = _playerId;
        captiveId = _captiveId;
        return this;
    }

    /// <summary>The delay travels in the attempt field, in whole seconds.</summary>
    public NetPackageRescueCaptiveSDX SetupLeave(int _playerId, int _captiveId, int _delaySeconds)
    {
        messageType = MessageTypes.Leave;
        playerId = _playerId;
        captiveId = _captiveId;
        attempt = _delaySeconds;
        return this;
    }

    public NetPackageRescueCaptiveSDX SetupFreed(int _captiveId)
    {
        messageType = MessageTypes.Freed;
        captiveId = _captiveId;
        return this;
    }

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null) return;
        var isServer = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;

        switch (messageType)
        {
            case MessageTypes.Request:
                if (!isServer || !ValidEntityIdForSender(playerId)) return;
                var id = RescueCaptiveManager.RequestCaptive(poiPosition, entityGroup, spawnBlocks, captiveBuff,
                    captiveId, attempt, out var position);
                if (id == -1) return; // not ready; the client asks again
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupAssigned(playerId, questCode, id, position),
                    _onlyClientsAttachedToAnEntity: false, playerId);
                break;

            case MessageTypes.Release:
                if (!isServer || !ValidEntityIdForSender(playerId)) return;
                RescueCaptiveManager.ReleaseCaptive(poiPosition, captiveId);
                break;

            case MessageTypes.FreeRequest:
                if (!isServer || !ValidEntityIdForSender(playerId)) return;
                RescueCaptiveManager.FreeCaptive(captiveId, playerId);
                break;

            case MessageTypes.EscortStart:
            case MessageTypes.EscortEnd:
                if (!isServer || !ValidEntityIdForSender(playerId)) return;
                RescueCaptiveManager.SetEscort(captiveId, messageType == MessageTypes.EscortStart);
                break;

            case MessageTypes.Leave:
                if (!isServer || !ValidEntityIdForSender(playerId)) return;
                RescueCaptiveManager.LeaveAfterEscort(captiveId, attempt);
                break;

            case MessageTypes.Assigned:
                ObjectiveRescueNPCSDX.OnCaptiveAssigned(questCode, captiveId, captivePosition);
                break;

            case MessageTypes.Freed:
                ObjectiveRescueNPCSDX.OnCaptiveFreed(captiveId);
                break;
        }
    }

    public override void read(PooledBinaryReader _br)
    {
        messageType = (MessageTypes)_br.ReadByte();
        playerId = _br.ReadInt32();
        questCode = _br.ReadInt32();
        poiPosition = StreamUtils.ReadVector3(_br);
        entityGroup = _br.ReadString();
        spawnBlocks = _br.ReadString();
        captiveBuff = _br.ReadString();
        captiveId = _br.ReadInt32();
        attempt = _br.ReadInt32();
        captivePosition = StreamUtils.ReadVector3(_br);
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write((byte)messageType);
        _bw.Write(playerId);
        _bw.Write(questCode);
        StreamUtils.Write(_bw, poiPosition);
        _bw.Write(entityGroup ?? string.Empty);
        _bw.Write(spawnBlocks ?? string.Empty);
        _bw.Write(captiveBuff ?? string.Empty);
        _bw.Write(captiveId);
        _bw.Write(attempt);
        StreamUtils.Write(_bw, captivePosition);
    }
}
