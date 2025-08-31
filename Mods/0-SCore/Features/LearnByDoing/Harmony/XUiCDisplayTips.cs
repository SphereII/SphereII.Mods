using HarmonyLib;
using UnityEngine;

public abstract class XUiCDisplayTips
{
    [HarmonyPatch(typeof(XUiC_SkillAttributeInfoWindow))]
    [HarmonyPatch(nameof(XUiC_SkillAttributeInfoWindow.GetBindingValueInternal))]
    public class XUiCSkillSkillInfoWindowGetBindingValue
    {
        public static void Postfix(ref bool __result, XUiC_SkillAttributeInfoWindow __instance, ref string _value,
            string _bindingName)
        {
            
            if (_bindingName == "detailsdescription" && string.IsNullOrEmpty(_value))
            {
                if (__instance.CurrentSkill == null) return;
                var perkName = __instance.CurrentSkill.Name;
                var searchKey = $"{perkName}LearnByDoingDesc";
                _value = Localization.Get(searchKey, true);
                if (searchKey == _value)
                    _value = string.Empty;
                __result = true;
            }
        }
    }
    [HarmonyPatch(typeof(XUiC_SkillPerkInfoWindow))]
    [HarmonyPatch(nameof(XUiC_SkillPerkInfoWindow.GetBindingValueInternal))]
    public class XUiCSkillPerkInfoWindowGetBindingValue
    {
        public static void Postfix(ref bool __result, XUiC_SkillPerkInfoWindow __instance, ref string _value,
            string _bindingName)
        {
            
            if (_bindingName == "detailsdescription" && string.IsNullOrEmpty(_value))
            {
                if (__instance.CurrentSkill == null) return;
                var perkName = __instance.CurrentSkill.Name;
                var searchKey = $"{perkName}LearnByDoingDesc";
                _value = Localization.Get(searchKey, true);
                if (searchKey == _value)
                    _value = string.Empty;

                __result = true;
            }
        }
    }
}
