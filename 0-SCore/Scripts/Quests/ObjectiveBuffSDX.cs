using UnityEngine;

internal class ObjectiveBuffSDX : BaseObjective
{
    private string strBuff = "";

    protected override bool useUpdateLoop => true;

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

    public override void AddHooks()
    {
        // Don't do anything. Rely on the PumpQuest event   
    }

    public override void RemoveHooks()
    {
    }


    public override void SetupObjective()
    {
        this.keyword = Localization.Get("ObjectiveBuffSDX_keyword");
        if ( string.IsNullOrEmpty( this.keyword ) )
            this.keyword = Localization.Get("ObjectiveBuff_keyword");
    }

    public override void SetupDisplay()
    {
        var buff = BuffManager.GetBuff(strBuff);
        if (buff == null) return;

        Description = string.Format("{0} {1}:", keyword, buff.LocalizedName);
    }

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
        if (myEntity != null)
        {
            // Check if it has the desired buff.
            Debug.Log(" Checking if Entity has Buff: " + strBuff);
            Debug.Log(" Buffs: " + myEntity.Buffs.ActiveBuffs.ToArray());
            Complete = myEntity.Buffs.HasBuff(strBuff);
            if (Complete)
            {
                ObjectiveState = ObjectiveStates.Complete;
                OwnerQuest.CheckForCompletion();
            }
        }
    }

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);

        if (properties.Values.ContainsKey("buff"))
            strBuff = properties.Values["buff"];
    }
}