using System.Globalization;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.LearnByDoing.Harmony
{
    [HarmonyPatch(typeof(XUiC_SkillEntry))]
    [HarmonyPatch(nameof(XUiC_SkillEntry.GetBindingValue))]
    public class XUICSkillGetBindingValue
    {
        public static void Postfix(ref bool __result, XUiC_SkillEntry __instance, ref string value,
            string bindingName)
        {
            var entityPlayer = __instance.xui.playerUI.entityPlayer;
            if (bindingName == "LearnByDoing")
            {
                __result = true;
                value = "0";
                if (__instance.currentSkill == null) return;

                var perkName = "";
                perkName = __instance.currentSkill.Name;

                var perkCurrentLevel = $"${perkName}_lbd_xp";
                var perkToNextLevel = $"${perkName}_lbd_xptonext";

                var currentLevel = entityPlayer.Buffs.GetCustomVar(perkCurrentLevel);
                var toNextLevel = entityPlayer.Buffs.GetCustomVar(perkToNextLevel);
                Log.Out($"Perk: {perkName} {perkCurrentLevel} : {currentLevel}  {perkToNextLevel} : {toNextLevel}");
                if (toNextLevel > 0)
                {
                    value = (currentLevel / toNextLevel).ToString(CultureInfo.InvariantCulture);
                }

                return;
            }

            if (bindingName == "LearnByDoingVisible")
            {
                __result = true;
                value = "false";
                if (__instance.currentSkill == null) return;
                // value = (__instance.currentSkill.Level + 1 == __instance.currentSkill.CalculatedLevel(entityPlayer) && __instance.currentSkill.Level + 1 <=
                //     __instance.currentSkill.CalculatedMaxLevel(entityPlayer)).ToString();
            }
        }
    }
}