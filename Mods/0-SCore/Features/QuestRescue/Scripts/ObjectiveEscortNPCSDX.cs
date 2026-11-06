using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// Escort objective: the captive freed by RescueNPCSDX must get to safety alive; fails the quest if
/// the captive dies first. Where "safety" is depends on <c>escort_to</c>:
/// <list type="bullet">
///   <item>quest_giver: within <c>distance</c> blocks of the quest giver. A quest with no quest
///   giver (givequest) completes once the captive is out of the POI instead.</item>
///   <item>poi: <c>distance</c> blocks outside the POI's bounds.</item>
/// </list>
/// The default follows <c>on_complete</c>: a captive that leaves only needs to get out of the POI,
/// one that joins walks back to the quest giver with the player.
/// <para>
/// <c>on_complete</c> defaults to <c>leave</c>: a captive only stays hired when the quest opts in
/// with <c>join</c> and is completed. If the quest fails or is abandoned during the escort, the
/// captive leaves either way.
/// </para>
/// <para>
/// Put it in the turn-in phase, next to ReturnToNPC and InteractWithNPC. While it runs, the
/// captive has the RescueEscort cvar, which the NPCModRescueEscort AI package uses to keep it
/// following through fights.
/// </para>
/// <code>
/// &lt;objective type="EscortNPCSDX, SCore"&gt;
///     &lt;property name="phase" value="4"/&gt;
///     &lt;property name="distance" value="10"/&gt;     &lt;!-- optional, default 10 --&gt;
///     &lt;property name="nav_object" value="quest"/&gt; &lt;!-- optional marker on the captive --&gt;
///     &lt;property name="on_complete" value="leave"/&gt; &lt;!-- leave (default) or join --&gt;
///     &lt;property name="leave_delay" value="10"/&gt;    &lt;!-- leave: seconds before it's removed --&gt;
///     &lt;property name="escort_to" value="poi"/&gt;      &lt;!-- poi or quest_giver; default by on_complete --&gt;
/// &lt;/objective&gt;
/// </code>
/// </summary>
public class ObjectiveEscortNPCSDX : BaseObjective
{
    public const string PropDistance = "distance";
    public const string PropOnComplete = "on_complete";
    public const string PropLeaveDelay = "leave_delay";
    public const string PropEscortTo = "escort_to";

    private float distance = 10f;

    // leave (default): the captive is dismissed when the escort completes, wanders off, and is
    // removed after leaveDelay seconds. join: it stays hired by the player who freed it.
    private bool leaveOnComplete = true;
    private float leaveDelay = 10f;

    // true: done once the captive is out of the POI. false: done at the quest giver.
    private bool escortToPoi;

    private int captiveId = -1;
    private NavObject navObject;
    private bool escortStarted;

    public override ObjectiveValueTypes ObjectiveValueType => ObjectiveValueTypes.Boolean;

    public override bool UpdateUI => ObjectiveState != ObjectiveStates.Failed;

    public override void SetupObjective()
    {
        keyword = Localization.Get("ObjectiveEscortNPCSDX_keyword");
    }

    public override void SetupDisplay()
    {
        Description = keyword;
        StatusText = "";
    }

    public override void AddHooks()
    {
        captiveId = -1;
        if (OwnerQuest.DataVariables.TryGetValue(ObjectiveRescueNPCSDX.CaptiveIdVariable, out var stored))
            int.TryParse(stored, NumberStyles.Integer, CultureInfo.InvariantCulture, out captiveId);

        QuestEventManager.Current.AddObjectiveToBeUpdated(this);
    }

    public override void RemoveHooks()
    {
        QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
        RemoveCaptiveNavObject();
    }

    public override void Refresh()
    {
        if (Complete)
            OwnerQuest.RefreshQuestCompletion();
    }

    public override void HandleCompleted() => EndEscort();

    /// <summary>
    /// The quest failed or was abandoned mid-escort. Whatever on_complete says, a captive only stays
    /// hired through a completed quest, so it leaves. (The server ignores a dead captive.)
    /// </summary>
    public override void HandleFailed()
    {
        EndEscort();
        SendLeave();
    }

    // -------------------------------------------------------------------------
    // Update states (once a second, in this objective's phase)
    // -------------------------------------------------------------------------

    public override void UpdateState_NeedSetup()
    {
        if (captiveId <= 0)
        {
            // Without a RescueNPCSDX earlier in the quest there's nothing to escort. Don't block
            // the quest on a config mistake.
            Log.Error($"[SCore] EscortNPCSDX in quest {OwnerQuestClass?.ID}: no captive. Add a RescueNPCSDX objective in an earlier phase.");
            MarkComplete();
            return;
        }

        SetEscort(true);
        CurrentValue = (byte)UpdateStates.Update;
    }

    public override void UpdateState_Update()
    {
        var captive = GameManager.Instance.World.GetEntity(captiveId) as EntityAlive;
        if (captive == null) return; // not loaded on this client yet

        // After a restart the saved state skips NeedSetup; claim the escort again so it gets
        // cleared when this objective ends.
        if (!escortStarted)
            SetEscort(true);

        if (navObject == null)
            AddCaptiveNavObject(captive);

        if (captive.IsDead())
        {
            GameManager.ShowTooltip(OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("ObjectiveRescueNPCSDX_died"));
            ObjectiveState = ObjectiveStates.Failed;
            OwnerQuest.CloseQuest(Quest.QuestState.Failed);
            return;
        }

        if (escortToPoi)
        {
            if (IsOutsidePOI(captive.position))
                MarkComplete();
            return;
        }

        if (TryGetDestination(out var destination))
        {
            var offset = captive.position - destination;
            offset.y = 0f;
            if (offset.sqrMagnitude <= distance * distance)
                MarkComplete();
            return;
        }

        // No quest giver (e.g. the quest came from the givequest console command): getting the
        // captive out of the POI is enough.
        if (IsOutsidePOI(captive.position))
            MarkComplete();
    }

