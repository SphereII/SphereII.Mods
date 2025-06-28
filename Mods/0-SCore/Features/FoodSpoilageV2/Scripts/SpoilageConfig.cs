using System;
using UnityEngine; // For Mathf.Max

namespace SphereII.FoodSpoilage
{
    /// <summary>
    /// Helper class to access and cache Food Spoilage configuration settings.
    /// </summary>
    public static class SpoilageConfig
    {
        // Cache for configuration values to reduce lookups
        private static bool _isInitialized = false;
        private static bool _foodSpoilageEnabled;
        private static bool _useAlternateItemValue;
        private static bool _globalFullStackSpoil;
        private static int _globalTickPerLoss;
        private static string _globalSpoiledItem;
        private static float _toolbeltModifier;
        private static float _backpackModifier;
        private static float _containerModifier;
        private static float _minimumSpoilage;

        private static void Initialize()
        {
            if (_isInitialized) return;

            _foodSpoilageEnabled = Configuration.CheckFeatureStatus(SpoilageConstants.AdvFeatureClass, SpoilageConstants.MainFeature);
            _useAlternateItemValue = Configuration.CheckFeatureStatus(SpoilageConstants.AdvFeatureClass, SpoilageConstants.UseAlternateItemValueFeature);
            _globalFullStackSpoil = Configuration.CheckFeatureStatus(SpoilageConstants.AdvFeatureClass, SpoilageConstants.FullStackSpoilFeature);

            // Parse global values with TryParse for safety
            int.TryParse(Configuration.GetPropertyValue(SpoilageConstants.AdvFeatureClass, SpoilageConstants.CfgTickPerLoss), out _globalTickPerLoss);
            _globalSpoiledItem = Configuration.GetPropertyValue(SpoilageConstants.AdvFeatureClass, SpoilageConstants.CfgSpoiledItem);
            if (string.IsNullOrEmpty(_globalSpoiledItem))
            {
                _globalSpoiledItem = SpoilageConstants.DefaultSpoiledItem;
            }

            float.TryParse(Configuration.GetPropertyValue(SpoilageConstants.AdvFeatureClass, SpoilageConstants.CfgToolbelt), out _toolbeltModifier);
            float.TryParse(Configuration.GetPropertyValue(SpoilageConstants.AdvFeatureClass, SpoilageConstants.CfgBackpack), out _backpackModifier);
            float.TryParse(Configuration.GetPropertyValue(SpoilageConstants.AdvFeatureClass, SpoilageConstants.CfgContainer), out _containerModifier);
            float.TryParse(Configuration.GetPropertyValue(SpoilageConstants.AdvFeatureClass, SpoilageConstants.CfgMinimumSpoilage), out _minimumSpoilage);

            // Ensure minimum spoilage is at least a small positive value
            _minimumSpoilage = Mathf.Max(0.1f, _minimumSpoilage);

            _isInitialized = true;
        }

        // Public accessors that ensure initialization
        public static bool IsFoodSpoilageEnabled => GetInitializedValue(ref _foodSpoilageEnabled);
        public static bool UseAlternateItemValue => GetInitializedValue(ref _useAlternateItemValue);
        public static bool IsGlobalFullStackSpoilEnabled => GetInitializedValue(ref _globalFullStackSpoil);
        public static int GlobalTickPerLoss => GetInitializedValue(ref _globalTickPerLoss);
        public static string GlobalSpoiledItem => GetInitializedValue(ref _globalSpoiledItem);
        public static float ToolbeltModifier => GetInitializedValue(ref _toolbeltModifier);
        public static float BackpackModifier => GetInitializedValue(ref _backpackModifier);
        public static float ContainerModifier => GetInitializedValue(ref _containerModifier);
        public static float MinimumSpoilage => GetInitializedValue(ref _minimumSpoilage);

        // Helper to ensure initialization before returning a value
        private static T GetInitializedValue<T>(ref T value)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            return value;
        }

        // Direct pass-through for CheckFeatureStatus if needed elsewhere, though ideally use specific properties above
        public static bool CheckFeatureStatus(string featureName)
        {
             if (!_isInitialized) Initialize(); // Ensure base system is checked
            return Configuration.CheckFeatureStatus(SpoilageConstants.AdvFeatureClass, featureName);
        }
    }
}