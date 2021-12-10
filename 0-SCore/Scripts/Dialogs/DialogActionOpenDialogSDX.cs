public class DialogActionOpenDialogSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.windowManager.Open("HireInformation", true, false, true);
    }
}

public class DialogActionOpenWindowSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.windowManager.Open(ID, true, false, true);
    }

}