    // -------------------------------------------------------------------------

    private bool TryGetDestination(out Vector3 destination)
    {
        if (OwnerQuest.GetPositionData(out destination, Quest.PositionDataTypes.QuestGiver))
            return true;

        if (GameManager.Instance.World.GetEntity(OwnerQuest.QuestGiverID) is Entity giver)
        {
            destination = giver.position;
            return true;
        }

        return false;
    }

    /// <summary>True once the position is more than <c>distance</c> blocks outside the POI's bounds.</summary>
    private bool IsOutsidePOI(Vector3 position)
    {
        if (!OwnerQuest.GetPositionData(out var poiPosition, Quest.PositionDataTypes.POIPosition)) return false;
        if (!OwnerQuest.GetPositionData(out var poiSize, Quest.PositionDataTypes.POISize)) return false;

        var area = new Rect(poiPosition.x - distance, poiPosition.z - distance,
            poiSize.x + distance * 2f, poiSize.z + distance * 2f);
        return !area.Contains(new Vector2(position.x, position.z));
    }

    private void MarkComplete()
    {
        if (Complete) return;

        RemoveCaptiveNavObject();
        EndEscort();
        if (leaveOnComplete) SendLeave();
        Complete = true;
        CurrentValue = (byte)UpdateStates.Completed;
        OwnerQuest.RefreshQuestCompletion();
    }

    private void SendLeave()
    {
        RescueCaptiveManager.RequestLeave(OwnerQuest.OwnerJournal.OwnerPlayer.entityId, captiveId, leaveDelay);
    }

    private void EndEscort()
    {
        if (escortStarted) SetEscort(false);
    }

    private void SetEscort(bool escorting)
    {
        escortStarted = escorting;
        if (captiveId <= 0) return;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            RescueCaptiveManager.SetEscort(captiveId, escorting);
        else
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupEscort(
                    OwnerQuest.OwnerJournal.OwnerPlayer.entityId, captiveId, escorting));
    }

    private void AddCaptiveNavObject(EntityAlive captive)
    {
        if (string.IsNullOrEmpty(NavObjectName)) return;
        navObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, captive);
    }

    private void RemoveCaptiveNavObject()
    {
        if (navObject == null) return;
        NavObjectManager.Instance.UnRegisterNavObject(navObject);
        navObject = null;
    }

    // -------------------------------------------------------------------------
    // Properties / cloning
    // -------------------------------------------------------------------------

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);
        properties.ParseFloat(PropDistance, ref distance);
        properties.ParseFloat(PropLeaveDelay, ref leaveDelay);
        if (leaveDelay < 0f) leaveDelay = 0f;

        var onComplete = "leave";
        properties.ParseString(PropOnComplete, ref onComplete);
        onComplete = onComplete?.Trim() ?? "leave";
        if (onComplete.EqualsCaseInsensitive("join"))
            leaveOnComplete = false;
        else if (!onComplete.EqualsCaseInsensitive("leave"))
            Log.Error($"[SCore] EscortNPCSDX in quest {OwnerQuestClass?.ID}: {PropOnComplete} must be 'leave' or 'join', not '{onComplete}'. Using 'leave'.");

        // A captive that leaves is done once it's out of the POI; one that joins comes back with
        // the player.
        escortToPoi = leaveOnComplete;
        var escortTo = "";
        properties.ParseString(PropEscortTo, ref escortTo);
        escortTo = escortTo?.Trim() ?? "";
        if (escortTo.EqualsCaseInsensitive("poi"))
            escortToPoi = true;
        else if (escortTo.EqualsCaseInsensitive("quest_giver"))
            escortToPoi = false;
        else if (escortTo.Length > 0)
            Log.Error($"[SCore] EscortNPCSDX in quest {OwnerQuestClass?.ID}: {PropEscortTo} must be 'poi' or 'quest_giver', not '{escortTo}'. Using '{(escortToPoi ? "poi" : "quest_giver")}'.");
    }

    public override BaseObjective Clone()
    {
        var objective = new ObjectiveEscortNPCSDX();
        CopyValues(objective);
        return objective;
    }

    public override void CopyValues(BaseObjective objective)
    {
        base.CopyValues(objective);
        var escort = (ObjectiveEscortNPCSDX)objective;
        escort.distance = distance;
        escort.leaveOnComplete = leaveOnComplete;
        escort.leaveDelay = leaveDelay;
        escort.escortToPoi = escortToPoi;
    }

    public override void InternalToParametersDictionary(ref Dictionary<string, object> parameters)
    {
        parameters.Add("distance", distance);
        parameters.Add("on_complete", leaveOnComplete ? "leave" : "join");
        parameters.Add("escort_to", escortToPoi ? "poi" : "quest_giver");
        parameters.Add("captive_id", captiveId);
    }
}
