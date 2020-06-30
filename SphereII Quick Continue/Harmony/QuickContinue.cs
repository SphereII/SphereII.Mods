using DMT;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

public class SphereII_QuickLoad
{
  
    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_Main_Menu_AutoClick
    {
        static void Postfix(XUiC_MainMenu __instance)
        {
            if (SphereIIToggleCapsLock.GetScrollLock())
            {
                if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) == 0)
                {
                    GamePrefs.Set(EnumGamePrefs.AutopilotMode, 1);
                    MethodInfo method = __instance.GetType().GetMethod("btnContinueGame_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                    method.Invoke(__instance, new object[] { null, null });
                }
                else if (GamePrefs.GetInt(EnumGamePrefs.AutopilotMode) == 1)
                {
                    MethodInfo method = __instance.GetType().GetMethod("btnQuit_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                    method.Invoke(__instance, new object[] { null, null });
                }
            }
        }
    }

    [HarmonyPatch(typeof(XUiC_NewContinueGame))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_XUIC_NewContinueGame
    {
        static void Postfix(XUiC_NewContinueGame __instance)
        {
            if (SphereIIToggleCapsLock.GetScrollLock())
            {
                MethodInfo method = __instance.GetType().GetMethod("BtnStart_OnPressed", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(__instance, new object[] { null, null });
            }
        }
    }

}



