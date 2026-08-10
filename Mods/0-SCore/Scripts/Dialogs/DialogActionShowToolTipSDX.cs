public class DialogActionShowToolTipSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
        uiforPlayer.xui.ToolTipWindow.ToolTip = ID;
    }
}
