class EAIRangedAttackTargetSDX : EAIRangedAttackTarget2
{
    public override void Start()
    {
        base.Start();

        // Face the entity; no trick shots!
        if (entityTarget != null)
            theEntity.RotateTo(entityTarget, 45f, 45f);
    }

    public override void Reset()
    {
        base.Reset();
        //   EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Ranged);

    }
    public override bool Continue()
    {
        bool result = base.Continue();

        //// If the enemy is dead, reset its hand items
        //if (this.entityTarget.IsDead())
        //    EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Ranged);

        //// Make sure its using its ranged weapon.
        //if ( result )
        //     EntityUtilities.ChangeHandholdItem(theEntity.entityId, EntityUtilities.Need.Ranged);
        return result;

    }

    public override void Update()
    {
        //if (this.theEntity.HasInvestigatePosition)
        //    return;
        base.Update();
    }


}

