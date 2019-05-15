using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

                    // If the hashes match, don't bump the version
                    if (CheckHash(mod.Location) == false)
                        continue;


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

                            Logging.Log("Bumping Version Number for " + mod.Name);
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
                        Logging.Log(" Version number bump is skipped: Version Format is expected:  x.x.x.x.   Got: " + mod.ModVersion);
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

        public bool CheckHash(String strPath)
        {
            bool BumpNumbers = false;
            String strXMLFile = Path.Combine(strPath, "mod.xml");
            if (!File.Exists(strXMLFile))
                return false;

            // Load up the XML document and check if there's a hashes node; if so, select it so we can add our files, otherwise create it.
            XmlDocument doc = new XmlDocument();
            doc.Load(strXMLFile);
            XmlNode node = doc.SelectSingleNode("/mod/hashes");
            if (node == null)
            {
                XmlElement Hash = doc.CreateElement("hashes");
                doc.DocumentElement.AppendChild(Hash);
                node = doc.SelectSingleNode("/mod/hashes");
            }

            // Loop through all the files, skipping some normally inconsistent files and generating hashes for them
            foreach (string strFile in Directory.GetFiles(strPath, "*", SearchOption.AllDirectories))
            {
                if (strFile.EndsWith("mod.xml") || strFile.EndsWith("ModInfo.xml") || strFile.EndsWith(".dll") || strFile.EndsWith("pdb"))
                    continue;
                String strHash = CalculateMD5(strFile);

                // New hash node for each individual file.
                XmlElement File = doc.CreateElement("hash");

                // Store the folder and the file in this path, unless it's top level, then just leave it blank.
                XmlAttribute filename = doc.CreateAttribute("filename");
                filename.InnerText = strFile.Replace(strPath + "\\" , "");

                XmlAttribute hash = doc.CreateAttribute("hash");
                hash.InnerText = strHash;

                File.Attributes.Append(filename);
                File.Attributes.Append(hash);
                
                // Check if the node exists already
                string strXPath = "/mod/hashes/hash[@filename='" + filename.InnerText + "']";
                XmlNode existingNode = doc.SelectSingleNode(strXPath);
                if ( existingNode == null)
                {
                    BumpNumbers = true; // New file detected.
                    node.AppendChild(File);
                    existingNode = doc.SelectSingleNode(strXPath);
                }

                // Check if the existing hash matches the new hash. If it doesn't, then removing the existing one and add the new one.
                if (existingNode.Attributes["hash"].Value != hash.InnerText.ToString() )
                {
                    BumpNumbers = true;
                    node.RemoveChild(existingNode);
                    node.AppendChild(File);
                }
            }

            if ( BumpNumbers )
                doc.Save( strXMLFile );
            return BumpNumbers;
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

    }
}
