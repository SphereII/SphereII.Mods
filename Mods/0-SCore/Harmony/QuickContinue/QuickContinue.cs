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
     
            _xuiInstance.playerUI.windowManager.Open(XUiC_MainMenu.ID, true, false);
            return false;

        }
    }

    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_Main_Menu_AutoClick
    {
        // Set once the first main menu has auto-continued. This used to be stored in the game's
        // AutopilotMode pref, but the game quits on every server stop while AutopilotMode > 0,
        // which closed the game when starting a Play Test from the Prefab Editor.
        private static bool _autoContinued;

        private static void Postfix(XUiC_MainMenu __instance)
        {
            var autoStart = false;
            for (var i = 0; i < Environment.GetCommandLineArgs().Length; i++)
                if (Environment.GetCommandLineArgs()[i].EqualsCaseInsensitive("-autostart"))
                    autoStart = true;

            if (!autoStart) return;

            if (!_autoContinued)
            {
                _autoContinued = true;
                var method = __instance.GetType().GetMethod("btnContinueGame_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(__instance, new object[] { null, null });
            }
            else
            {
                var method = __instance.GetType().GetMethod("btnQuit_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                method?.Invoke(__instance, new object[] { null, null });
            }


        }
    }

    [HarmonyPatch(typeof(XUiC_ContinueGame))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_XUIC_NewContinueGame
    {
        private static void Postfix(XUiC_ContinueGame __instance)
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