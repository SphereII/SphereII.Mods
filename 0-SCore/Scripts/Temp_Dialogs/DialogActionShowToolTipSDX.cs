public class DialogActionShowToolTipSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.xui.currentToolTip.ToolTip = ID;
    }
}