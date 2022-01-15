using UnityEngine;

public class DialogActionAddBuffSDX : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        bool flag = !player.isEntityRemote;

        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var entityNPC = uiforPlayer.xui.Dialog.Respondent;
        if (entityNPC != null)
        {
            UnityEngine.Debug.Log($"Current Owner is: {entityNPC.EntityName}");

            // calls buffOrderDismiss, removes the cvar. Doesn't work
           var results= entityNPC.Buffs.AddBuff(base.ID, -1, flag && entityNPC.isEntityRemote, false, false);
            Debug.Log($"results: {results}");
            return;
        }

        if (!string.IsNullOrEmpty(Value) && Value.ToLower() == "self")
        {
          
        }
        player.Buffs.AddBuff(ID);
    }
}
