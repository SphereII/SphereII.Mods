public class EAISwimSDX : EAIBase
{
    public EAISwimSDX()
    {
        MutexBits = 4;
    }

    public override void Init(EntityAlive _theEntity)
    {
        base.Init(_theEntity);
        theEntity.getNavigator().setCanDrown(true);
        //	this.theEntity.getNavigator().setInWater(true);
    }

    public override bool CanExecute()
    {
        return theEntity.IsSwimming();
    }

    public override void Update()
    {
        if (base.RandomFloat < 0.8f)
        {
            theEntity.moveHelper.StartJump(false, 0f, 0f);
        }
    }
}
