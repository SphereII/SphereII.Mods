using UnityEngine;
public class DialogActionOpenDialogSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
         LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        Debug.Log(" Respondent: " + LocalPlayerUI.primaryUI.xui.Dialog.Respondent);
        uiforPlayer.windowManager.Open("HireInformation", true, false, true);
        Debug.Log(" Respondent2: " + LocalPlayerUI.primaryUI.xui.Dialog.Respondent);
    }

    private string name = string.Empty;
}
