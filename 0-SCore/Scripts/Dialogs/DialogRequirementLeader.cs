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

        // If value is "not" then we want to return true when the NPC doesn't have any leader,
        // not just when they have a leader and it's not the player.
        var playerIsLeader = leader?.entityId == player.entityId;
        return Value.EqualsCaseInsensitive("not") ? !playerIsLeader : playerIsLeader;
    }
}


