using UnityEngine;
public class DialogActionOpenDialogSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
         LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.windowManager.Open("HireInformation", true, false, true);
    }

    private string name = string.Empty;
}

public class DialogActionOpenWindowSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.windowManager.Open(ID, true, false, true);
    }

    private string name = string.Empty;
}
