using HarmonyLib;
using System.Reflection;

public class ClearUI
{
    public class ClearUIInit : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());
            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }


    // Sneak Damage pop up
    [HarmonyPatch(typeof(EntityPlayerLocal))]
    [HarmonyPatch("NotifySneakDamage")]
    public class SphereII_ClearUI_NotifySneakDamage
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Remove the damage notifier
    [HarmonyPatch(typeof(EntityPlayerLocal))]
    [HarmonyPatch("NotifyDamageMultiplier")]
    public class SphereII_ClearUI_NotifyDamageMultiplier
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Disables the compass marking, etc
    [HarmonyPatch(typeof(XUiC_CompassWindow))]
    [HarmonyPatch("Update")]
    public class SphereII_ClearUI_XUiC_CompassWindow
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionDynamic))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionDynamic
    {
        private static bool Prefix()
        {
            return false;
        }
    }


    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionAttack))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionAttack
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionUseOther))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionUseOther
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionRanged))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionRanged
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Removes the Tool Tips for Journal
    [HarmonyPatch(typeof(XUiC_TipWindow))]
    [HarmonyPatch("ShowTip")]
    public class SphereII_ClearUI_XUiC_TipWindow
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Removes the looting pop up
    [HarmonyPatch(typeof(XUiC_InteractionPrompt))]
    [HarmonyPatch("SetText")]
    public class SphereII_ClearUI_XUiC_InteractionPrompt
    {
        private static bool Prefix()
        {
            return false;
        }
    }


    // // Remove all the tool tips
    // [HarmonyPatch(typeof(NGuiWdwInGameHUD))]
    // [HarmonyPatch("SetTooltipText")]
    //
    // // There's multiple Tool tips, so let's specify the parameter types here:
    // // public void SetTooltipText(string _text, string[] _args, string _alertSound, ToolTipEvent eventHandler)
    // [HarmonyPatch(new Type[] { typeof(string), typeof(string[]), typeof(string), typeof(ToolTipEvent) })]
    // public class SphereII_ClearUI_SetTooltipText
    // {
    //     static bool Prefix()
    //     {
    //         return false;
    //     }
    // }

    // Remove the SetLabel Text calls
    [HarmonyPatch(typeof(NGUIWindowManager))]
    [HarmonyPatch("SetLabelText")]
    [HarmonyPatch(new[] { typeof(EnumNGUIWindow), typeof(string) })]
    public class SphereII_ClearUI_NGUIWindowManager
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Remove the SetLabel Text calls
    [HarmonyPatch(typeof(NGUIWindowManager))]
    [HarmonyPatch("SetLabelText")]
    [HarmonyPatch(new[] { typeof(EnumNGUIWindow), typeof(string), typeof(bool) })]
    public class SphereII_ClearUI_NGUIWindowManager_SetLabelText
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    // Removes the timer
    [HarmonyPatch(typeof(XUiC_Timer))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_ClearUI_XUiC_Timer_OnOpen
    {
        private static bool Prefix()
        {
            return false;
        }
    }


    // Removes the timer
    [HarmonyPatch(typeof(XUiC_Timer))]
    [HarmonyPatch("UpdateTimer")]
    public class SphereII_ClearUI_XUiC_Timer_UpdateTimer
    {
        private static bool Prefix()
        {
            return false;
        }
    }


    // removes cross hair
    [HarmonyPatch(typeof(ItemClass))]
    [HarmonyPatch("GetCrosshairType")]
    public class SphereII_ClearUI_ItemClass_Crosshair
    {
        private static ItemClass.EnumCrosshairType Postfix(ItemClass.EnumCrosshairType __result)
        {
            return ItemClass.EnumCrosshairType.None;
        }
    }
}