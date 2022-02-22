using HarmonyLib;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class SCore_QuickLoad
{

    // If steam is not detected, we want the game to go into Offline mode.
    // This can happen even if Steam is running, but at a different permission level.
    [HarmonyPatch(typeof(XUiC_SteamLogin))]
    [HarmonyPatch("updateState")]
    public class SphereII_SteamLoginAutoLogin
    {
        private static void Postfix(XUiC_SteamLogin __instance)
        {
            var autoStart = false;
            for (var i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-autostart"))
                    autoStart = true;

            if (!autoStart) return;

            Log.Out("SphereII Quick Continue Modlet is activated. Game is going into Offline mode. Multi-player disabled as steam was not detected.");
            var method = __instance.GetType().GetMethod("BtnOffline_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(__instance, new object[] { null, null });
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