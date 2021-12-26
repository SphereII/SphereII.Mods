public class DialogRequirementLeader : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0)
            return false;

        var leader = EntityUtilities.GetLeaderOrOwner(entityId);
        if (leader == null)
            return false;

        return leader.entityId == player.entityId;
    }
}


