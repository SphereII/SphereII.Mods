using UnityEngine;

public class EAISwimSDX : EAIWander
{
    private Vector3 position;
    private float time;

    public EAISwimSDX()
    {
        MutexBits = 1;
    }

    public override void Init(EntityAlive entity)
    {
        base.Init(entity);
        theEntity.getNavigator().setCanDrown(true);
    }

    public override void Start()
    {
        this.time = 0f;
        this.theEntity.FindPath(this.position, this.theEntity.GetMoveSpeed(), false, this);
    }

    public override bool CanExecute()
    {
        if (this.theEntity.sleepingOrWakingUp)
        {
            return false;
        }

        if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
        {
            return false;
        }

        int num = (int) (200f * this.executeWaitTime);
        if (base.GetRandom(1000) >= num)
        {
            return false;
        }

        if (this.manager.lookTime > 0f)
        {
            return false;
        }

        int num2 = (int) this.manager.interestDistance;
        Vector3 vector;
        if (this.theEntity.IsAlert)
        {
            num2 *= 2;
            vector = RandomPositionGenerator.CalcAway(this.theEntity, 0, num2, 4, this.theEntity.LastTargetPos);
        }
        else
        {
            vector = RandomPositionGenerator.Calc(this.theEntity, num2, 4);
        }

        if (vector.Equals(Vector3.zero))
        {
            return false;
        }

        position = vector;
        var result = theEntity.world.IsWater(vector);
        Debug.Log($"Is target in water? {result}");
        return result;
    }

    public override bool Continue()
    {
        var result = theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None &&
               theEntity.moveHelper.BlockedTime <= 0.3f && time <= 30f &&
               !theEntity.navigator.noPathAndNotPlanningOne();
Debug.Log($"Can Continue? : {result}");
return result;
    }

    public override void Update()
    {
        base.Update();
        if (RandomFloat < 0.8f) theEntity.moveHelper.StartJump(false);
    }
}