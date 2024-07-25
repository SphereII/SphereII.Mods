public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int) player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0)
            return false;

        var currentNpc = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
        if (currentNpc == null) return false;

        /*
        // Do they have a tag that prevents them from being hired?
        if (currentNpc.HasAnyTags(FastTags.Parse("nohire"))) return false;

        if (currentNpc.Buffs.HasCustomVar("nohire") && currentNpc.Buffs.GetCustomVar("nohire") > 0) return false;
*/
        // Do they already have a leader?
        var isTame = currentNpc.Buffs.HasCustomVar("Leader") && currentNpc.Buffs.GetCustomVar("Leader") > 0;
        if (currentNpc.Buffs.HasCustomVar("Owner") && currentNpc.Buffs.GetCustomVar("Owner") > 0)
            isTame = true;

        if (base.Value.EqualsCaseInsensitive("not"))
            return !isTame;

        return isTame;
    }
}