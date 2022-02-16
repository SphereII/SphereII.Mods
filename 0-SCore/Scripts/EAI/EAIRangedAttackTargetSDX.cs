public class EAIRangedAttackTargetSDX : EAIRangedAttackTarget2
{
    private EntityAlive entityTarget;
    private int attackTimeout;
    private int itemActionType;
    private bool bCanSee;
    private int curAttackPeriod;
    private int attackPeriodMax;

    public EAIRangedAttackTargetSDX() : base()
    {

    }

    public override void SetData(DictionarySave<string, string> data)
    {
        base.SetData(data);
        base.GetData(data, "itemType", ref this.itemActionType);
        base.GetData(data, "attackPeriod", ref this.attackPeriodMax);
    }

    private bool inRange(float _distanceSq)
    {
        float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, this.theEntity.inventory.holdingItemItemValue, 0f, this.theEntity, null, default(FastTags), true, true, true, true, 1, true);
        return _distanceSq < value * value * 0.25f;
    }

    public override bool CanExecute()
    {
        if (this.theEntity.inventory.holdingItem.Actions == null)
            return false;

        ItemActionRanged itemActionRanged = this.theEntity.inventory.holdingItem.Actions[itemActionType] as ItemActionRanged;
        if (itemActionRanged == null)
            return false;

        //if (this.theEntity.inventory.holdingItemItemValue.Meta <= 0)
        //{
        //    if (itemActionRanged.CanReload(this.theEntity.inventory.holdingItemData.actionData[1]))
        //    {
        //        itemActionRanged.ReloadGun(this.theEntity.inventory.holdingItemData.actionData[1]);
        //    }
        //    return false;
        //}
        if (this.attackTimeout > 0)
        {
            this.attackTimeout--;
            return false;
        }
        if (!this.theEntity.Spawned || !this.theEntity.IsAttackValid())
        {
            return false;
        }
        entityTarget = EntityUtilities.GetAttackOrRevengeTarget(theEntity.entityId) as EntityAlive;
        if (entityTarget == null)
            return false;

        float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
        if (!this.inRange(distanceSq))
        {
            return false;
        }
        this.bCanSee = this.theEntity.CanSee(this.entityTarget);

        return this.bCanSee;
    }

    public override bool Continue()
    {
        float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
        if (!this.inRange(distanceSq))
        {
            return false;
        }

        return true;
        //return this.curAttackPeriod > 0 && this.theEntity.hasBeenAttackedTime <= 0;
    }

    public override void Start()
    {

        float delay = this.theEntity.inventory.holdingItem.Actions[0].Delay;
        this.attackTimeout = (int)(delay * 20f);
        this.curAttackPeriod = this.attackPeriodMax;
    }

    public override void Update()
    {
        this.curAttackPeriod--;
        if (this.inRange(this.entityTarget.GetDistanceSq(this.theEntity)) && this.theEntity.IsInFrontOfMe(this.entityTarget.getHeadPosition()))
        {
            if (this.itemActionType == 0)
            {
                this.theEntity.Attack((float)this.curAttackPeriod < (float)this.attackPeriodMax / 2f);
                return;
            }
            this.theEntity.Use((float)this.curAttackPeriod < (float)this.attackPeriodMax / 2f);
        }
    }

    public override void Reset()
    {
        this.entityTarget = null;
        this.curAttackPeriod = 0;
        float delay = this.theEntity.inventory.holdingItem.Actions[0].Delay;
        this.attackTimeout = (int)(delay * 20f);
        this.attackTimeout = 5 + base.GetRandom(5);
        if (this.itemActionType == 0)
        {
            this.theEntity.Attack(true);
            return;
        }
        this.theEntity.Use(true);
    }

    public override string ToString()
    {
        bool flag = this.entityTarget != null && this.inRange(this.entityTarget.GetDistanceSq(this.theEntity));
        return string.Concat(new string[]
        {
            base.ToString(),
            ": ",
            (this.entityTarget != null) ? this.entityTarget.EntityName : "null",
            " see: ",
            this.bCanSee ? "Y" : "N",
            " range=",
            flag ? "Y" : "N"
        });
    }
}

/*
internal class EAIRangedAttackTargetSDX : EAIRangedAttackTarget2
{
    private EntityAlive entityTarget;
    private static FieldInfo EntityTargetField;

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        if (EntityTargetField == null)
            EntityTargetField = AccessTools.Field(typeof(EAIRangedAttackTarget2), "entityTarget");
    }

    public override void Start()
    {
        base.Start();

        // Face the entity; no trick shots!
        if (entityTarget != null)
            theEntity.RotateTo(entityTarget, 45f, 45f);
    }

    public override bool CanExecute()
    {
        if (base.CanExecute())
        {
            entityTarget = EntityUtilities.GetAttackOrRevengeTarget(theEntity.entityId) as EntityAlive;
            EntityTargetField.SetValue(EntityTargetField, entityTarget);
            if (entityTarget == null)
                return false;
        }

        return true;
    }

    public override void Reset()
    {
        base.Reset();
        //   EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Ranged);
    }

    public override bool Continue()
    {
        var result = base.Continue();

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
}*/