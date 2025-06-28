using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

public class IncludeFilenamePatches
{
    [HarmonyPatch(typeof(Localization))]
    [HarmonyPatch(nameof(Localization.LoadPatchDictionaries))]
    public class IncludeFilenamePatchesLoadPatchDictionaries
    {
        private static bool Prefix(string _modName, string _folder, bool _loadingInGame)
        {
            var text = _folder + "/Localization.txt";
            if (!SdFile.Exists(text)) return true;

            List<string> includedFiles = new List<string>();
            foreach( var line in SdFile.ReadAllLines(text))
            {
                if (!line.StartsWith("<include")) continue;

                var includeFilename = GetFilenameFromString(line);
                if ( string.IsNullOrEmpty(includeFilename)) continue;
                var filename = _folder + "/" + includeFilename;
                if (!SdFile.Exists(filename))
                {
                   Log.Out("[MODS] Included Localization File not found: " + includeFilename);
                    continue;
                }
                if (includedFiles.Contains(filename)) continue;
                includedFiles.Add(filename);
              
            }

            if (includedFiles.Count <= 0) return true;

            Log.Out($"[MODS] Loading Localization Patches for {_modName}...");
            foreach( var file in includedFiles)
            {
                Log.Out($"[MODS] Loading Patched Localization File {file}");
                if (!Localization.LoadCsv(file, true))
                {
                    Log.Out("\t*** Failed to load Patched Localization File: " + file);
                }
            }

            return true;
        }

        private static string GetFilenameFromString(string input)
        {
            string filenameAttribute = "filename=\"";
            int startIndex = input.IndexOf(filenameAttribute, StringComparison.Ordinal);

            if (startIndex == -1)
            {
                // Attribute "filename=" not found
                return null; 
            }

            // Move past 'filename="'
            startIndex += filenameAttribute.Length;

            int endIndex = input.IndexOf('"', startIndex);
            if (endIndex == -1)
            {
                // Closing quote not found
                return null; // Return null tuple
            }

            // Extract the full path value inside the quotes
            string fullPathValue = input.Substring(startIndex, endIndex - startIndex);
            return fullPathValue;
        }
    }

}
