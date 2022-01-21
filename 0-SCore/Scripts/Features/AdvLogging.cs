public static class AdvLogging
{
    public static void DisplayLog(string AdvFeatureClass, string strDisplay)
    {
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "Logging"))
            return;

        if (Configuration.CheckFeatureStatus("AdvancedLogging", "LowOutput"))
            Log.Out($"{strDisplay}");
        else
            Log.Out($"{AdvFeatureClass} :: {strDisplay}");
    }

    
    public static void DisplayLog(string AdvFeatureClass, string Feature, string strDisplay)
    {

        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            return;

        if (Configuration.CheckFeatureStatus("AdvancedLogging", "LowOutput"))
            Log.Out($"{strDisplay}");
        else
            Log.Out($"{AdvFeatureClass} :: {Feature} :: {strDisplay}");
    }

    public static bool LogEnabled(string AdvFeatureClass)
    {
        return Configuration.CheckFeatureStatus(AdvFeatureClass);
    }

    public static bool LogEnabled(string AdvFeatureClass, string Feature)
    {
        return Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
    }
}