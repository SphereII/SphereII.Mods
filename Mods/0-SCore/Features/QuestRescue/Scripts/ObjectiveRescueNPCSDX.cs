using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Rescue objective: spawns a captive NPC in the quest's POI and completes when the captive is
/// freed (has a leader). Fails the quest if the captive dies first.
/// <para>
/// Put it in the same phase as ClearSleepers, after the RallyPoint. The captive is spawned once
/// the rally point's reset has rebuilt the POI.
/// </para>
/// <code>
/// &lt;objective type="RescueNPCSDX, SCore"&gt;
///     &lt;property name="phase" value="3"/&gt;
///     &lt;property name="entity_group" value="npcCaptiveGroup"/&gt;   &lt;!-- entity group or entity class --&gt;
///     &lt;property name="spawn_block" value="captiveSpawnMarker"/&gt; &lt;!-- optional, comma list --&gt;
///     &lt;property name="captive_buff" value="buffCaptive"/&gt;       &lt;!-- optional, applied at spawn --&gt;
///     &lt;property name="nav_object" value="quest"/&gt;
/// &lt;/objective&gt;
/// </code>
/// </summary>
public class ObjectiveRescueNPCSDX : BaseObjective
{
    public const string PropEntityGroup = "entity_group";
    public const string PropSpawnBlock = "spawn_block";
    public const string PropCaptiveBuff = "captive_buff";
    public const string PropLeaveDelay = "leave_delay";

    /// <summary>
    /// Quest data variable holding the captive's entity ID, so it survives a restart. The escort
    /// objective reads it too.
    /// </summary>
    public const string CaptiveIdVariable = "rescue_captive_id";

    /// <summary>Seconds between spawn requests while the server isn't ready.</summary>
    private const int RequestInterval = 3;

    // Objectives listening for server replies. Keyed by nothing: there are only ever a few.
    private static readonly List<ObjectiveRescueNPCSDX> Active = new List<ObjectiveRescueNPCSDX>();

    private string entityGroup = string.Empty;
    private string spawnBlocks = string.Empty;
    private string captiveBuff = string.Empty;

    // Without an EscortNPCSDX, the freed captive leaves when the quest ends, and is removed this
    // many seconds after it's dismissed.
    private float leaveDelay = 10f;

    private int captiveId = -1;
    private int requestAttempts;
    private int secondsUntilRequest;
    private NavObject navObject;
    private bool navTracksEntity;

    public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

    public override bool UpdateUI => ObjectiveState != ObjectiveStates.Failed;

    public override void SetupObjective()
    {
        keyword = Localization.Get("ObjectiveRescueNPCSDX_keyword");
    }

    public override void SetupDisplay()
    {
        Description = keyword;
        StatusText = "";
    }

    public override void AddHooks()
    {
        if (OwnerQuest.DataVariables.TryGetValue(CaptiveIdVariable, out var stored))
            int.TryParse(stored, NumberStyles.Integer, CultureInfo.InvariantCulture, out captiveId);

        if (!Active.Contains(this)) Active.Add(this);
        QuestEventManager.Current.AddObjectiveToBeUpdated(this);
    }

    public override void RemoveHooks()
    {
        Active.Remove(this);
        QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
        RemoveCaptiveNavObject();
    }

    public override void Refresh()
    {
        if (Complete)
            OwnerQuest.RefreshQuestCompletion();
    }

    /// <summary>
    /// The quest was completed. Without an escort objective to decide, the freed captive doesn't
    /// stay hired: it leaves.
    /// </summary>
    public override void HandleCompleted()
    {
        if (Complete && !HasEscort())
            RescueCaptiveManager.RequestLeave(OwnerQuest.OwnerJournal.OwnerPlayer.entityId, captiveId, leaveDelay);
    }

