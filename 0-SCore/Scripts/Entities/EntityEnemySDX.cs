/*
 * Class: EntityAliveEnemySDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base entityAlive. This is meant to be a base class, where other classes can extend
 *      from, giving them the ability to accept quests and buffs.
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features. 
 *
 *      <property name="Class" value="EntityEnemySDX, SCore" />
 */

using Random = System.Random;

public class EntityEnemySDX : EntityEnemy
{
    public float flEyeHeight = -1f;
    public Random random = new Random();
    public ulong timeToDie;

    public override float GetEyeHeight()
    {
        if (flEyeHeight == -1f)
            return base.GetEyeHeight();

        return flEyeHeight;
    }

    public override void PostInit()
    {
        base.PostInit();
        SetupStartingItems();
        inventory.SetHoldingItemIdx(0);
    }

    protected virtual void SetupStartingItems()
    {
        for (var i = 0; i < itemsOnEnterGame.Count; i++)
        {
            var itemStack = itemsOnEnterGame[i];
            var forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6);
            else
                itemStack.count = forId.Stacknumber.Value;
            inventory.SetItem(i, itemStack);
        }
    }
    public override void OnUpdateLive()
    {
        base.OnUpdateLive();
        if (!this.isEntityRemote)
        {
            if (!this.IsDead() && this.world.worldTime >= this.timeToDie && !this.attackTarget)
            {
                this.Kill(DamageResponse.New(true));
            }
        }
    }
    public override void OnAddedToWorld()
    {
        base.OnAddedToWorld();
        this.timeToDie = this.world.worldTime + 1800UL + (ulong)(22000f * this.rand.RandomFloat);
        if (this.IsFeral && base.GetSpawnerSource() == EnumSpawnerSource.Biome)
        {
            int num = (int)SkyManager.GetDawnTime();
            int num2 = (int)SkyManager.GetDuskTime();
            int num3 = GameUtils.WorldTimeToHours(this.WorldTimeBorn);
            if (num3 < num || num3 >= num2)
            {
                int num4 = GameUtils.WorldTimeToDays(this.world.worldTime);
                if (GameUtils.WorldTimeToHours(this.world.worldTime) >= num2)
                {
                    num4++;
                }
                this.timeToDie = GameUtils.DayTimeToWorldTime(num4, num, 0);
            }
        }
    }
    public override bool IsSavedToFile()
    {
        // Has a leader cvar set, good enough, as the leader may already be disconnected, so we'll fail a GetLeaderOrOwner()
        if (Buffs.HasCustomVar("Leader")) return true;

        // If they have a cvar persist, keep them around.
        if (Buffs.HasCustomVar("Persist")) return true;

        // If its dynamic spawn, don't let them stay.
        if (GetSpawnerSource() == EnumSpawnerSource.Dynamic) return false;

        // If its biome spawn, don't let them stay.
        if (GetSpawnerSource() == EnumSpawnerSource.Biome) return false;
        return true;
    }

    //public override void AwardKill(EntityAlive killer)
    //{
    //    if (killer != null && killer != this)
    //    {
    //        EntityPlayer entityPlayer = killer as EntityPlayer;
    //        if (entityPlayer)
    //        {
    //            if (!entityPlayer.isEntityRemote)
    //                SCoreQuestEventManager.Instance.EntityEnemyKilled(EntityClass.list[entityClass].entityClassName);
    //        }
    //    }
    //    base.AwardKill(killer);
    //}
}