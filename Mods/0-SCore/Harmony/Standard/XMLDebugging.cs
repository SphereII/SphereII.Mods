// using HarmonyLib;
// using System.Xml;
//
// namespace Harmony.Standard
// {
//     /**
//      * SCoreXML_Debugging
//      * 
//      * This class includes a Harmony patch allows additiona logging features to be turned on during XML parsing, enabled through the Config/blocks.xml' VerboseXMLParser and PhysicsBody.
//      */
//     public class XMLDebugging
//     {
//         private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
//         private static readonly string Feature = "VerboseXMLParser";
//         private static readonly string SecondFeature = "PhysicsBody";
//
//         [HarmonyPatch(typeof(XmlPatcher))]
//         [HarmonyPatch("singlePatch")]
//         public class XmlPatcherSinglePatch
//         {
//             private static bool Prefix(XmlFile _targetFile, XmlElement _patchElement, string _patchName)
//             {
//                 if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//                     return true;
//
//                 var strDisplay = _patchName + ": Attempting to Patch: " + _patchElement.GetAttribute("xpath");
//                 AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
//
//                 return true;
//             }
//         }
//
//         [HarmonyPatch(typeof(DialogFromXml))]
//         [HarmonyPatch("ParseResponse")]
//         public class SCoreDialogFromXML
//         {
//             private static bool Prefix(global::Dialog dialog, XmlElement e)
//             {
//                 if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
//                     return true;
//
//                 var strDisplay = "";
//                 if (dialog != null)
//                 {
//                     strDisplay = "DialogFromXML ParseResponse: Dialog: " + dialog.ID;
//                     AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
//                 }
//
//                 if (e != null)
//                 {
//                     strDisplay = "DialogFromXML ParseResponse: Element: " + e.OuterXml.Trim();
//                     AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
//                 }
//
//                 return true;
//             }
//         }
//
//         [HarmonyPatch(typeof(PhysicsBodyColliderConfiguration))]
//         [HarmonyPatch("Read")]
//         public class PhysicsBodyColliderConfigurationRead
//         {
//             private static void CheckTag(DynamicProperties dynamicProperties, string strTag)
//             {
//                 var strDisplay = "Property: " + strTag;
//                 if (dynamicProperties.Contains(strTag))
//                     strDisplay += " Value: " + dynamicProperties.GetStringValue(strTag);
//                 else
//                     strDisplay += " Property is not set.";
//
//                 AdvLogging.DisplayLog(AdvFeatureClass, strDisplay);
//             }
//
//             private static bool Prefix(XmlElement _e)
//             {
//                 if (!Configuration.CheckFeatureStatus(AdvFeatureClass, SecondFeature))
//                     return true;
//
//                 var physicsBodyColliderConfiguration = new PhysicsBodyColliderConfiguration();
//                 var dynamicProperties = new DynamicProperties();
//                 foreach (var obj in _e.ChildNodes)
//                 {
//                     var xmlNode = (XmlNode)obj;
//                     if (xmlNode.NodeType == XmlNodeType.Element && xmlNode.Name.Equals("property")) dynamicProperties.Add(xmlNode);
//                 }
//
//                 AdvLogging.DisplayLog(AdvFeatureClass, "\n======= Physics Bodies ============");
//                 CheckTag(dynamicProperties, "tag");
//                 CheckTag(dynamicProperties, "path");
//                 CheckTag(dynamicProperties, "collisionLayer");
//                 CheckTag(dynamicProperties, "ragdollLayer");
//                 CheckTag(dynamicProperties, "layer");
//                 CheckTag(dynamicProperties, "ragdollScale");
//                 CheckTag(dynamicProperties, "type");
//                 CheckTag(dynamicProperties, "flags");
//
//                 if (dynamicProperties.Contains("collisionLayer"))
//                 {
//                     if (!dynamicProperties.Contains("ragdollLayer"))
//                         AdvLogging.DisplayLog(AdvFeatureClass, "\tWARNING: Collision Layer is set, but does not contain a ragdollLayer");
//                 }
//                 else
//                 {
//                     if (!dynamicProperties.Contains("layer"))
//                         AdvLogging.DisplayLog(AdvFeatureClass, "\tWARNING: Collision Layer IS NOT SET. Falling back to layer property, but that is not found either! ");
//                 }
//
//                 AdvLogging.DisplayLog(AdvFeatureClass, "======= End Physics Bodies ============");
//                 return true;
//             }
//         }
//     }
// }