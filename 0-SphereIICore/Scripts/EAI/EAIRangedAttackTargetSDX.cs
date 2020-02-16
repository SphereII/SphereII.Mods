using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class EAIRangedAttackTargetSDX : EAIRangedAttackTarget2
{

    public override void Start()
    {
        base.Start();
        if (this.entityTarget != null)
            this.theEntity.RotateTo(this.entityTarget, 45f, 45f);
    }
    public override bool Continue()
    {
        bool result = base.Continue();

        // Non zombies should continue to attack
        if (this.entityTarget.IsDead())
        {
            this.theEntity.IsEating = false;
            this.theEntity.SetAttackTarget(null, 0);
            return false;
        }
        float distanceSq = this.entityTarget.GetDistanceSq(this.theEntity);
        // Let the entity move closer, without walking a few steps and trying to fire, which can make the entity stutter as it tries to keep up with a retreating enemey.
        if (distanceSq > 50 && distanceSq < 60)
            return result;

        // Hold your ground
        if (distanceSq > 10f && distanceSq < 60)
        {
          //  DisplayLog("I am ranged, so I will not move forward.", this.theEntity);
            this.theEntity.SetLookPosition(this.entityTarget.position);
            this.theEntity.RotateTo(this.entityTarget, 45, 45);
            this.theEntity.navigator.clearPath();
            this.theEntity.moveHelper.Stop();

            return false;
        }

        // Back away!
        if (distanceSq > 4 && distanceSq < 10)
        {
            EntityUtilities.BackupHelper(this.theEntity.entityId, this.entityTarget.position, 40f);
            return true;
        }
        if (distanceSq < 5)
        {
            // Fight and hold your ground until its stunned, then run.
            if (this.entityTarget.bodyDamage.CurrentStun != EnumEntityStunType.None)
            {
                EntityUtilities.BackupHelper(this.theEntity.entityId, this.entityTarget.position, 40f);
                return true;
            }

        }

        return result;
    }




}

