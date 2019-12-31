using Harmony;
using System;
using System.Collections;
using System.Xml;
using UnityEngine;


public class SphereII_XML_Debugging
{
    private static string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
    private static string Feature = "VerboseXMLParser";

    [HarmonyPatch(typeof(XmlPatcher))]
    [HarmonyPatch("singlePatch")]
    public class SphereII_XmlPatcher_SinglePatch
    {

        static bool Prefix(XmlFile _targetFile, XmlElement _patchElement, string _patchName)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            String strDisplay = _patchName + ": Attempting to Patch: " + _patchElement.GetAttribute("xpath");
            AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);

            return true;
        }
    }

    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch("ParseResponse")]
    public class SphereII_DialogFromXML
    {

        static bool Prefix(Dialog dialog, XmlElement e)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            String strDisplay = "";
            if (dialog != null)
            {
                strDisplay = "DialogFromXML ParseREsponse: Dialog: " + dialog.ID;
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
            }

            if (e != null)
            {
                strDisplay = "DialogFromXML ParseREsponse: Element: " + e.OuterXml.Trim();
                AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
            }

            return true;
        }
    }
}


