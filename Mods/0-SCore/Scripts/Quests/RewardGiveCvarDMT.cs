using System.Globalization;


// <reward type="GiveCvarDMT, SCore" id="yourcvar" value="1" />
public class RewardGiveCvarDMT : BaseReward
{
    public RewardGiveCvarDMT()
    {
        HiddenReward = true;
    }

    public override void SetupReward()
    {
        HiddenReward = true;
    }

    public override void GiveReward(EntityPlayer player)
    {
        var value = StringParsers.ParseFloat(Value, 0, -1, NumberStyles.Float);
        if (OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasCustomVar(ID))
            value += OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.GetCustomVar(ID);
        OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.SetCustomVar(ID, value);
    }

    public override BaseReward Clone()
    {
        var rewardGiveCvarDMT = new RewardGiveCvarDMT();
        CopyValues(rewardGiveCvarDMT);
        return rewardGiveCvarDMT;
    }
}