    /// <summary>
    /// The quest failed or was abandoned. An unfreed captive is removed; a freed one leaves (unless
    /// an escort objective handles it).
    /// </summary>
    public override void HandleFailed()
    {
        if (captiveId <= 0) return;

        if (Complete)
        {
            if (!HasEscort())
                RescueCaptiveManager.RequestLeave(OwnerQuest.OwnerJournal.OwnerPlayer.entityId, captiveId, leaveDelay);
            return;
        }

        if (!OwnerQuest.GetPositionData(out var poiPosition, Quest.PositionDataTypes.POIPosition)) return;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            RescueCaptiveManager.ReleaseCaptive(poiPosition, captiveId);
        else
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupRelease(
                    OwnerQuest.OwnerJournal.OwnerPlayer.entityId, poiPosition, captiveId));
    }

    // -------------------------------------------------------------------------
    // Update states (BaseObjective.Update runs these once a second, in this objective's phase)
    // -------------------------------------------------------------------------

    public override void UpdateState_NeedSetup()
    {
        requestAttempts = 0;
        secondsUntilRequest = 0;
        CurrentValue = (byte)UpdateStates.WaitingForServer;
        SendRequest();
    }

    public override void UpdateState_WaitingForServer()
    {
        if (--secondsUntilRequest > 0) return;
        SendRequest();
    }

    public override void UpdateState_Update()
    {
        var captive = GameManager.Instance.World.GetEntity(captiveId) as EntityAlive;
        if (captive == null) return; // not loaded on this client yet

        // Swap the position marker for one that follows the captive.
        if (navObject != null && !navTracksEntity)
            AddCaptiveNavObject(captive, captive.position);

        if (captive.IsDead())
        {
            GameManager.ShowTooltip(OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("ObjectiveRescueNPCSDX_died"));
            ObjectiveState = ObjectiveStates.Failed;
            OwnerQuest.CloseQuest(Quest.QuestState.Failed);
            return;
        }

        if (RescueCaptiveManager.IsFreed(captive))
            MarkFreed();
    }

    // -------------------------------------------------------------------------
    // Server replies
    // -------------------------------------------------------------------------

    private void SendRequest()
    {
        secondsUntilRequest = RequestInterval;
        if (!OwnerQuest.GetPositionData(out var poiPosition, Quest.PositionDataTypes.POIPosition)) return;

        requestAttempts++;
        var player = OwnerQuest.OwnerJournal.OwnerPlayer;
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            var id = RescueCaptiveManager.RequestCaptive(poiPosition, entityGroup, spawnBlocks, captiveBuff,
                captiveId, requestAttempts, out var position);
            if (id != -1) Assign(id, position);
            return;
        }

        SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
            NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupRequest(
                player.entityId, OwnerQuest.QuestCode, poiPosition, entityGroup, spawnBlocks, captiveBuff,
                captiveId, requestAttempts));
    }

    public static void OnCaptiveAssigned(int questCode, int captiveId, Vector3 position)
    {
        foreach (var objective in Active.ToArray())
            if (objective.OwnerQuest.QuestCode == questCode)
                objective.Assign(captiveId, position);
    }

    public static void OnCaptiveFreed(int captiveId)
    {
        foreach (var objective in Active.ToArray())
            if (objective.captiveId == captiveId)
                objective.MarkFreed();
    }

    private void Assign(int id, Vector3 position)
    {
        if (CurrentValue != (byte)UpdateStates.WaitingForServer) return;

        captiveId = id;
        OwnerQuest.DataVariables[CaptiveIdVariable] = id.ToString(CultureInfo.InvariantCulture);
        CurrentValue = (byte)UpdateStates.Update;

        var captive = GameManager.Instance.World.GetEntity(id) as EntityAlive;
        AddCaptiveNavObject(captive, position);
    }

    private bool HasEscort()
    {
        foreach (var objective in OwnerQuest.Objectives)
            if (objective is ObjectiveEscortNPCSDX) return true;
        return false;
    }

    private void MarkFreed()
    {
        if (Complete) return;

        RemoveCaptiveNavObject();
        Complete = true;
        CurrentValue = (byte)UpdateStates.Completed;
        OwnerQuest.RefreshQuestCompletion();
    }

    // -------------------------------------------------------------------------
    // Nav object
    // -------------------------------------------------------------------------

    private void AddCaptiveNavObject(EntityAlive captive, Vector3 position)
    {
        if (string.IsNullOrEmpty(NavObjectName)) return;

        RemoveCaptiveNavObject();
        if (captive != null)
        {
            navObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, captive);
            navTracksEntity = true;
        }
        else
        {
            navObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, position);
            navTracksEntity = false;
        }
    }

    private void RemoveCaptiveNavObject()
    {
        if (navObject == null) return;
        NavObjectManager.Instance.UnRegisterNavObject(navObject);
        navObject = null;
        navTracksEntity = false;
    }

    // -------------------------------------------------------------------------
    // Properties / cloning
    // -------------------------------------------------------------------------

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);
        properties.ParseString(PropEntityGroup, ref entityGroup);
        properties.ParseString(PropSpawnBlock, ref spawnBlocks);
        properties.ParseString(PropCaptiveBuff, ref captiveBuff);
        properties.ParseFloat(PropLeaveDelay, ref leaveDelay);
        if (leaveDelay < 0f) leaveDelay = 0f;

        if (string.IsNullOrEmpty(entityGroup))
            Log.Error($"[SCore] RescueNPCSDX in quest {OwnerQuestClass?.ID}: '{PropEntityGroup}' is required");
    }

    public override BaseObjective Clone()
    {
        var objective = new ObjectiveRescueNPCSDX();
        CopyValues(objective);
        return objective;
    }

    public override void CopyValues(BaseObjective objective)
    {
        base.CopyValues(objective);
        var rescue = (ObjectiveRescueNPCSDX)objective;
        rescue.entityGroup = entityGroup;
        rescue.spawnBlocks = spawnBlocks;
        rescue.captiveBuff = captiveBuff;
        rescue.leaveDelay = leaveDelay;
    }

    public override void InternalToParametersDictionary(ref Dictionary<string, object> parameters)
    {
        parameters.Add("entity_group", entityGroup);
        parameters.Add("captive_id", captiveId);
    }
}
