using HarmonyLib;

namespace LargerParties.PlayerParty.Harmony {

        [HarmonyPatch(typeof(Party))]
        [HarmonyPatch("IsFull")]
        public class PartyIsFull {
            public static bool Prefix(ref bool __result) {
                __result = true;
                return false;
            }
        }

}