using Harmony;
using System;
using System.Collections;
using System.Xml;
using UnityEngine;

[HarmonyPatch(typeof(XmlPatcher))]
[HarmonyPatch("singlePatch")]
public class SphereII_XmlPatcher_SinglePatch
{
    private static string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
    private static string Feature = "VerboseXMLParser";

    static bool Prefix(XmlFile _targetFile, XmlElement _patchElement, string _patchName)
    {
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            return true;

        String strDisplay = _patchName + ": Attempting to Patch: " + _patchElement.GetAttribute("xpath");
        AdvLogging.DisplayLog(AdvFeatureClass,  strDisplay);

        return true;
    }
}


