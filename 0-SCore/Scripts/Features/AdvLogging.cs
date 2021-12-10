public static class AdvLogging
{
    public static void DisplayLog(string AdvFeatureClass, string strDisplay)
    {
        if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Logging"))
            Log.Out($"{AdvFeatureClass} :: {strDisplay}");
    }

    public static void DisplayLog(string AdvFeatureClass, string Feature, string strDisplay)
    {
        if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            Log.Out($"{AdvFeatureClass} :: {Feature} :: {strDisplay}");


    }
}