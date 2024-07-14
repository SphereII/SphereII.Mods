using UnityEngine;

public class DialogActionAddBuffSDX : BaseDialogAction
{
    public override void PerformAction(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        if (!string.IsNullOrEmpty(Value) && Value.ToLower() == "self")
        {
            var entityNPC = uiforPlayer.xui.Dialog.Respondent;
            if (entityNPC != null)
            {
                Debug.Log($"Adding {ID}...");
                // calls buffOrderDismiss, removes the cvar. Doesn't work
                entityNPC.Buffs.AddBuff(base.ID);
            }
            return;
        }
        player.Buffs.AddBuff(ID);
    }
}
