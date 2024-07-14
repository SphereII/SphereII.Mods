public class DialogActionOpenDialogSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.windowManager.Open("HireInformation", true);
    }
}

public class DialogActionOpenWindowSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.windowManager.Open(ID, true);
    }
}