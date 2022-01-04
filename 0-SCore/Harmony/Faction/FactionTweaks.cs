using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Harmony.Faction
{
    /**
     * SCoreFaction_Tweaks
     * 
     * This class includes a Harmony patches to enable or improve functionality in the factions. It includes allowing the factions to be saved to disk, as well
     * as fixing a casting bug when setting the relationship (A18/A19).
     */
    internal class FactionTweaks
    {
        // Fixing for a reverse condition for isGameStarted
        [HarmonyPatch(typeof(FactionManager))]
        [HarmonyPatch("Update")]
        public class FactionUpdate
        {
            // Loops around the instructions and removes the return condition.
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                // Grab all the instructions
                var codes = new List<CodeInstruction>(instructions);

                var counter = 0;
                foreach (var t in codes)
                {
                    if (t.opcode != OpCodes.Brfalse) continue;
                    if (counter == 4)
                    {
                        t.opcode = OpCodes.Brtrue;
                        break;
                    }

                    counter++;
                }

                return codes.AsEnumerable();
            }
        }

        // Fixing casting bug
        [HarmonyPatch(typeof(global::Faction))]
        [HarmonyPatch("SetRelationship")]
        public class SetRelationship
        {
            public static bool Prefix(global::Faction __instance, byte _factionId, float _value)
            {
                __instance.Relationships[_factionId] = Mathf.Clamp(_value, 0f, 1000f);
                return false;
            }
        }
   
    }
}