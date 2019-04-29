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
        int entityID = 0;
        if(player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if(entityID == 0)
            return;
        EntityAliveSDX myEntity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if(myEntity != null)
                myEntity.ExecuteCMD(base.ID, player);
    }

    private string name = string.Empty;
}
