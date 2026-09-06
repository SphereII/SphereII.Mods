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

    /// <summary>
    /// Quality points that make up one colour band, when nothing is configured.
    /// </summary>
    public const int DefaultQualityPerTier = 100;

    /// <summary>
    /// Quality points per colour band. A 0,600 QualityLevels range wants 100 - one band per
    /// hundred points. A 1,100 range wants 20.
    /// </summary>
    public static int GetQualityPerTier()
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, "QualityPerTier");
        if (string.IsNullOrEmpty(value)) return DefaultQualityPerTier;

        var perTier = StringParsers.ParseSInt32(value);

        // A zero or negative band width would divide by zero or run the tiers backwards.
        return perTier > 0 ? perTier : DefaultQualityPerTier;
    }

    /// <summary>
    /// Colour band that the lowest slice of quality lands on.
    /// <para>
    /// Zero - the default - leaves band zero on tier 0, so quality below one full band reads as
    /// broken grey. That suits a 0,600 range, where an item under 100 really is nearly destroyed.
    /// One shifts every band up, so the lowest quality already reads as tier 1 brown and there is
    /// no grey band. That suits a 1,100 range, where quality 0 is the only broken state.
    /// </para>
    /// </summary>
    public static int GetQualityTierOffset()
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, "QualityTierOffset");
        if (string.IsNullOrEmpty(value)) return 0;

        return StringParsers.ParseSInt32(value);
    }

    /// <summary>
    /// Maps a quality value onto one of the colour bands defined in qualityinfo.xml.
    /// <para>
    /// The colour array is a fixed seven entries in the base game, so the result is always clamped
    /// into it however wide the configured quality range is.
    /// </para>
    /// </summary>
    public static int CalculateTier(int quality)
    {
        if (quality <= 0) return 0;

        var tier = quality / GetQualityPerTier() + GetQualityTierOffset();
        return Mathf.Clamp(tier, 0, QualityInfo.qualityColors.Length - 1);
    }

    public static Color GetColor(int quality)
    {
        return QualityInfo.qualityColors[CalculateTier(quality)];
    }

    public static string GetColorHex(int quality)
    {
        var tier = Mathf.Min(CalculateTier(quality), QualityInfo.hexColors.Length - 1);
        return QualityInfo.hexColors[tier];
    }
}
