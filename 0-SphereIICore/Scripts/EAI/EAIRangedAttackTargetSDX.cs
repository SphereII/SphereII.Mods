using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class EAIRangedAttackTargetSDX : EAIRangedAttackTarget2
{
    public override void Start()
    {
        // Make sure its using its ranged weapon.
        EntityUtilities.ChangeHandholdItem(theEntity.entityId, EntityUtilities.Need.Ranged);

        base.Start();

        // Face the entity; no trick shots!
        if (this.entityTarget != null)
            this.theEntity.RotateTo(this.entityTarget, 45f, 45f);
    }

    public override bool Continue()
    {
        bool result = base.Continue();

        // If the enemy is dead, reset its hand items
        if (this.entityTarget.IsDead())
            EntityUtilities.ChangeHandholdItem(this.theEntity.entityId, EntityUtilities.Need.Reset);

        return result;

    }



}

