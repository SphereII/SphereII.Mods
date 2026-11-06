/// <summary>
/// Frees a rescue quest captive: the NPC the player is talking to joins them (leader and owner,
/// Follow order) at no cost, its captive buff is removed, and the rescue objective completes for
/// everyone on the quest.
/// <para>
/// Pair it with the IsCaptiveSDX requirement so the response only shows for captives.
/// </para>
/// <example>
/// <code>
/// &lt;response id="free" text_key="dialog_free_captive" nextstatementid="done"&gt;
///     &lt;requirement type="IsCaptiveSDX, SCore" requirementtype="Hide"/&gt;
///     &lt;action type="FreeCaptiveSDX, SCore"/&gt;
/// &lt;/response&gt;
/// </code>
/// </example>
/// </summary>
public class DialogActionFreeCaptiveSDX : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
        if (!(playerUI?.xui?.Dialog?.Respondent is EntityAlive captive)) return;
        if (!DialogRequirementIsCaptiveSDX.IsCaptive(captive)) return;

        // Set the leader here first, like the Hire command: the freeing player's machine is the
        // one whose cvar changes are sent to the server.
        EntityUtilities.SetLeaderAndOwner(captive.entityId, player.entityId);
        EntityUtilities.CheckForDanglingHires(player.entityId);

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            RescueCaptiveManager.FreeCaptive(captive.entityId, player.entityId);
        else
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRescueCaptiveSDX>().SetupFreeRequest(player.entityId, captive.entityId));
    }
}
