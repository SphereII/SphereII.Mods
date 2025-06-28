using HarmonyLib;
using UnityEngine;

public class XUiC_DialogWindowGroupPatches
{
    [HarmonyPatch(typeof(XUiC_DialogWindowGroup))]
    [HarmonyPatch(nameof(XUiC_DialogWindowGroup.RefreshDialog))]
    public class XUiC_DialogWindowGroupRefreshDialog
    {
        public static void Postfix(XUiC_DialogWindowGroup __instance)
        {
            if (__instance.CurrentDialog.CurrentOwner is not EntityAliveSDX entityAliveSdx) return;
            var currentDialog = __instance.CurrentDialog;
            if ( currentDialog.CurrentStatement == null) return;
            if (__instance.xui.Dialog.Respondent == null) return;
            GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, __instance.xui.Dialog.Respondent.EntityName , currentDialog.CurrentStatement.Text, 9999f, false);
        }
    }
    
    [HarmonyPatch(typeof(XUiC_DialogWindowGroup))]
    [HarmonyPatch(nameof(XUiC_DialogWindowGroup.OnOpen))]
    public class XUiC_DialogWindowGroupOnOpen
    {
        public static void Postfix(XUiC_DialogWindowGroup __instance)
        {
            if (__instance.CurrentDialog.CurrentOwner is not EntityAliveSDX entityAliveSdx) return;
            var currentDialog = __instance.CurrentDialog;
            if ( currentDialog.CurrentStatement == null) return;
            GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, __instance.xui.Dialog.Respondent.EntityName , currentDialog.CurrentStatement.Text, 9999f, false);
        }
    }

    [HarmonyPatch(typeof(XUiC_DialogWindowGroup))]
    [HarmonyPatch(nameof(XUiC_DialogWindowGroup.OnClose))]
    public class XUiC_DialogWindowGroupOnClose
    {
        public static void Postfix(XUiC_DialogWindowGroup __instance)
        {
            GameManager.ShowSubtitle(LocalPlayerUI.primaryUI.xui, string.Empty,string.Empty,1f);
        }
    }

}