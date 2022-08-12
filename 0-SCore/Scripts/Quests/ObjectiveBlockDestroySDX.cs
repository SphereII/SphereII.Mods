using System;

public class ObjectiveBlockDestroySDX : BaseObjective
{
	private string localizedName = "";
	private int neededCount;
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveBlockDestroy_keyword");
		this.localizedName = ((base.ID != "" && base.ID != null) ? Localization.Get(base.ID) : "Any Block");
		this.neededCount = Convert.ToInt32(base.Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.localizedName);
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.neededCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.BlockDestroy -= this.Current_BlockDestroy;
		QuestEventManager.Current.BlockDestroy += this.Current_BlockDestroy;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.BlockDestroy -= this.Current_BlockDestroy;
	}

	private void Current_BlockDestroy(Block block, Vector3i blockPos)
	{
		if (base.Complete)
		{
			return;
		}

		var blockname = block.GetBlockName();
		bool flag = false;
		if (base.ID == null || base.ID == "")
		{
			flag = true;
		}
		else
		{
			if (base.ID.EqualsCaseInsensitive(blockname))
			{
				flag = true;
			}
			if (blockname.Contains(":") && base.ID.EqualsCaseInsensitive(blockname.Substring(0, blockname.IndexOf(':'))))
			{
				flag = true;
			}
		}
		if (!flag && base.ID != null && base.ID != "")
		{
			Block blockByName = Block.GetBlockByName(base.ID, true);
			if (blockByName != null && blockByName.SelectAlternates && blockByName.ContainsAlternateBlock(blockname))
			{
				flag = true;
			}
		}
		if (flag && base.OwnerQuest.CheckRequirements())
		{
			byte currentValue = base.CurrentValue;
			base.CurrentValue = (byte)(currentValue + 1);
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if ((int)base.CurrentValue > this.neededCount)
		{
			base.CurrentValue = (byte)this.neededCount;
		}
		if (base.Complete)
		{
			return;
		}
		base.Complete = ((int)base.CurrentValue >= this.neededCount);
		if (base.Complete)
		{
			base.OwnerQuest.CheckForCompletion(QuestClass.CompletionTypes.AutoComplete, null, true);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveBlockDestroySDX objectiveBlockDestroy = new ObjectiveBlockDestroySDX();
		this.CopyValues(objectiveBlockDestroy);
		return objectiveBlockDestroy;
	}


}
