/*
* Class: EntityGenericSDX
* Author:  sphereii 
* Category: Entity
* Description:
*   This mod is an extension of the EntityAlive. 
*/
public class EntityGenericSDX : EntityAlive
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

