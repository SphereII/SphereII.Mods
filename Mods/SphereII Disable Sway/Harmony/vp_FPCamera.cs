using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace SphereII_CameraSway {

    [HarmonyPatch]
    public class SCorevp_FPCameraPatches {

        [HarmonyTargetMethod]
        static IEnumerable<MethodBase> TargetMethods() {
            yield return typeof(vp_FPCamera).GetMethod("UpdateSwaying");
            yield return typeof(vp_FPCamera).GetMethod("UpdateBob");
        }
        public static bool Prefix() {
            return SwayUtilities.CanSway(false);
        }
    }
}