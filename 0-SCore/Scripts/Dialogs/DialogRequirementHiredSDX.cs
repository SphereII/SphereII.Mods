public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0)
            return false;

        var myEntity = player.world.GetEntity(entityId) as EntityAliveSDX;
        if (myEntity == null) return false;

        var isTame = false;
        if (base.Value.EqualsCaseInsensitive("not"))
            isTame = !EntityUtilities.isTame(entityId, player);
        else
            isTame = EntityUtilities.isTame(entityId, player);
        return isTame;
    }
}


