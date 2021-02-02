
using HarmonyLib;
using ICSharpCode.WpfDesign.XamlDom;
using System.IO;
using System.Text;
using System.Xml;
using UnityEngine;

public class SphereII_ModInfo_Patches
{
    [HarmonyPatch(typeof(Mod))]
    [HarmonyPatch("LoadFromFolder")]
    public class SphereII_ModInfo_LoadFromFolder
    {
        public static bool Prefix(Mod __instance, string _path)
        {
            string text = _path + "/ModInfo.xml";
            string fileName = Path.GetFileName(_path);
            if (!File.Exists(text))
                return false;

            string _content = File.ReadAllText(text);
            PositionXmlDocument positionXmlDocument = new PositionXmlDocument();
            try
            {
                using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(_content ?? "")))
                {
                    positionXmlDocument.Load(XmlReader.Create(stream));
                }
            }
            catch (XmlException e)
            {
                return true;  // Let vanilla deal with it.
            }

            // Scan for Enabled XML property and decided to enable or disable it.
            XmlNode documentElement = positionXmlDocument.DocumentElement;
            XmlNode enabledNode = documentElement.SelectSingleNode("//Enabled");
            if (enabledNode != null )
            {
                XmlElement _elem = enabledNode as XmlElement;
                if ( _elem != null )
                {
                    if ( _elem.HasAttribute("value"))
                    {
                        bool enabled = StringParsers.ParseBool(_elem.GetAttribute("value"));
                        if (enabled == false)
                        {
                            return false;
                        }
                    }
                    
                }
            }
            return true;

        }


    }

}