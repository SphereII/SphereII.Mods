using UnityEngine;

/// <summary>
/// Turns the quest's POI into a permanent outpost: it's rebuilt from a variant prefab (the POI's
/// name plus poi_variant_suffix) and a settler NPC is stationed in it. See POIOutpostManager.
/// <para>
/// Use on_complete="true" so it runs when the quest is turned in. The rebuild waits until the POI
/// is loaded and no player is near it, so the player usually finds the outpost on their next visit.
/// </para>
/// <code>
/// &lt;action type="ConvertPOISDX, SCore"&gt;
///     &lt;property name="on_complete" value="true"/&gt;
///     &lt;property name="poi_variant_suffix" value="_outpost"/&gt;   &lt;!-- required --&gt;
///     &lt;property name="settler" value="npcOutpostTrader"/&gt;       &lt;!-- entity group or class --&gt;
///     &lt;property name="settler_block" value="outpostSettlerMarker"/&gt;
///     &lt;property name="settler_buff" value=""/&gt;
///     &lt;property name="settler_respawn" value="true"/&gt;
/// &lt;/action&gt;
/// </code>
/// </summary>
public class QuestActionConvertPOISDX : BaseQuestAction
{
    public const string PropSuffix = "poi_variant_suffix";
    public const string PropSettler = "settler";
    public const string PropSettlerBlock = "settler_block";
    public const string PropSettlerBuff = "settler_buff";
    public const string PropSettlerRespawn = "settler_respawn";

    private string suffix = string.Empty;
    private string settler = string.Empty;
    private string settlerBlock = string.Empty;
    private string settlerBuff = string.Empty;
    private bool settlerRespawn = true;

    public override void PerformAction(Quest ownerQuest)
    {
        var quest = ownerQuest ?? OwnerQuest;
        if (quest == null) return;

        if (!quest.GetPositionData(out var poiPosition, Quest.PositionDataTypes.POIPosition))
        {
            Log.Warning($"[SCore] POIOutposts: quest {quest.ID} has no POI; ConvertPOISDX needs a quest that goes to a POI");
            return;
        }

        var questTags = string.Join(",", quest.QuestTags.GetTagNames());

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            POIOutpostManager.RequestConversion(poiPosition, new POIOutpostManager.Outpost
            {
                Suffix = suffix,
                QuestId = quest.ID,
                QuestTags = questTags,
                Settler = settler,
                SettlerBlock = settlerBlock,
                SettlerBuff = settlerBuff,
                Respawn = settlerRespawn
            });
            return;
        }

        var playerId = quest.OwnerJournal?.OwnerPlayer?.entityId ?? -1;
        SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
            NetPackageManager.GetPackage<NetPackagePOIOutpostSDX>().Setup(playerId, poiPosition, suffix,
                quest.ID, questTags, settler, settlerBlock, settlerBuff, settlerRespawn));
    }

    public override BaseQuestAction Clone()
    {
        var action = new QuestActionConvertPOISDX();
        CopyValues(action);
        action.suffix = suffix;
        action.settler = settler;
        action.settlerBlock = settlerBlock;
        action.settlerBuff = settlerBuff;
        action.settlerRespawn = settlerRespawn;
        return action;
    }

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);
        properties.ParseString(PropSuffix, ref suffix);
        properties.ParseString(PropSettler, ref settler);
        properties.ParseString(PropSettlerBlock, ref settlerBlock);
        properties.ParseString(PropSettlerBuff, ref settlerBuff);
        properties.ParseBool(PropSettlerRespawn, ref settlerRespawn);

        suffix = suffix?.Trim() ?? string.Empty;
        settler = settler?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(suffix))
            Log.Error($"[SCore] POIOutposts: ConvertPOISDX in quest {Owner?.ID} needs a {PropSuffix}");
        else
            // The rebuild only sizes and cleans the POI for suffixes it knows about.
            QuestPOIVariantManager.RegisterSuffix(suffix);

        if (!OnComplete && Phase <= 1)
            Log.Warning($"[SCore] POIOutposts: ConvertPOISDX in quest {Owner?.ID} runs at the start of phase {Phase}. Add on_complete=\"true\" to convert the POI when the quest is turned in.");
    }
}

/// <summary>Client → server: a quest's ConvertPOISDX action ran on a client.</summary>
public class NetPackagePOIOutpostSDX : NetPackage
{
    private int playerId;
    private Vector3 poiPosition;
    private string suffix = string.Empty;
    private string questId = string.Empty;
    private string questTags = string.Empty;
    private string settler = string.Empty;
    private string settlerBlock = string.Empty;
    private string settlerBuff = string.Empty;
    private bool settlerRespawn = true;

    public NetPackagePOIOutpostSDX Setup(int _playerId, Vector3 _poiPosition, string _suffix, string _questId,
        string _questTags, string _settler, string _settlerBlock, string _settlerBuff, bool _settlerRespawn)
    {
        playerId = _playerId;
        poiPosition = _poiPosition;
        suffix = _suffix ?? string.Empty;
        questId = _questId ?? string.Empty;
        questTags = _questTags ?? string.Empty;
        settler = _settler ?? string.Empty;
        settlerBlock = _settlerBlock ?? string.Empty;
        settlerBuff = _settlerBuff ?? string.Empty;
        settlerRespawn = _settlerRespawn;
        return this;
    }

    public override NetPackageDirection PackageDirection => NetPackageDirection.ToServer;

    public override void ProcessPackage(World _world, GameManager _callbacks)
    {
        if (_world == null || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
        if (!ValidEntityIdForSender(playerId)) return;

        POIOutpostManager.RequestConversion(poiPosition, new POIOutpostManager.Outpost
        {
            Suffix = suffix,
            QuestId = questId,
            QuestTags = questTags,
            Settler = settler,
            SettlerBlock = settlerBlock,
            SettlerBuff = settlerBuff,
            Respawn = settlerRespawn
        });
    }

    public override void read(PooledBinaryReader _br)
    {
        playerId = _br.ReadInt32();
        poiPosition = StreamUtils.ReadVector3(_br);
        suffix = _br.ReadString();
        questId = _br.ReadString();
        questTags = _br.ReadString();
        settler = _br.ReadString();
        settlerBlock = _br.ReadString();
        settlerBuff = _br.ReadString();
        settlerRespawn = _br.ReadBoolean();
    }

    public override void write(PooledBinaryWriter _bw)
    {
        base.write(_bw);
        _bw.Write(playerId);
        StreamUtils.Write(_bw, poiPosition);
        _bw.Write(suffix ?? string.Empty);
        _bw.Write(questId ?? string.Empty);
        _bw.Write(questTags ?? string.Empty);
        _bw.Write(settler ?? string.Empty);
        _bw.Write(settlerBlock ?? string.Empty);
        _bw.Write(settlerBuff ?? string.Empty);
        _bw.Write(settlerRespawn);
    }
}
