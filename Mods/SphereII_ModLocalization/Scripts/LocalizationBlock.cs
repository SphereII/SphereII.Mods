using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using Harmony;
using System.Collections.Generic;

class BlockLocalizationBlock : Block
{
    private static readonly string HeaderKey = "KEY";

    
    public override void Init()
    {
        // Disables the extra debugging information we see in logs:
        // (Filename: C:\buildslave\unity\build\Runtime/Export/Debug.bindings.h Line: 43)
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);


        Debug.Log("SphereII - Localization Gap Solution");
        Debug.Log("Initializing Localization Block...");
        String strModFolder = Utils.GetGameDir("Mods");

        Debug.Log("Searching in: " + strModFolder);
        Debug.Log("Searching for Localization.txt...");
        LocalizationSearch(strModFolder, "Localization.txt");

        Debug.Log("Searching for ModLocalization - Quests.txt...");
        LocalizationSearch(strModFolder, "Localization - Quest.txt");
    }

    // Search in all the sub folders for the localization files.
    private void LocalizationSearch(string sDir, string strSearchFile )
    {
        try
        {
            foreach (string f in Directory.GetFiles(sDir, strSearchFile ))
            {
                String strPath = Path.Combine(sDir, f);
                Debug.Log("Found : " + strPath);

                LoadDictionary(strPath);
            }

            foreach (string d in Directory.GetDirectories(sDir))
            {
                this.LocalizationSearch(d, strSearchFile);
            }
        }
        catch (System.Exception excpt)
        {
            Debug.Log("Error: " + excpt.ToString());
        }
    }

    // Read up each localization file and convert them to csv
    private static bool LoadDictionary( string strModdedLocalization )
    {
        byte[] asset = File.ReadAllBytes( strModdedLocalization );
        if (LoadCSV(asset, string.Empty))
        {
            byte[] asset2 = File.ReadAllBytes( strModdedLocalization);
            LoadCSV(asset2, Localization.QuestPrefix);
            return true;
        }
        return false;
    }

    private static bool LoadCSV(byte[] _asset, string _keysPrefix )
    {
        ByteReader byteReader = new ByteReader(_asset);
        BetterList<string> betterList = byteReader.ReadCSV();
        if (betterList.size < 2)
        {
            return false;
        }
        betterList[0] = HeaderKey;
        if (!string.Equals(betterList[0], HeaderKey))
        {
            Debug.LogError(string.Concat(new string[]
            {
                "Invalid localization CSV file. The first value is expected to be '",
                HeaderKey,
                "', followed by language columns.\nInstead found '",
                betterList[0],
                "'"
            }));
            return false;
        }

    
        while (betterList != null)
        {
            AddCSV(betterList, _keysPrefix);
            betterList = byteReader.ReadCSV();
        }
        return true;
    }

    public static void AddCSV(global::BetterList<string> _temp, string _prefix)
    {
        if (_temp.size < 2)
        {
            return;
        }
        string[] array = new string[_temp.size - 1];
        for (int i = 1; i < _temp.size; i++)
        {
            array[i - 1] = _temp[i];
        }
        string text = _temp[0];
        if (!string.IsNullOrEmpty(text))
        {
            string text2 = _prefix + text;
            if (Localization.Dictionary.ContainsKey(text2))
                Localization.Dictionary.Remove(text2);
                
            Localization.Dictionary.Add(text2, array);
        }
    
    }
}

