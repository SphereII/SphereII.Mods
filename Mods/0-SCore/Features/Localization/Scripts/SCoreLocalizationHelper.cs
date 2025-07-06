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
