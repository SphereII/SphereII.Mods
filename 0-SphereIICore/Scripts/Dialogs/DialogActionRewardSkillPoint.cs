using System;
using UnityEngine;
public class DialogActionRewardSkillPoint : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        player.Progression.SkillPoints += Convert.ToInt32(base.Value);
        GameManager.ShowTooltipWithAlert(player as EntityPlayerLocal, string.Format( "You have recieved {0} Skill Points", base.Value), "");
    }

    private string name = string.Empty;
}
