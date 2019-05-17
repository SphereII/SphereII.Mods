using UnityEngine;
public class DialogActionExecuteCommandSDX : DialogActionAddBuff
{
    public override BaseDialogAction.ActionTypes ActionType
    {
        get
        {
            return BaseDialogAction.ActionTypes.AddBuff;
        }
    }

    public override void PerformAction(EntityPlayer player)
    {
        int entityID = -1;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == -1)
            return;

        EntityUtilities.ExecuteCMD(entityID, base.ID, player );
    }

    private string name = string.Empty;
}
