public class RewardGiveCvarDMT : BaseReward
{
    public RewardGiveCvarDMT()
    {
        base.HiddenReward = true;
    }

    public override void SetupReward()
    {
        base.HiddenReward = true;
    }

    public override void GiveReward(EntityPlayer player)
    {
        float value = StringParsers.ParseFloat(base.Value, 0, -1, System.Globalization.NumberStyles.Float);
        if (base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasCustomVar(base.ID))
            value += base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.GetCustomVar(base.ID);
        base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.SetCustomVar(base.ID, value, true);
    }

    public override BaseReward Clone()
    {
        RewardGiveCvarDMT rewardGiveCvarDMT = new RewardGiveCvarDMT();
        base.CopyValues(rewardGiveCvarDMT);
        return rewardGiveCvarDMT;
    }
}
