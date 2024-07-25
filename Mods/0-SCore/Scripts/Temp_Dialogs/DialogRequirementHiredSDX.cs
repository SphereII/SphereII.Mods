public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int) player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return false;

        var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (myEntity != null)
        {
                return EntityUtilities.isTame(entityID, player);
        }

        return false;
    }
}

public class DialogRequirementNotHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == 0)
            return false;

        var myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (myEntity != null)
        {
             return !EntityUtilities.isTame(entityID, player);
        }

        return false;
    }
}