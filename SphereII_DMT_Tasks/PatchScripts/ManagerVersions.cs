using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using DMT.Attributes;

namespace DMT.Tasks
{
    [RunOrder(RunSection.InitialPatch, RunOrder.Early)]
    public class ManageVersionNumbers : BaseTask
    {

        public override bool Patch(PatchData data)
        {
            try
            {
                Logging.Log("Checking version numbers");

                IList<ModInfo> enabledMods = data.ActiveMods;
                for(int i = 0; i < enabledMods.Count; i++)
                {
                    ModInfo mod = enabledMods[i];
                    if(i > 0)
                    {
                        Logging.NewLine();
                    }

                    try
                    {
                        Version ver = new Version(mod.ModVersion);
                        Version NewVersion = new Version(ver.Major, ver.Minor, ver.Build, ver.Revision + 1);
                        mod.ModVersion = NewVersion.ToString();

                        // Update the Mod.xml if found.
                        String strModLocation = Path.Combine(mod.Location, "mod.xml");
                        if(File.Exists(strModLocation))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(strModLocation);
                            XmlNode node = doc.SelectSingleNode("/mod/info/mod_version");
                            if(node != null)
                                node.InnerText = mod.ModVersion;

                            doc.Save(strModLocation);
                        }

                        // If there's an existing ModInfo.xml, update that as well.
                        strModLocation = Path.Combine(mod.Location, "ModInfo.xml");
                        if(File.Exists(strModLocation))
                        {
                            XmlDocument doc = new XmlDocument();
                            doc.Load(strModLocation);
                            XmlNode node = doc.SelectSingleNode("/xml/ModInfo/Version");
                            if(node != null)
                                node.Attributes["value"].Value = mod.ModVersion;
                            doc.Save(strModLocation);
                        }
                    }
                    catch(Exception ex)
                    {
                        Logging.Log(" Version Format is expected:  x.x.x.x.   Got: " + mod.ModVersion);
                        //Logging.Log(" Exception: " + ex.ToString());
                        // Version number can't be updated.
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                Logging.LogError(ex.Message);
            }

            return false;
        }


    }
}
