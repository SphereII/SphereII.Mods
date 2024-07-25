using System.Globalization;


// <reward type="GiveBuff, SCore" id="yourbuff" />
public class RewardGiveBuff : BaseReward
{
    public RewardGiveBuff()
    {
        HiddenReward = false;
    }

    public override void SetupReward()
    {
        HiddenReward = false;
    }

    public override void GiveReward(EntityPlayer player)
    {
        OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.AddBuff(ID);
    }

    public override BaseReward Clone()
    {
        var reward = new RewardGiveBuff();
        CopyValues(reward);
        return reward;
    }
}