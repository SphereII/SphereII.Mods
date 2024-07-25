public class DialogActionShowToolTipSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.xui.currentToolTip.ToolTip = ID;
    }
}
