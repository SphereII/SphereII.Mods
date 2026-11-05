public class SCoreConstants
{
    // NPC
    // These start at the defaults from Config/blocks.xml and are read from ConfigFeatureBlock by Load().
    // Nothing is parsed when the class loads, so another mod touching this class during startup
    // (before blocks exist) can't break it for the whole session.
    // They stay as fields, not properties, so mods compiled against them keep working.
    public const float DefaultBlockTimeToJump = 0.19f;
    public const float DefaultBlockedTime = 0.2f;

    public static float BlockTimeToJump = DefaultBlockTimeToJump;
    public static float BlockedTime = DefaultBlockedTime;

    private const string AdvancedNPCFeatures = "AdvancedNPCFeatures";

    /// <summary>
    /// Reads the values from ConfigFeatureBlock. Does nothing if the block isn't loaded yet.
    /// A missing or bad value keeps its default and logs a warning.
    /// </summary>
    public static void Load()
    {
        if (Block.GetBlockValue("ConfigFeatureBlock").type == 0) return;

        BlockTimeToJump = ReadFloat("BlockTimeToJump", DefaultBlockTimeToJump);
        BlockedTime = ReadFloat("BlockedTime", DefaultBlockedTime);
    }

    public static void Load(ref ModEvents.SGameStartDoneData data)
    {
        Load();
    }

    private static float ReadFloat(string property, float fallback)
    {
        var value = Configuration.GetPropertyValue(AdvancedNPCFeatures, property);
        if (StringParsers.TryParseFloat(value, out var result))
            return result;

        Log.Warning($"[0-SCore] {AdvancedNPCFeatures}/{property} is missing or not a number ('{value}'). Using {fallback}.");
        return fallback;
    }

    // EntityAlive SDX

    #region EntityAliveSDX Properties

    public const string PropHirable = "Hirable";
    public const string PropIsQuestGiver = "IsQuestGiver";
    public const string PropNames = "Names";
    public const string PropBoundaryBox = "BoundaryBox";
    public const string PropBoundaryCenter = "Center";
    public const string PropPathingCode = "PathingCode";
    public const string PropLeader = "Leader";
    public const string PropPersist = "Persist";
    public const string PropBagItems = "BagItems";
    public const string PropTitles = "Titles";
    public const string PropBuffs = "Buffs";
    public const string PropFollow = "Follow";
    public const string PropStay = "Stay";
    public const string PropGuard = "Guard";
    public const string PropWander = "Wander";

    // CustomVar Keys (Examples)
    public const string CVarOnMission = "onMission";
    public const string CVarPathingCode = "PathingCode";
    public const string CVarEnemyNPC = "EnemyNPC";
    public const string CVarQuest = "Quest";
    public const string CVarTrader = "Trader";

    // Magic Numbers (Examples - Adjust values as needed)
    public const float DefaultEnemyDistanceToTalk = 10f;
    public const int LeaderCacheExpiryTicks = 30;
    public const float SpiderEyeHeight = 0.15f;
    public const float ZombieCopEyeHeight = 0.6f;
    public const float DefaultEyeHeight = 0.8f;
    public const float CrouchingEyeHeightMultiplier = 0.5f;
    public const float MinDistanceSqToProcessMovement = 0.010000001f; // ~0.1f distance

    // String Literals (Examples)
    public const char CommandSeparator = ';';
    public const char ValueSeparator = ',';
    public const char VectorSeparator = ','; // Assuming ModGeneralUtilities uses ','

    #endregion
}