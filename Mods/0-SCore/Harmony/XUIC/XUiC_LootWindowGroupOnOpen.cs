using HarmonyLib;

namespace SCore.Harmony.XUIC
{
    public class XUiC_LootWindowGroupOnOpenPatches
    {
        // XUiC_LootWindowGroup no longer overrides OnOpen — DeclaredMethod won't find it.
        // Use HarmonyTargetMethod with AccessTools.Method to walk up the inheritance chain.
        [HarmonyPatch]
        public class LootWindowGroupOnOpenPatchesOnOpen
        {
            [HarmonyTargetMethod]
            public static System.Reflection.MethodBase TargetMethod() =>
                AccessTools.Method(typeof(XUiC_LootWindowGroup), "OnOpen");

            private static bool Prefix(object __instance)
            {
                var instance = __instance as XUiC_LootWindowGroup;
                if (instance?.te == null) return true;

                if (!string.IsNullOrEmpty(instance.te.lootListName)) return true;
                Log.Out($"Missing lootListName on {instance.te}");
                var windowManager2 = instance.xui.playerUI.windowManager;
                windowManager2.Close("timer");
                windowManager2.Close("looting");
                return false;
            }
        }
    }
}
