using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
namespace SphereII_WeaponSway {

    [HarmonyPatch]
    public class SCorevp_FPWeaponPatches {

        [HarmonyTargetMethod]
        static IEnumerable<MethodBase> TargetMethods() {
            yield return typeof(vp_FPWeapon).GetMethod("UpdateSwaying");
            yield return typeof(vp_FPWeapon).GetMethod("UpdateBob");
        }
        public static bool Prefix() {
            return SwayUtilities.CanSway(false);
        }
    }
}