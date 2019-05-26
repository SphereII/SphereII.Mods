using DMT;
using Harmony;
using System;
using System.Reflection;
using UnityEngine;

public class SphereII_QuickLoad : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_Main_Menu_AutoClick
    {
        static void Postfix(XUiC_MainMenu __instance)
        {
            if(SphereIIToggleCapsLock.GetScrollLock())

            {
                XUiC_NewContinueGame.SetIsContinueGame(__instance.xui, true);
                ProfileSDF profileSDF = new ProfileSDF();
                __instance.xui.playerUI.windowManager.Close("mainMenu");
                if(profileSDF.GetSelectedProfile().Length == 0)
                {
                    XUiC_OptionsProfiles.Open(__instance.xui, delegate
                    {
                        __instance.xui.playerUI.windowManager.Open(XUiC_NewContinueGame.ID, true, false, true);
                    });
                }
                else
                {
                    __instance.xui.playerUI.windowManager.Open(XUiC_NewContinueGame.ID, true, false, true);
                }

            }
        }

        [HarmonyPatch(typeof(XUiC_NewContinueGame))]
        [HarmonyPatch("OnOpen")]
        public class SphereII_XUIC_NewContinueGame
        {
            static void Postfix(XUiC_NewContinueGame __instance)
            {
                if(SphereIIToggleCapsLock.GetScrollLock())
                {
                    __instance.xui.playerUI.windowManager.Close("newContinueGame");
                    GamePrefs.SetPersistent(EnumGamePrefs.GameMode, true);
                    NetworkConnectionError networkConnectionError = Steam.Network.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword));
                    if(networkConnectionError != NetworkConnectionError.NoError)
                    {
                        XUiWindowGroup xuiWindowGroup = (XUiWindowGroup)__instance.xui.playerUI.windowManager.GetWindow(XUiC_MessageBoxWindowGroup.ID);
                        ((XUiC_MessageBoxWindowGroup)xuiWindowGroup.Controller).ShowNetworkError(networkConnectionError);
                    }
                }


            }
        }

        //BtnExit_OnPressed(XUiController _sender, OnPressEventArgs _e)
        [HarmonyPatch(typeof(XUiC_InGameMenuWindow))]
        [HarmonyPatch("BtnExit_OnPressed")]
        public class SphereII_XUIC_InGameMenuWindow
        {
            static void Postfix()
            {
                    if(SphereIIToggleCapsLock.GetScrollLock())
                        Application.Quit();
            }
        }
    }
}


