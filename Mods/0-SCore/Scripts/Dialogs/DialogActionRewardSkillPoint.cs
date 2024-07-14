using System;
public class DialogActionRewardSkillPointSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        player.Progression.SkillPoints += Convert.ToInt32(base.Value);
    }
}
