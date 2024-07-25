using System;

public class DialogActionRewardSkillPointSDX : DialogActionAddBuff
{
    private readonly string name = string.Empty;

    public override void PerformAction(EntityPlayer player)
    {
        player.Progression.SkillPoints += Convert.ToInt32(Value);
        GameManager.ShowTooltip(player as EntityPlayerLocal, string.Format("You have recieved {0} Skill Points", Value), "");
    }
}