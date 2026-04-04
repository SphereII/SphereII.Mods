public class DialogActionRemoveBuffNPCSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var playerUI = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = playerUI.xui.Dialog.Respondent as IEntityAliveSDX;
        if (myEntity == null) return;

        (myEntity as EntityAlive).Buffs.RemoveBuff(base.ID, true);
    }
}
