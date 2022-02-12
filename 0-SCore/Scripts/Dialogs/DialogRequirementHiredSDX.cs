public class DialogRequirementHiredSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var entityId = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == 0)
            return false;

        var currentNPC = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
        if (currentNPC == null) return false;

        var isTame = false;
        
        if ( currentNPC.Buffs.HasCustomVar("Leader") && currentNPC.Buffs.GetCustomVar("Leader") > 0 )
            isTame = true;

        if (currentNPC.Buffs.HasCustomVar("Owner") && currentNPC.Buffs.GetCustomVar("Owner") > 0)
            isTame = true;

        if (base.Value.EqualsCaseInsensitive("not"))
            return !isTame;

        return isTame;
    }
}


