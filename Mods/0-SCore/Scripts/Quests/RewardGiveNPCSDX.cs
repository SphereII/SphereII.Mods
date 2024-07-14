using UnityEngine;

internal class RewardGiveNPCSDX : BaseReward
{
    //		<reward type="GiveNPCSDX, SCore" id="entityGroup"  />  // Spawns in an entity from the group to be your NPC
    //		<reward type="GiveNPCSDX, SCore"  />  // Hires the current NPC
    public override void GiveReward(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(ID))
        {
            var questNPC = GameManager.Instance.World.Entities.dict[OwnerQuest.QuestGiverID] as EntityAliveSDX;
            if (questNPC)
                EntityUtilities.SetOwner(questNPC.entityId, player.entityId);
            else
                Debug.Log(" NPC not Found.");
        }
        else // Try to spawn in a new NPC from the NPC Group
        {
            SpawnFromGroup(ID, player);
        }
    }

    public override BaseReward Clone()
    {
        var rewardNPC = new RewardGiveNPCSDX();
        CopyValues(rewardNPC);
        return rewardNPC;
    }

    public override void SetupReward()
    {
        Description = Localization.Get("RewardGiveNPCSDX_keyword");
        SetupValueText();
        Icon = "ui_game_symbol_trophy";
    }

    public override string GetRewardText()
    {
        return "I'll join you.";
    }

    private void SetupValueText()
    {
        ValueText = "Value Test";
    }

    public void SpawnFromGroup(string strEntityGroup, EntityPlayer player)
    {
        var EntityID = 0;

        // If the group is set, then use it.
        if (string.IsNullOrEmpty(strEntityGroup))
            return;

        var ClassID = 0;
        EntityID = EntityGroups.GetRandomFromGroup(strEntityGroup, ref ClassID);
        if (EntityID == -1)
            return; // failed

        var NewEntity = EntityFactory.CreateEntity(EntityID, player.position, player.rotation);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
            if (NewEntity is EntityAliveSDX)
            {
                var uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
                uiforPlayer.windowManager.Open("JoinInformation", true);
                //(NewEntity as EntityAliveSDX).SetOwner(player as EntityPlayerLocal);
            }
        }
        else
        {
            Debug.Log(" Could not Spawn NPC for: " + player.EntityName + " : " + player.entityId);
        }
    }
}