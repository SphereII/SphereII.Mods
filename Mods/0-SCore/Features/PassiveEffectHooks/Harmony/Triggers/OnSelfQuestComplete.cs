using HarmonyLib;
using UnityEngine;

public static class OnSelfQuestCompleted
{
    public static void CompleteQuest(FastTags<TagGroup.Global> questTags, QuestClass questClass)
    {
        var minEventParams = new MinEventParams {
            TileEntity = TraderUtils.GetCurrentTraderTileEntity(),
            Self = GameManager.Instance.World.GetPrimaryPlayer(),
            ItemValue = ItemValue.None,
            Biome = GameManager.Instance.World.GetPrimaryPlayer()?.biomeStandingOn
        };

        minEventParams.Self.SetCVar("$completedQuestTier", questClass.DifficultyTier);
        minEventParams.Self.MinEventContext = minEventParams;
        minEventParams.Self.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfQuestComplete);
        minEventParams.Self.SetCVar("$completedQuestTier", 0);
    }
}