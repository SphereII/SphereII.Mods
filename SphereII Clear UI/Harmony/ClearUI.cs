using DMT;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

public class ClearUI
{
    public class ClearUI_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

 
    // Sneak Damage pop up
    [HarmonyPatch(typeof(EntityPlayerLocal))]
    [HarmonyPatch("NotifySneakDamage")]
    public class SphereII_ClearUI_NotifySneakDamage
    {
        static bool Prefix()
        {
            return false;
        }
    }
    // Remove the damage notifier
    [HarmonyPatch(typeof(EntityPlayerLocal))]
    [HarmonyPatch("NotifyDamageMultiplier")]
    public class SphereII_ClearUI_NotifyDamageMultiplier
    {
        static bool Prefix()
        {
            return false;
        }
    }
   

    // Disables the compass marking, etc
    [HarmonyPatch(typeof(XUiC_CompassWindow))]
    [HarmonyPatch("Update")]
    public class SphereII_ClearUI_XUiC_CompassWindow
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionDynamic))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionDynamic
    {
        static bool Prefix()
        {
            return false;
        }
    }

  
    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionAttack))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionAttack
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionUseOther))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionUseOther
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes the overlays, like the damage, downground and upgrade indicators
    [HarmonyPatch(typeof(ItemActionRanged))]
    [HarmonyPatch("canShowOverlay")]
    public class SphereII_ClearUI_ItemActionRanged
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes the Tool Tips for Journal
    [HarmonyPatch(typeof(XUiC_TipWindow))]
    [HarmonyPatch("ShowTip")]
    public class SphereII_ClearUI_XUiC_TipWindow
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes the looting pop up
    [HarmonyPatch(typeof(XUiC_InteractionPrompt))]
    [HarmonyPatch("SetText")]
    public class SphereII_ClearUI_XUiC_InteractionPrompt
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes Tool tips and skill perks
    [HarmonyPatch(typeof(NGuiWdwInGameHUD))]
    [HarmonyPatch("ShowInfoText")]
    public class SphereII_ClearUI_NGuiWdwInGameHUD
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Remove all the tool tips
    [HarmonyPatch(typeof(NGuiWdwInGameHUD))]
    [HarmonyPatch("SetTooltipText")]

    // There's multiple Tool tips, so let's specify the parameter types here:
    // public void SetTooltipText(string _text, string[] _args, string _alertSound, ToolTipEvent eventHandler)
    [HarmonyPatch(new Type[] { typeof(string), typeof(string[]), typeof(string), typeof(ToolTipEvent) })]
    public class SphereII_ClearUI_SetTooltipText
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Remove the SetLabel Text calls
    [HarmonyPatch(typeof(NGUIWindowManager))]
    [HarmonyPatch("SetLabelText")]
    [HarmonyPatch(new Type[] { typeof(EnumNGUIWindow), typeof(string) })]
    public class SphereII_ClearUI_NGUIWindowManager
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Remove the SetLabel Text calls
    [HarmonyPatch(typeof(NGUIWindowManager))]
    [HarmonyPatch("SetLabelText")]
    [HarmonyPatch(new Type[] { typeof(EnumNGUIWindow), typeof(string), typeof(bool) })]
    public class SphereII_ClearUI_NGUIWindowManager_SetLabelText
    {
        static bool Prefix()
        {
            return false;
        }
    }

    // Removes the timer
    [HarmonyPatch(typeof(XUiC_Timer))]
    [HarmonyPatch("OnOpen")]
    public class SphereII_ClearUI_XUiC_Timer_OnOpen
    {
        static bool Prefix()
        {
            return false;
        }

    }


    // Removes the timer
    [HarmonyPatch(typeof(XUiC_Timer))]
    [HarmonyPatch("UpdateTimer")]
    public class SphereII_ClearUI_XUiC_Timer_UpdateTimer
    {
        static bool Prefix()
        {
            return false;
        }
      
    }

   

    // removes cross hair
    [HarmonyPatch(typeof(ItemClass))]
    [HarmonyPatch("GetCrosshairType")]
    public class SphereII_ClearUI_ItemClass_Crosshair
    {
        static ItemClass.EnumCrosshairType Postfix(ItemClass.EnumCrosshairType __result)
        {
            return ItemClass.EnumCrosshairType.None;
        }
    }

}

