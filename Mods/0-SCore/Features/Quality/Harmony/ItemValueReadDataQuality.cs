using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace SCore.Features.Quality.Harmony
{
    // When an item is read from a save or a network packet, vanilla raises its quality to the item's
    // QualityMin property, if it has one:
    //
    //     quality = (byte)Utils.FastMax(quality, itemClass.QualityMin);
    //
    // Quality is a ushort, but the result is cast to a byte. With vanilla's 1-6 range that loses
    // nothing. With CustomQualityLevels, any quality above 255 wraps (300 becomes 44, 600 becomes 88)
    // every time the item is loaded. Only items that set QualityMin are affected: in vanilla that is
    // the cooking pot, grill, anvil and bellows.
    //
    // This swaps the byte cast for a ushort cast. Qualities of 255 and below are unchanged, so it is
    // applied whether or not CustomQualityLevels is on.
    [HarmonyPatch(typeof(ItemValue))]
    [HarmonyPatch("ReadData")]
    public class ItemValueReadDataQuality
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            var fastMax = AccessTools.Method(typeof(Utils), nameof(Utils.FastMax), new[] { typeof(int), typeof(int) });
            var quality = AccessTools.Field(typeof(ItemValue), "quality");

            var patched = false;
            for (var i = 0; i < codes.Count - 2; i++)
            {
                if (!codes[i].Calls(fastMax)) continue;
                if (codes[i + 1].opcode != OpCodes.Conv_U1) continue;
                if (!codes[i + 2].StoresField(quality)) continue;

                codes[i + 1].opcode = OpCodes.Conv_U2;
                patched = true;
                break;
            }

            if (!patched)
                Log.Warning("[SCore] ItemValue.ReadData: QualityMin byte cast not found. Items with a QualityMin property may lose quality above 255 on load.");

            return codes;
        }
    }
}
