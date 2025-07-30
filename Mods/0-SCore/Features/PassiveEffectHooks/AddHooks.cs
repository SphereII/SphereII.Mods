public static class AddHooks
{

    public static void Initialize()
    {
        QuestEventManager.Current.BuyItems -= OnBought.BoughtItem;  
        QuestEventManager.Current.BuyItems += OnBought.BoughtItem;

        QuestEventManager.Current.SellItems -= OnSell.SellItem;
        QuestEventManager.Current.SellItems += OnSell.SellItem;

        QuestEventManager.Current.RepairItem -= OnRepair.CheckForDegradation;
        QuestEventManager.Current.RepairItem += OnRepair.CheckForDegradation;
        
        QuestEventManager.Current.CraftItem -= OnCraft.CheckForCrafting;    
        QuestEventManager.Current.CraftItem += OnCraft.CheckForCrafting;

        QuestEventManager.Current.ScrapItem -= OnSelfItemScrap.CheckForScrapping;
        QuestEventManager.Current.ScrapItem += OnSelfItemScrap.CheckForScrapping;
        
        QuestEventManager.Current.QuestComplete -= OnSelfQuestCompleted.CompleteQuest;
        QuestEventManager.Current.QuestComplete += OnSelfQuestCompleted.CompleteQuest;

        QuestEventManager.Current.BlockPlace -= OnSelfPlaceBlock.PlaceBlock;
        QuestEventManager.Current.BlockPlace += OnSelfPlaceBlock.PlaceBlock;
        
    }

}
