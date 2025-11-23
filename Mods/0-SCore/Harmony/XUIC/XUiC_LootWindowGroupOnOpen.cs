using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.XUIC
{
    public class XUiC_LootWindowGroupOnOpenPatches
    {
        [HarmonyPatch(typeof(XUiC_LootWindowGroup))]
        [HarmonyPatch(nameof(XUiC_LootWindowGroup.OnOpen))]
        public class LootWindowGroupOnOpenPatchesOnOpen
        {
            private static bool Prefix(XUiC_LootWindowGroup __instance)
            {
                if (__instance?.te == null) return true;
           
                if (!string.IsNullOrEmpty(__instance.te.lootListName)) return true;
                Log.Out($"Missing lootListName on {__instance.te}");
                var windowManager2 = __instance.xui.playerUI.windowManager;
                __instance.ignoreCloseSound = true;
                windowManager2.Close("timer");
                __instance.isOpening = false;
                __instance.isClosingFromDamage = true;
                windowManager2.Close("looting");
                return false;
            }
        }
    }
}
