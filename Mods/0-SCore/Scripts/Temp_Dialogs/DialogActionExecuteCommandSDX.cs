public class DialogActionExecuteCommandSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override ActionTypes ActionType => ActionTypes.AddBuff;

    public override void PerformAction(EntityPlayer player)
    {
        var entityID = -1;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int) player.Buffs.GetCustomVar("CurrentNPC");

        if (entityID == -1)
            return;

        EntityUtilities.ExecuteCMD(entityID, ID, player);
    }
}