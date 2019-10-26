using UnityEngine;
public static class AdvLogging
{

    public static void DisplayLog(string AdvFeatureClass, string strDisplay)
    {
        if(Configuration.CheckFeatureStatus(AdvFeatureClass, "Logging"))
            Debug.Log(AdvFeatureClass + " :: " + strDisplay);
    }
}

