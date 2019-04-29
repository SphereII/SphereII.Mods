class RewardItemSDX : RewardItem
{
    //		<reward type="ItemSDX, Mods" id="casinoCoin" value="1" />
    public override void GiveReward(EntityPlayer player)
    {
        if (GameManager.Instance.World.Entities.dict.ContainsKey(base.OwnerQuest.SharedOwnerID))
        {
            EntityAlive questEntity = GameManager.Instance.World.Entities.dict[base.OwnerQuest.SharedOwnerID] as EntityAlive;
            if (questEntity == null)
                return;

            questEntity.inventory.AddItem(this.Item);
        }
    }

}

