using System.IO;
using HarmonyLib;

namespace SCore.Features.ErrorChecks.Harmony {
    public class TraderDataReadInventoryData {
        [HarmonyPatch(typeof(TraderData))]
        [HarmonyPatch("ReadInventoryData")]

        public class TraderDataReadInventoryDataPatch
        {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "TraderDataReadInventory";

            public static bool Prefix (ref TraderData __instance, BinaryReader _br )
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
                
                __instance.PrimaryInventory.Clear();
                var itemStacks = GameUtils.ReadItemStack(_br);
                if ( itemStacks != null )
                    __instance.PrimaryInventory.AddRange(itemStacks);
                __instance.TierItemGroups.Clear();
                var num = (int)_br.ReadByte();
                for (int i = 0; i < num; i++)
                {
                    var itemStack = GameUtils.ReadItemStack(_br);
                    if ( itemStack != null)
                        __instance.TierItemGroups.Add(itemStack);
                }
                __instance.AvailableMoney = _br.ReadInt32();
                __instance.priceMarkupList.Clear();
                var num2 = _br.ReadInt32();
                for (var j = 0; j < num2; j++)
                {
                    __instance.priceMarkupList.Add(_br.ReadSByte());
                }

                return false;
            }
        }
    }
}