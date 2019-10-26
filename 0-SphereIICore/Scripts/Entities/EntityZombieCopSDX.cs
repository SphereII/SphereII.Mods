/*
* Class: EntityZombieCopSDX
* Author:  sphereii and HAL9000
* Category: Entity
* Description:
*   This mod is an extension of the ZombieSDX. However, since base class supports hand items, this class is now useless.
*/
public class EntityZombieCopSDX : EntityZombie
{
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);
        this.inventory.SetSlots(new ItemStack[]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }

    public override void InitFromPrefab(int _entityClass)
    {
        base.InitFromPrefab(_entityClass);
        this.inventory.SetSlots(new ItemStack[]
        {
            new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });
    }

 
}

