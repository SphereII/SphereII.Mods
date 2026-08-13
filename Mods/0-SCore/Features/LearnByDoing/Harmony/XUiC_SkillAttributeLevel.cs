using System.Globalization;
using HarmonyLib;
using SCore.Features.LearnByDoing.Scripts;
using UnityEngine;

namespace SCore.Features.LearnByDoing.Harmony
{
    [HarmonyPatch(typeof(XUiC_SkillAttributeLevel))]
    [HarmonyPatch(nameof(XUiC_SkillAttributeLevel.GetBindingValueInternal))]
    public class XUICSkillAttributeLevelGetBindingValue
    {
        public static readonly CachedStringFormatterXuiRgbaColor staticoncolorFormatter = new CachedStringFormatterXuiRgbaColor();

        public static void Postfix(ref bool __result, XUiC_SkillAttributeLevel __instance, ref string _value,
            string _bindingName)
        {
            var entityPlayer = __instance.xui.playerUI.entityPlayer;
            if (_bindingName == "LearnByDoing")
            {
                __result = true;
                _value = "0";
                if (__instance.CurrentSkill == null) return;

                var perkName = "";
                perkName = __instance.CurrentSkill.Name;

                var perkCurrentLevel = $"${perkName}_lbd_xp";
                var perkToNextLevel = $"${perkName}_lbd_xptonext";

                var currentLevel = entityPlayer.Buffs.GetCustomVar(perkCurrentLevel);
                var toNextLevel = entityPlayer.Buffs.GetCustomVar(perkToNextLevel);
            //    Log.Out($"Perk: {perkName} {perkCurrentLevel} : {currentLevel}  {perkToNextLevel} : {toNextLevel}");
                if (toNextLevel > 0)
                {
                    _value = (currentLevel / toNextLevel).ToString(CultureInfo.InvariantCulture);
                }

                return;
            }

            if (_bindingName == "LearnByDoingVisible")
            {
                __result = true;
                _value = "false";
                if (__instance.CurrentSkill == null) return;
                var perkName = __instance.CurrentSkill.Name;
                var perkToNextLevel = $"${perkName}_lbd_xptonext";
                var toNextLevel = entityPlayer.Buffs.GetCustomVar(perkToNextLevel);
                if (toNextLevel == 0)
                    return;
                _value = (__instance.CurrentSkill.Level + 1 == __instance.level && __instance.CurrentSkill.Level + 1 <=
                    __instance.CurrentSkill.CalculatedMaxLevel(entityPlayer)).ToString();
            }

            if (_bindingName == "color")
            {
                __result = true;

                // Always a formatted value, including the no-skill early out. Color.ToString()
                // yields "RGBA(0.000, ...)", which XUi cannot parse as a colour.
                _value = staticoncolorFormatter.Format(LbdDecayColor.Healthy);
                if (__instance.CurrentSkill == null) return;

                var decayCounter = entityPlayer.Buffs.GetCustomVar($"${__instance.CurrentSkill.Name}_decay_counter");
                _value = staticoncolorFormatter.Format(LbdDecayColor.ForDecayCounter(decayCounter));
            }
        }
    }
}