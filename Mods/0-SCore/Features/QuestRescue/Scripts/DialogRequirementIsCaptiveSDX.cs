/// <summary>
/// True if the NPC is a rescue quest captive that hasn't been freed yet. Putting "not" in the
/// value negates it, e.g. to hide the NPC's normal hire response while it is a captive.
/// <example>
/// <code>
/// &lt;requirement type="IsCaptiveSDX, SCore" requirementtype="Hide"/&gt;
/// &lt;requirement type="IsCaptiveSDX, SCore" requirementtype="Hide" value="not"/&gt;
/// </code>
/// </example>
/// </summary>
public class DialogRequirementIsCaptiveSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var isCaptive = IsCaptive(talkingTo);
        return Value.EqualsCaseInsensitive("not") ? !isCaptive : isCaptive;
    }

    public static bool IsCaptive(EntityAlive entity)
    {
        if (entity == null || entity.IsDead()) return false;
        if (entity.Buffs.GetCustomVar(RescueCaptiveManager.CaptiveCVar) <= 0f) return false;
        return !RescueCaptiveManager.IsFreed(entity);
    }
}
