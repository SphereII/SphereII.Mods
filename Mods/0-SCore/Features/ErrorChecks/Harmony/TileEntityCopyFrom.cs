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

            public static bool Prefix (TileEntity _other )
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
                Debug.Log($"ErrorHandling::TileEntityCopyFrom::Prefix:: {_other.blockValue.Block.GetBlockName()}. No Defined CopyFrom()");
                return false;
                
                
            }
        }
    }
}