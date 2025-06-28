public class SCoreConstants
{
    // NPC
    public static readonly float BlockTimeToJump =
        StringParsers.ParseFloat(Configuration.GetPropertyValue("AdvancedNPCFeatures", "BlockTimeToJump"));

    public static readonly float BlockedTime =
        StringParsers.ParseFloat(Configuration.GetPropertyValue("AdvancedNPCFeatures", "BlockedTime"));

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