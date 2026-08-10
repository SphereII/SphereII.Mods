public class DialogActionOpenDialogSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
        uiforPlayer.windowManager.Open("HireInformation", true, true);
    }
}

public class DialogActionOpenWindowSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
        uiforPlayer.windowManager.Open(ID, true, true);
    }

}
