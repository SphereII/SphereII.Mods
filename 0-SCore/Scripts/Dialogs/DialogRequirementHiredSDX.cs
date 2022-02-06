public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0)
            return false;

        var isTame = true;
        if ( EntityUtilities.GetLeaderOrOwner(entityId) == null )
            isTame = false;

        if (base.Value.EqualsCaseInsensitive("not"))
            return !isTame;

        return isTame;
    }
}


