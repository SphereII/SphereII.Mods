using System;
using UnityEngine;

//<property name="AITask-5" value="ApproachAndAttackSDX, Mods" param1="" param2=""  /> <!-- param1 not used -->
// Disables the Eating animation
class EAIApproachAndAttackSDX : EAIApproachAndAttackTarget
{
    private bool isTargetToEat = false;

    private bool blDisplayLog = false;
  
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog)
            Debug.Log(this.GetType() + " : " + this.theEntity.EntityName + ": " + this.theEntity.entityId + ": " + strMessage);
    }

    public override bool CanExecute()
    {
        bool result = base.CanExecute();
        
        if(result && this.entityTarget != null )
        {
            this.theEntity.SetLookPosition(this.entityTarget.getHeadPosition());
            this.theEntity.RotateTo(this.entityTarget, 30f, 30f);

            DisplayLog(" Has Task: " + EntityUtilities.HasTask(this.theEntity.entityId, "Ranged"));

            // Don't execute the approach and attack if there's a ranged ai task, and they are still 4 blocks away
            if(EntityUtilities.HasTask(this.theEntity.entityId, "Ranged") )
            {
                if (result)
                {
                    int Task = EntityUtilities.CheckAIRange(theEntity.entityId, entityTarget.entityId);
                    if (Task != 2)
                        return false;

                }
            }

            EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Melee);
        }

        
        return result;
    }

    public override bool Continue()
    {
        bool result = base.Continue();
        if (result)
        {
            float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
            float MaxRangeForWeapon = EffectManager.GetValue(PassiveEffects.MaxRange, this.theEntity.inventory.holdingItemItemValue, 40f, this.theEntity, null, this.theEntity.inventory.holdingItem.ItemTags, true, true, true, true, 1, true);
            if (distanceSq > 5 && distanceSq < MaxRangeForWeapon)
                return false;

        }

        return result;
    }


}

