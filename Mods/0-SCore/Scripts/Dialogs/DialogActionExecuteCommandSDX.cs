public class DialogActionExecuteCommandSDX : DialogActionAddBuff
{
    public override BaseDialogAction.ActionTypes ActionType => BaseDialogAction.ActionTypes.AddBuff;

    public override void PerformAction(EntityPlayer player)
    {
        var entityId = -1;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == -1)
            return;

        EntityUtilities.ExecuteCMD(entityId, base.ID, player);
    }

}
