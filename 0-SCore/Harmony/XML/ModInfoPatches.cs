using HarmonyLib;
using ICSharpCode.WpfDesign.XamlDom;
using System.IO;
using System.Text;
using System.Xml;

namespace Harmony.XML
{
    public class ModInfoPatches
    {
        [HarmonyPatch(typeof(Mod))]
        [HarmonyPatch("LoadFromFolder")]
        public class ModInfoLoadFromFolder
        {
            public static bool Prefix(Mod __instance, string _path)
            {
                var text = _path + "/ModInfo.xml";
                if (!File.Exists(text))
                    return false;

                var content = File.ReadAllText(text);
                var positionXmlDocument = new PositionXmlDocument();
                try
                {
                    using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
                    {
                        positionXmlDocument.Load(XmlReader.Create(stream));
                    }
                }
                catch (XmlException)
                {
                    return true; // Let vanilla deal with it.
                }

                // Scan for Enabled XML property and decided to enable or disable it.
                XmlNode documentElement = positionXmlDocument.DocumentElement;
                var enabledNode = documentElement?.SelectSingleNode("//Enabled");

                if (!(enabledNode is XmlElement elem)) return true;

                if (!elem.HasAttribute("value")) return true;

                var enabled = StringParsers.ParseBool(elem.GetAttribute("value"));
                return enabled;
            }
        }
    }
}