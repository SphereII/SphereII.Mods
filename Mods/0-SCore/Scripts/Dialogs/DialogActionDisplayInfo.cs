using UnityEngine;

public class DialogActionDisplayInfo : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
        var myEntity = uiforPlayer.xui.Dialog.Respondent as IEntityAliveSDX;
        if (myEntity == null) return;
        var entityAlive = myEntity as EntityAlive;
        Log.Out($"Starting {myEntity.EntityName} Dump");
        Log.Out("Buffs:");
        foreach (var buff in entityAlive.Buffs.ActiveBuffs)
            Log.Out($"\t{buff.BuffName}");

        Log.Out("CVars:");
        foreach (var cvar in entityAlive.Buffs.CVars)
        {
            Log.Out($"\t{cvar.Key} :: {cvar.Value}");
        }
        Log.Out($"Done With {myEntity.EntityName}");
    }
}
