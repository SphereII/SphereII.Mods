public static class SCoreLocalizationHelper
{
    public static string GetLocalization(string str)
    {
        var result = Localization.Get(str);
        if (result.StartsWith(str))
        {
            result = GetLocalizationName(str);
        }
        if (result.StartsWith(str))
        {
            result = GetLocalizationDesc(str);
        }
        if (result.StartsWith(str))
        {
            result = GetLocalizationLongDesc(str);
        }

        return result;

    }
    
    // Localized text for what a book or schematic unlocks. The key guesses in GetLocalization
    // only work when the perk's localization keys share its name; vanilla breaks that
    // (perkLuckyLooterAmmunition uses perkLuckyLooterAmmoDesc), and so can modded books.
    // Fall back to the keys progression.xml declares, then to the book's own name, so the
    // player never sees a raw key.
    public static string GetUnlockLocalization(ItemClass itemClass)
    {
        if (itemClass == null) return string.Empty;
        var unlock = itemClass.Properties.GetString("Unlocks");
        if (string.IsNullOrEmpty(unlock)) return itemClass.GetLocalizedItemName();

        var result = GetLocalization(unlock);
        if (!result.StartsWith(unlock)) return result;

        if (Progression.ProgressionClasses != null &&
            Progression.ProgressionClasses.TryGetValue(unlock, out var progressionClass))
        {
            foreach (var key in new[] { progressionClass.DescKey, progressionClass.LongDescKey, progressionClass.NameKey })
            {
                if (string.IsNullOrEmpty(key) || !Localization.Exists(key)) continue;
                return Localization.Get(key);
            }
        }

        return itemClass.GetLocalizedItemName();
    }

    public static string GetLocalizationName(string str)
    {
        return Localization.Get(str + "Name");
    }

    public static string GetLocalizationDesc(string str)
    {
        return Localization.Get(str + "Desc");
    }

    public static string GetLocalizationLongDesc(string str)
    {
        return Localization.Get(str + "LongDesc");
    }
    
}
