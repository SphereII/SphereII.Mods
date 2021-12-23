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


}