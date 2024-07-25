public class DialogActionRemoveBuffNPCSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as EntityAliveSDX;
        if (myEntity == null)
        {
            return;
        }

        myEntity.Buffs.RemoveBuff(base.ID, true);
    }
}
