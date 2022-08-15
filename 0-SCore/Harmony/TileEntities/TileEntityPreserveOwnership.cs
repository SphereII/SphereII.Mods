using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace SCore.Harmony.TileEntities
{
    public class TileEntityPreserveOwnership
    {
        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("UpgradeDowngradeFrom")]
        public class TileEntityUpgradeDowngradeFrom
        {
            public static bool Prefix(ref TileEntity __instance, TileEntity _other)
            {
                var newTileEntity = __instance as TileEntityPoweredTrigger;
                var oldTileEntity = _other as TileEntityPoweredTrigger;
                if (newTileEntity == null || oldTileEntity == null) return true;

                if (oldTileEntity.GetOwner() == null) return true;

                newTileEntity.SetOwner(oldTileEntity.GetOwner());
                return true;
            }
        }
    }
}
