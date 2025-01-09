using System.IO;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ErrorChecks.Harmony {
    public class TileEntityCopyFrom {
        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("CopyFrom")]

        public class TileEntityCopyFromPatch
        {
            private static readonly string AdvFeatureClass = "ErrorHandling";
            private static readonly string Feature = "TileEntityCopyFrom";

            public static bool Prefix (TileEntity __instance, TileEntity _other )
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
                // if (_other.blockValue.Block != null)
                // {
                //     Debug.Log(
                //         $"ErrorHandling::TileEntityCopyFrom::Prefix:: {_other.blockValue.Block.GetBlockName()}. No Defined CopyFrom()");
                // }
                // else
                // {
                //     Debug.Log($"ErrorHandling::TileEntityCopyFrom::Prefix:: No Defined Block on Tile Entity");
                //     Debug.Log($"ErrorHandling::TileEntityCopyFrom::Prefix:: Current Block: {__instance.blockValue.Block?.GetBlockName()}");
                //     Debug.Log($"ErrorHandling::TileEntityCopyFrom::Prefix:: Other Block Value: {_other.blockValue}");
                // }

                return false;
                
                
            }
        }
    }
}