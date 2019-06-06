/*
 * Class: EntityFlyingBaseSDX
 * Author:  sphereii
 * Category: Entity
 * Description:
 *      This mod is a small extension of the Vulture class that makes the flying entities non-aggressive by default. They follow the same AI of vultures, minus the 
 *      attack
 *
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *  
 *      <property name="Class" value="EntityFlyingBaseSDX, Mods" />
 *
 * Features:
 *      This class inherits features from the Vulture class
 *
 *  Demeanor:
 *      Flying creatures are naturally passive, but can be turned aggressive by seeing the Demeanor flag to "Aggressive"
 *
 *      Usage:
 *          <property name="Demeanor" value="Passive" />
 *
 */
using System;
/*
 *
 * Simple flying code. Flying creatures behave like vultures, swooping down at the player, but do not attack the player
 */

class EntityFlyingBaseSDX : EntityVulture
{
    // Create local task and target lists, so we can add more fleshed out flying creatures
    EAITaskList TaskList = new EAITaskList();
    EAITaskList TargetList = new EAITaskList();
    public static EAIFactory AIFactory = new EAIFactory();

    public String strDemeanor = "Passive";

    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);

        // Sets the hand value, so we can give our entities ranged weapons.
        this.inventory.SetSlots(new ItemStack[]
        {
               new ItemStack(this.inventory.GetBareHandItemValue(), 1)
        });

        EntityClass entityClass = EntityClass.list[_entityClass];
        if (entityClass.Properties.Values.ContainsKey("Demeanor"))
            this.strDemeanor = entityClass.Properties.Values["Demeanor"].ToString();
    }

    // Since the Vulture class doesn't inherit any of the AI tasks that we want and need, we'll over-ride and take control
    public override void CopyPropertiesFromEntityClass()
    {
        EntityClass entityClass = EntityClass.list[this.entityClass];
        base.CopyPropertiesFromEntityClass();

        int num = 1;
        // Grab all the AITasks avaiable
        while (entityClass.Properties.Values.ContainsKey(EntityClass.PropAITask + num))
        {
            string text = "EAI" + entityClass.Properties.Values[EntityClass.PropAITask + num];
            EAIBase eaibase = (EAIBase)AIFactory.Instantiate(text);
            if (eaibase == null)
            {
                throw new Exception("Class '" + text + "' not found!");
            }

      //      eaibase.SetEntity(this);
            if (entityClass.Properties.Params1.ContainsKey(EntityClass.PropAITask + num))
            {
                eaibase.SetParams2(entityClass.Properties.Params1[EntityClass.PropAITask + num]);
            }
            if (entityClass.Properties.Params2.ContainsKey(EntityClass.PropAITask + num))
            {
                eaibase.SetParams2(entityClass.Properties.Params2[EntityClass.PropAITask + num]);
            }
            this.TaskList.AddTask(num, eaibase);
            num++;
        }
        int num2 = 1;
        // Grab all the AI Targets
        while (entityClass.Properties.Values.ContainsKey(EntityClass.PropAITargetTask + num2))
        {
            string text2 = "EAI" + entityClass.Properties.Values[EntityClass.PropAITargetTask + num2];
            EAIBase eaibase2 = (EAIBase)AIFactory.Instantiate(text2);
            if (eaibase2 == null)
            {
                throw new Exception("Class '" + text2 + "' not found!");
            }
       //     eaibase2.SetEntity(this);
            if (entityClass.Properties.Params1.ContainsKey(EntityClass.PropAITargetTask + num2))
            {
                eaibase2.SetParams1(entityClass.Properties.Params1[EntityClass.PropAITargetTask + num2]);
            }
            if (entityClass.Properties.Params2.ContainsKey(EntityClass.PropAITargetTask + num2))
            {
                eaibase2.SetParams1(entityClass.Properties.Params2[EntityClass.PropAITargetTask + num2]);
            }
            this.TargetList.AddTask(num2, eaibase2);
            num2++;
        }
    }

    // We over-ride the Attack Valid check, and see if the demeanor of the entity is aggressive (by default) or passive.
    // If it's passive, the entity will be curious but won't actually attack
    public override bool IsAttackValid()
    {
        return base.IsAttackValid() && (strDemeanor == "Aggressive");
    }

    // The vulture updateTasks() doesn't call down the chain, so it never does any checks on the AI Tasks.
    protected override void updateTasks()
    {
        base.updateTasks();

        if (!GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
        {
            if (!EntityClass.list[this.entityClass].UseAIPackages)
            {
                using (ProfilerUsingFactory.Profile("entities.live.ai.tasks"))
                {
                    this.TaskList.OnUpdateTasks();
                    this.TargetList.OnUpdateTasks();
                }
            }
        }
    }


}