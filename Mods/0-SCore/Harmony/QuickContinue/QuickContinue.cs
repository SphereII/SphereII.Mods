using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using Platform;
using UnityEngine;

public class SCore_QuickLoad
{

    // If steam is not detected, we want the game to go into Offline mode.
    // This can happen even if Steam is running, but at a different permission level.
    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch(nameof(XUiC_MainMenu.Open))]
    public class SphereII_XUiC_MainMenuOpen
    {
        private static bool Prefix(XUiC_MainMenu __instance, XUi _xuiInstance)
        {
            var autoStart = false;
            for (var i = 0; i < Environment.GetCommandLineArgs().Length; i++)
            {
                if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-skipnewsscreen"))
                    autoStart = true;
                if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-autostart"))
                    autoStart = true;

            }

            if (!autoStart) return true;
     
            _xuiInstance.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false, true);
            return false;

        }
    }

    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_Main_Menu_AutoClick
    {
        private static void Postfix(XUiC_MainMenu __instance)
        {
            var autoStart = false;
            for (var i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-autostart"))
                    autoStart = true;

            if (!autoStart) return;

            if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) == 0)
            {
                GamePrefs.Set(EnumGamePrefs.AutopilotMode, 1);
                var method = __instance.GetType().GetMethod("btnContinueGame_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(__instance, new object[] { null, null });
            }
            else if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) == 1)
            {
                var method = __instance.GetType().GetMethod("btnQuit_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(__instance, new object[] { null, null });
            }


        }
    }

    [HarmonyPatch(typeof(XUiC_NewContinueGame))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_XUIC_NewContinueGame
    {
        private static void Postfix(XUiC_NewContinueGame __instance)
        {
            var autoStart = false;
            for (var i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-autostart"))
                    autoStart = true;

            if (!autoStart) return;

            var method = __instance.GetType().GetMethod("BtnStart_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(__instance, new object[] { null, null });
        }
    }
}