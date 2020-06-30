using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
/**
 * SphereII_Faction_Tweaks
 * 
 * This class includes a Harmony patches to enable or improve functionality in the factions. It includes allowing the factions to be saved to disk, as well
 *  as fixing a casting bug when setting the relationship (A18/A19).
 */
class SphereII_Faction_Tweaks
{
    // Fixing for a reverse condition for isGameStarted
    [HarmonyPatch(typeof(FactionManager))]
    [HarmonyPatch("Update")]
    public class SphereII_Faction_Update
    {
        // Loops around the instructions and removes the return condition.
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            // Grab all the instructions
            var codes = new List<CodeInstruction>(instructions);

            int Counter = 0;
            for (int i = 0; i < codes.Count ; i++)
            {
                if (codes[i].opcode == OpCodes.Brfalse)
                {
                    if (Counter == 4)
                    {
                        codes[i].opcode = OpCodes.Brtrue;
                        break;
                    }
                    Counter++;

                }
            }

            return codes.AsEnumerable();
        }
    }

    // Fixing casting bug
    [HarmonyPatch(typeof(Faction))]
    [HarmonyPatch("SetRelationship")]
    public class SphereII_Faction_SetRelationship
    {
        public static bool Prefix(Faction __instance, byte _factionId, float _value)
        {
            __instance.Relationships[(int)_factionId] = Mathf.Clamp(_value, 0f, 1000f);
            return false;

        }
    }

}

