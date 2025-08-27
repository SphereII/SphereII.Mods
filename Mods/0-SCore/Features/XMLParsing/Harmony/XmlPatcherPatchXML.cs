using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.XMLParsing.Harmony
{
    [HarmonyPatch(typeof(XmlPatcher))]
    [HarmonyPatch(nameof(XmlPatcher.PatchXml))]
    public class XmlPatcherPatchXml
    {
        public static bool Prefix(XmlFile _patchFile, Mod _patchingMod)
        {
            // Search for any nodes with ref_file.
            List<XElement> includeElement = _patchFile.XmlDoc.Descendants("ref_file").ToList();
            if (includeElement.Count == 0) return true;

            foreach (var include in includeElement)
            {
                // Look for the snippet attribute, which will contain our code.
                var includePath = include.Attribute("snippet")?.Value;
                if (string.IsNullOrEmpty(includePath)) continue;

                var labels = include.Attribute("label")?.Value;
                
                // Don't support cross-mod loading.
                var filePath = Path.Combine(_patchingMod.Path, "Config", includePath);
                var externalDoc = XDocument.Load(filePath);
                if (externalDoc?.Root == null) continue;
                List<XNode> nodesToInsert = new List<XNode>();
                if (string.IsNullOrEmpty(labels))
                {
                    Debug.Log($"Adding Snippet File: {includePath}...");
                    nodesToInsert = externalDoc.Root.Nodes().ToList();
                }
                else
                {
                    Debug.Log($"Processing Snippet Labels: {labels}");
                    foreach (var label in labels.Split(','))
                    {
                        nodesToInsert.AddRange(externalDoc.XPathSelectElements($"//*[@label='{label.Trim()}']/*")
                            .ToList());
                    }
                    
                }
                include.ReplaceWith(nodesToInsert);
            }

            return true;
        }
    }
}