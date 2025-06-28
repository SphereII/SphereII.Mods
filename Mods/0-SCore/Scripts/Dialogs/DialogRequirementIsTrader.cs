using System;public class DialogRequirementIsTrader : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        if ( talkingTo.NPCInfo.TraderID >0) return true;
        return false;
    }
}
