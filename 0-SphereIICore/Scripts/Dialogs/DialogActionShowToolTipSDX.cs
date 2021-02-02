﻿public class DialogActionShowToolTipSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        uiforPlayer.xui.currentToolTip.ToolTip = ID;
    }

    private readonly string name = string.Empty;
}
