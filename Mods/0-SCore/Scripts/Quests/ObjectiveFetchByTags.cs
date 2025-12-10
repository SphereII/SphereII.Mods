using System;

// - Added FetchByTags, which triggers when the item with the specified tag is added to the inventory.
//     <objective type="FetchByTags, SCore" value="5" phase="1">
//     <property name="tags" value="ore" />
//     </objective>


public class ObjectiveFetchByTags :  ObjectiveFetch
{
    private FastTags<TagGroup.Global> tags;
    private string localizationDesc;
    public override void SetupDisplay()
    {
        base.Description = string.Format(this.keyword, Localization.Get(localizationDesc));
        this.StatusText = string.Format("{0}/{1}", currentCount, itemCount);
    }
    
    public override BaseObjective Clone()
    {
        ObjectiveFetchByTags objectiveFetch = new ObjectiveFetchByTags();
        this.CopyValues(objectiveFetch);
        objectiveFetch.KeepItems = true;
        objectiveFetch.tags = tags;
        objectiveFetch.localizationDesc = localizationDesc;
        return objectiveFetch;
    }

 
    public override void RemoveObjectives()
    {

    }


    public override void SetupObjective()
    {
        keyword = Localization.Get("ObjectiveFetch_keyword");
        itemCount = Convert.ToInt32(this.Value);
    }
    public override void Refresh()
    {
        if (Complete)
        {
            return;
        }
        var playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;

        currentCount = playerInventory.Backpack.GetItemCount(tags);
        currentCount += playerInventory.Toolbelt.GetItemCount(tags);
        if (currentCount > itemCount)
        {
            currentCount = itemCount;
        }

        SetupDisplay();
        if (currentCount != CurrentValue)
        {
            CurrentValue = (byte)currentCount;
        }
        Complete = currentCount >= itemCount && OwnerQuest.CheckRequirements();
        if (Complete)
        {
            OwnerQuest.RefreshQuestCompletion();
        }
    }
    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);
        if (properties.Values.ContainsKey("tags"))
        {
            tags = FastTags<TagGroup.Global>.Parse(properties.Values["tags"]);
        }
        
        if ( properties.Values.ContainsKey("Description"))
            localizationDesc = properties.GetStringValue("Description");
    }

}
