using System;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;
using System.IO;
using System.Linq;


public class VersionCheck
{
    private static XDocument configDoc;
    private const string ConfigFileName = "versioncheck.xml";
    private static string title;
    private static bool versionMismatchShown = false;

    public static bool LoadConfig()
    {
        string configPath = FindConfigFile();
        if (string.IsNullOrEmpty(configPath))
        {
            return false;
        }

        try
        {
            configDoc = XDocument.Load(configPath);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[VersionCheckMod] Error loading configuration file: {ex.Message}");
            return false;
        }
    }

    private static string FindConfigFile()
    {
        // Search for the config file in all possible mod directories
        foreach (string modPath in ModManager.GetLoadedMods().Select(mod => mod.Path))
        {
            string fullPath = Path.Combine(modPath, ConfigFileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        //   Debug.LogWarning($"[VersionCheckMod] Configuration file {ConfigFileName} not found in any mod directory.");
        return null;
    }

    private static string GetConfigString(string key)
    {
        return configDoc?.Root?.Element("Settings")?.Element(key)?.Value ?? string.Empty;
    }

    private static bool GetConfigBool(string key)
    {
        return bool.TryParse(GetConfigString(key), out bool result) && result;
    }

    public static void OnGameStartDone(ref ModEvents.SGameStartDoneData data)
    {
        CheckVersions();
    }

    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class XUiC_MainMenu_OnOpen_Patch
    {
        static void Postfix(XUiC_MainMenu __instance)
        {
            if (!GameManager.IsDedicatedServer)
            {
                CheckVersions(__instance.xui);
            }
        }
    }

    private static void CheckVersions(XUi xui = null)
    {
        if (LoadConfig() == false)
        {
            return;
        }

        if (versionMismatchShown)
        {
            return;
        }

        string gameVersion = GetGameVersion();
        string modVersion = GetConfigString("ModVersion");

        if (gameVersion == modVersion) return;

        if (GameManager.IsDedicatedServer)
        {
            var descriptionFormat = "score_versioncheck_mismatchDesc";
            string message = string.Format(Localization.Get(descriptionFormat), gameVersion, modVersion);
            Log.Error(message);
            versionMismatchShown = true;
            return;
        }

        if (xui != null)
        {
            DisplayVersionMismatchMessage(xui, gameVersion, modVersion);
            versionMismatchShown = true;
        }
        
    }

    private static string GetGameVersion()
    {
        return $"{Constants.cVersionMajor}.{Constants.cVersionMinor}.{Constants.cVersionBuild}";
    }

    private static void DisplayVersionMismatchMessage(XUi xui, string gameVersion, string modVersion)
    {
        title = configDoc?.Root?.Element("Settings")?.Element("ErrorMessage")?.Element("Title")?.Value
                ?? "score_versioncheck_mismatchTitle";
        string descriptionFormat =
            configDoc?.Root?.Element("Settings")?.Element("ErrorMessage")?.Element("Description")?.Value
            ?? "score_versioncheck_mismatchDesc";

        string message = string.Format(Localization.Get(descriptionFormat), gameVersion, modVersion);

        // Display the message box
        XUiC_MessageBoxWindowGroup.ShowMessageBox(
            xui,
            Localization.Get(title),
            message,
            XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel,
            () => Application.Quit(), // Left button (Quit)
            () => {
                // Right button (Continue)
                xui.playerUI.windowManager.CloseAllOpenWindows();
                xui.playerUI.windowManager.Open("mainmenu", true);
            },
            true,
            false
        );
    }

    [HarmonyPatch(typeof(XUiC_MessageBoxWindowGroup))]
    [HarmonyPatch(nameof(XUiC_MessageBoxWindowGroup.GetBindingValueInternal))]

    public class MessageBoxWindowGroupPatch
    {
        [HarmonyPostfix]
        public static void Postfix(XUiC_MessageBoxWindowGroup __instance, ref bool __result, ref string _value,
            string _bindingName)
        {
            // Check if it's the specific message box we want to modify
            if (__instance.MessageBoxType == XUiC_MessageBoxWindowGroup.MessageBoxTypes.OkCancel &&
                __instance.Title == Localization.Get(title)) // Replace 'title' with the exact title you're using
            {
                switch (_bindingName)
                {
                    case "leftbuttontext":
                        _value = Localization.Get("xuiQuit");
                        __result = true;
                        break;
                    case "rightbuttontext":
                        _value = Localization.Get("btnContinue");
                        __result = true;
                        break;
                }
            }
        }
    }
}