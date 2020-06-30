using System;

public class EAISwimSDX : EAIBase
{
	public EAISwimSDX()
	{
		this.MutexBits = 4;
	}

	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.theEntity.getNavigator().setCanDrown(true);
	//	this.theEntity.getNavigator().setInWater(true);
	}

	public override bool CanExecute()
	{
		return this.theEntity.IsSwimming();
	}

	public override void Update()
	{
		if (base.RandomFloat < 0.8f)
		{
			this.theEntity.moveHelper.StartJump(false, 0f, 0f);
		}
	}
}
