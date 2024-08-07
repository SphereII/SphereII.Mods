using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace LargerParties.PlayerParty.Harmony {

    [HarmonyPatch(typeof(Party))]
    [HarmonyPatch("AddPlayer")]
    public class PartyAddPlayer {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            foreach (var t in codes)
            {
                // Look for MemberCount = 8
                if (t.opcode != OpCodes.Ldc_I4_8)
                    continue;

                // Change to the MaxParty
                t.opcode = OpCodes.Ldc_I4_S;
                t.operand = PlayerPartySettings.MaxParty;
                break;
            }

            Debug.Log(
                $"SphereII Larger Parties: Max Party: {PlayerPartySettings.MaxParty} Minimum Exp To All Players: {PlayerPartySettings.MinExp}");
            return codes.AsEnumerable();
        }
    }
}