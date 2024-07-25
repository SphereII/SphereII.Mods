internal class RewardItemSDX : RewardItem
{
    //		<reward type="ItemSDX, SCore" id="casinoCoin" value="1" />
    public override void GiveReward(EntityPlayer player)
    {
        if (GameManager.Instance.World.Entities.dict.ContainsKey(OwnerQuest.SharedOwnerID))
        {
            var questEntity = GameManager.Instance.World.Entities.dict[OwnerQuest.SharedOwnerID] as EntityAlive;
            if (questEntity == null)
                return;

            questEntity.inventory.AddItem(Item);
        }
    }
}