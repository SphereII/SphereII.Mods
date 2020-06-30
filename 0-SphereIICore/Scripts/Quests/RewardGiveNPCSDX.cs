using UnityEngine;
class RewardGiveNPCSDX : BaseReward
{
    //		<reward type="GiveNPCSDX, Mods" id="entityGroup"  />  // Spawns in an entity from the group to be your NPC
    //		<reward type="GiveNPCSDX, Mods"  />  // Hires the current NPC
    public override void GiveReward(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(base.ID))
        {
            EntityAliveSDX questNPC = GameManager.Instance.World.Entities.dict[base.OwnerQuest.QuestGiverID] as EntityAliveSDX;
            if (questNPC)
            {
                EntityUtilities.SetOwner(questNPC.entityId, player.entityId);
            }
            else
            {
                Debug.Log(" NPC not Found.");
            }
        }
        else   // Try to spawn in a new NPC from the NPC Group
        {
            SpawnFromGroup(base.ID, player);
        }
    }

    public override BaseReward Clone()
    {
        RewardGiveNPCSDX rewardNPC = new RewardGiveNPCSDX();
        base.CopyValues(rewardNPC);
        return rewardNPC;
    }

    public override void SetupReward()
    {
        base.Description = Localization.Get("RewardGiveNPCSDX_keyword");
        this.SetupValueText();
        base.Icon = "ui_game_symbol_trophy";
    }

    public override string GetRewardText()
    {
        return "I'll join you.";
    }
    private void SetupValueText()
    {
        base.ValueText = "Value Test";
    }
    public void SpawnFromGroup( string strEntityGroup, EntityPlayer player )
    {
        int EntityID = 0;

        // If the group is set, then use it.
        if (string.IsNullOrEmpty(strEntityGroup))
            return;

        int ClassID = 0;
        EntityID = EntityGroups.GetRandomFromGroup(strEntityGroup, ref ClassID);
        if (EntityID == -1)
            return; // failed

        Entity NewEntity = EntityFactory.CreateEntity(EntityID, player.position, player.rotation);
        if (NewEntity)
        {
            NewEntity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            GameManager.Instance.World.SpawnEntityInWorld(NewEntity);
            if (NewEntity is EntityAliveSDX)
            {
                LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(player as EntityPlayerLocal);
                uiforPlayer.windowManager.Open("JoinInformation", true, false, true);
                //(NewEntity as EntityAliveSDX).SetOwner(player as EntityPlayerLocal);
            }
                
        }
        else
        {
            Debug.Log(" Could not Spawn NPC for: " + player.EntityName + " : " + player.entityId);
        }
    }
}

