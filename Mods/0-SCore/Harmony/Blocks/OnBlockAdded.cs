using HarmonyLib;

namespace SCore.Harmony.Blocks {
    public class OnBlockAddedPatches {
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnBlockAdded")]
        public class OnBlockAddedPatch {
            private static string _fieldName = "RegisterToFireManager";

            public static void Postfix(Block __instance, Vector3i _blockPos)
            {
                if (FireManager.Instance == null) return;
                if (!FireManager.Instance.Enabled) return;
                if (!__instance.Properties.Values.ContainsKey(_fieldName)) return;

                var result = false;
                __instance.Properties.ParseBool(_fieldName, ref result);
                if (!result) return;
                
                FireManager.Instance.Add(_blockPos);    
                
            }
        }
    }
}