using UnityEngine;

public class DialogActionDisplayInfo : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = uiforPlayer.xui.Dialog.Respondent as EntityAliveSDX;
        if (myEntity == null) return;
        Log.Out($"Starting {myEntity.EntityName} Dump");
        Log.Out("Buffs:");
        foreach( var buff in myEntity.Buffs.ActiveBuffs)
            Log.Out($"\t{buff.BuffName}");
        
        Log.Out("CVars:");
        foreach (var cvar in myEntity.Buffs.CVars)
        {
            Log.Out($"\t{cvar.Key} :: {cvar.Value}");
        }
        Log.Out($"Done With {myEntity.EntityName}");
    }
}
