using System;
using UnityEngine;
class ObjectiveBuffSDX : BaseObjective
{
    String strBuff = "";

    // method to clone the Objective
    public override BaseObjective Clone()
    {
        ObjectiveBuffSDX objectiveBuff = new ObjectiveBuffSDX();
        this.CopyValues(objectiveBuff);
        return objectiveBuff;
    }

    // Helper to the clone
    protected void CopyValues(ObjectiveBuffSDX objective)
    {
        objective.ID = this.ID;
        objective.Value = this.Value;
        objective.Optional = this.Optional;
        objective.currentValue = this.currentValue;
        objective.Phase = this.Phase;
        objective.strBuff = this.strBuff;
    }

    public override void AddHooks()
    {
        // Don't do anything. Rely on the PumpQuest event   
    }

    public override void RemoveHooks()
    {
    }


    public override void SetupDisplay()
    {
        base.Description = string.Format("{0} {1}:", this.keyword, this.strBuff);
    }

    public override void Refresh()
    {
        if (string.IsNullOrEmpty(this.strBuff))
            return;

        // Check if its a player entity.
        EntityAlive myEntity = null;
        if (OwnerQuest.OwnerJournal.OwnerPlayer != null)
            myEntity = OwnerQuest.OwnerJournal.OwnerPlayer as EntityAlive;

        // If it's not a player entity, check for the SharedOwnerID
        if (myEntity == null)
        {
            // make sure that the entity is in the world before polling it.
            if (GameManager.Instance.World.Entities.dict.ContainsKey(OwnerQuest.SharedOwnerID))
                myEntity = GameManager.Instance.World.Entities.dict[OwnerQuest.SharedOwnerID] as EntityAlive;
        }

        // If the entity is *something*, check to see if it has the objective buff, and pass to completion.
        if (myEntity != null)
        {
            // Check if it has the desired buff.
            Debug.Log(" Checking if Entity has Buff: " + this.strBuff);
            Debug.Log(" Buffs: " + myEntity.Buffs.ActiveBuffs.ToArray().ToString());
            base.Complete = myEntity.Buffs.HasBuff(this.strBuff);
            if (base.Complete)
            {
                base.ObjectiveState = ObjectiveStates.Complete;
                base.OwnerQuest.CheckForCompletion(QuestClass.CompletionTypes.AutoComplete, null);
            }
        }
    }

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);

        if (properties.Values.ContainsKey("buff"))
            this.strBuff = properties.Values["buff"];
    }

    protected override bool useUpdateLoop
    {
        get
        {
            return true;
        }
    }
}
