using System;
using UnityEngine;

/*	Value is the number of blocks before the objective passes
		<objective type="BlockDestroySDX, SCore" id="frameShapes" value="1" phase="2"/>

		<!-- If the id contains commas, it will loop through all these, checking if the destroyed block matches them. The count is still unified, so it counts for all of them -->
		<!-- for example, if you destroy woodChair1 and woodChair1Broken, it counts as 2 towards the objective. -->
		<objective type="BlockDestroySDX, SCore" id="woodChair1,officeChair01VariantHelper,woodChair1Broken" value="1" phase="2"/>


		<!-- If the base.ID does not match the passed in block, check if its a tag instead. -->
		<objective type="BlockDestroySDX, SCore" id="deepOre" value="1" phase="2"/>

		<!-- This can also be a comma delimited list -->
		<objective type="BlockDestroySDX, SCore" id="ore,deepOre" value="1" phase="2"/>
*/

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

		// Allow the objective to be searched via comma-delimited number
		var matchingName = "";

		bool flag = false;
		if (base.ID == null || base.ID == "")
		{
			flag = true;
		}
		else
		{
			// If the ID has commas in it, loop around and treat it like a list.
			foreach( var temp in base.ID.Split(','))
            {
				if ( temp.EqualsCaseInsensitive(blockname))
                {
					matchingName = temp;
					flag = true;
					break;
                }
            }
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
			Block blockByName;
			if ( string.IsNullOrEmpty(matchingName))
				blockByName = Block.GetBlockByName(base.ID, true);
			else
				blockByName = Block.GetBlockByName(matchingName, true);

			if (blockByName != null && blockByName.SelectAlternates && blockByName.ContainsAlternateBlock(blockname))
			{
				flag = true;
			}
		}

		// If we've reached here, and there's still not a valid block, see if its a tag
		if ( !flag )
        {
			foreach (var tag in base.ID.Split(','))
			{
				if (block.HasAnyFastTags(FastTags.Parse(tag)))
				{
					flag = true;
					break;
				}
			}
        }

		if (flag && base.OwnerQuest.CheckRequirements())
		{
			AddDestroyedBlock();
			HandleParty();
		}
	}

	public void AddDestroyedBlock()
    {
		byte currentValue = base.CurrentValue;
		base.CurrentValue = (byte)(currentValue + 1);
		this.Refresh();
	}

	private void HandleParty()
	{
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (ownerPlayer.Party == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.BlockActivated, ownerPlayer.entityId, base.OwnerQuest.QuestCode), false, -1, -1, -1, -1);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestObjectiveUpdate>().Setup(NetPackageQuestObjectiveUpdate.QuestObjectiveEventTypes.BlockActivated, ownerPlayer.entityId, base.OwnerQuest.QuestCode), false);
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
