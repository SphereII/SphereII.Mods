using UnityEngine;

public static class QualityUtils
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "QualityLevels";

    public static int MaxQuality = 6;
    public static int GetMaxQuality()
    {
     //   return MaxQuality;
        var qualityRange = Configuration.GetPropertyValue(AdvFeatureClass, Feature);
        if (string.IsNullOrEmpty(qualityRange)) return MaxQuality;
        
        var range = StringParsers.ParseVector2i(qualityRange);
        return range.y;
    }
    
    public static int GetQualityStage()
    {
        var qualityStage = Configuration.GetPropertyValue(AdvFeatureClass, "QualityStages");
        if (string.IsNullOrEmpty(qualityStage)) return 1;
       
        return StringParsers.ParseSInt32(qualityStage);
    }

 
    public static int GetMinQuality()
    {
        var qualityRange = Configuration.GetPropertyValue(AdvFeatureClass, Feature);
        if (string.IsNullOrEmpty(qualityRange)) return 0;
        
        var range = StringParsers.ParseVector2i(qualityRange);
        return range.x;
    }

    public static int CalculateTier(int quality)
    {
        if (quality == 0) return 0;
        var tier = Mathf.Clamp(quality / 100, GetMinQuality(), GetMaxQuality());
        if ( tier > QualityInfo.qualityColors.Length )
            tier = 6;
        return tier;
    }

    public static Color GetColor(int tier)
    {
        tier = CalculateTier(tier);
        
        if ( tier > QualityInfo.qualityColors.Length -1)
            tier = QualityInfo.qualityColors.Length -1;
        return QualityInfo.qualityColors[tier];
    }

    public static string GetColorHex(int tier)
    {
        tier = CalculateTierHex(tier);
        if ( tier > QualityInfo.hexColors.Length -1)
            tier = QualityInfo.hexColors.Length -1;
        return QualityInfo.hexColors[tier];
    }

    private static int CalculateTierHex(int quality)
    {
        if (quality == 0) return 0;
        var tier = Mathf.Clamp(quality / 100, GetMinQuality(), GetMaxQuality());
        if ( tier > QualityInfo.hexColors.Length )
            tier = 6;
        return tier;
    }
}
