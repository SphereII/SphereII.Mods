namespace SphereII.FoodSpoilage
{
    /// <summary>
    /// Constants used throughout the Food Spoilage system.
    /// </summary>
    public static class SpoilageConstants
    {
        // Feature Keys
        public const string AdvFeatureClass = "FoodSpoilage";
        public const string MainFeature = "FoodSpoilage";
        public const string UseAlternateItemValueFeature = "UseAlternateItemValue";
        public const string FullStackSpoilFeature = "FullStackSpoil";

        // Metadata Keys
        public const string MetaNextSpoilageTick = "NextSpoilageTick";
        public const string MetaSpoilageAmount = "SpoilageValue";
        public const string MetaFreshness = "Freshness";

        // Item Property Keys
        public const string PropSpoilable = "Spoilable";
        public const string PropSpoilageMax = "SpoilageMax";
        public const string PropSpoilagePerTick = "SpoilagePerTick";
        public const string PropTickPerLoss = "TickPerLoss";
        public const string PropSpoiledItem = "SpoiledItem";
        public const string PropPreserveBonus = "PreserveBonus";
        public const string PropFreshnessOnly = "FreshnessOnly";
        public const string PropFullStackSpoil = "FullStackSpoil";
        public const string PropQualityTierColor = "QualityTierColor";
        public const string PropFreshnessBonus = "FreshnessBonus";
        public const string PropFreshnessCVar = "FreshnessCVar";
        public const string PropAltItemTypeIcon = "AltItemTypeIcon"; // Used in ItemInfoWindow

        // Configuration Property Keys
        public const string CfgTickPerLoss = "TickPerLoss";
        public const string CfgSpoiledItem = "SpoiledItem";
        public const string CfgToolbelt = "Toolbelt";
        public const string CfgBackpack = "Backpack";
        public const string CfgContainer = "Container";
        public const string CfgMinimumSpoilage = "MinimumSpoilage";

        // Misc
        public const string DefaultSpoiledItem = "foodRottingFlesh";
        public const string NoneString = "None"; // For SpoiledItem property
        public const string AllString = "all";   // For FreshnessCVar property
    }
}