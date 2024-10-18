using System;
using UnityEngine;

public class ObjectiveBuffSDX : BaseObjective
{
    private string strBuff = "";

    public override bool useUpdateLoop => false;

    // method to clone the Objective
    public override BaseObjective Clone()
    {
        var objectiveBuff = new ObjectiveBuffSDX();
        CopyValues(objectiveBuff);
        return objectiveBuff;
    }

    // Helper to the clone
    protected void CopyValues(ObjectiveBuffSDX objective)
    {
        objective.ID = ID;
        objective.Value = Value;
        objective.Optional = Optional;
        objective.currentValue = currentValue;
        objective.Phase = Phase;
        objective.strBuff = strBuff;
    }

    public override void AddHooks() {
        EventOnBuffAdded.BuffAdded += CheckForBuff;
  
    }

    public override void RemoveHooks()
    {
        EventOnBuffAdded.BuffAdded -= CheckForBuff;
    }


    public override void SetupObjective()
    {
        this.keyword = Localization.Get("ObjectiveBuffSDX_keyword", false);
        
    }

    public override void SetupDisplay()
    {
        var buff = BuffManager.GetBuff(strBuff);
        if (buff == null) return;

        Description = $"{keyword} {buff.LocalizedName}";
    }

    // public override void Update(float deltaTime) {
    //     if (!(Time.time > this.updateTime)) return;
    //     updateTime = Time.time + 1f;
    //     var buffClass = BuffManager.GetBuff(strBuff);
    //     CheckForBuff(buffClass);
    // }

    public void CheckForBuff(BuffClass buffClass) {
        if (string.IsNullOrEmpty(strBuff)) return;
        if (buffClass == null) return;
        if (!string.Equals(buffClass.Name, strBuff, StringComparison.CurrentCultureIgnoreCase)) return;
        if (Complete) return;
       
        Complete= OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasBuff(strBuff);
        if (Complete == false) return;   
        ObjectiveState = ObjectiveStates.Complete;
        OwnerQuest.RefreshQuestCompletion();
    }
    
    
    // this isn't necessary anymore, with the event hooks working properly, but left just in case.
    public override void Refresh()
    {
        if (string.IsNullOrEmpty(strBuff))
            return;

        // Check if its a player entity.
        EntityAlive myEntity = null;
        if (OwnerQuest.OwnerJournal.OwnerPlayer != null)
            myEntity = OwnerQuest.OwnerJournal.OwnerPlayer;

        // If it's not a player entity, check for the SharedOwnerID
        if (myEntity == null)
            // make sure that the entity is in the world before polling it.
            if (GameManager.Instance.World.Entities.dict.ContainsKey(OwnerQuest.SharedOwnerID))
                myEntity = GameManager.Instance.World.Entities.dict[OwnerQuest.SharedOwnerID] as EntityAlive;

        // If the entity is *something*, check to see if it has the objective buff, and pass to completion.
        if (myEntity == null) return;
        Complete = myEntity.Buffs.HasBuff(strBuff);
        if (!Complete) return;
        ObjectiveState = ObjectiveStates.Complete;
        OwnerQuest.RefreshQuestCompletion();
    }

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);

        if (properties.Values.ContainsKey("buff"))
            strBuff = properties.Values["buff"].ToLower();
    }
}