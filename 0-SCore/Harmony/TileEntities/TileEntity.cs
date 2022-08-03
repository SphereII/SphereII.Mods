using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;


public enum SCoreTileEntity
{
    TileEntityPoweredPortal = 200,
    TileEntityAoE = 201
}


namespace Harmony.TileEntities
{
    public class TileEntityAddition
    {

        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("Instantiate")]
        public class TileEntityIsActive
        {
            public static bool Prefix(ref TileEntity __result, TileEntityType type, Chunk _chunk)
            {
                if ( type == (TileEntityType)SCoreTileEntity.TileEntityPoweredPortal)
                { 
                        __result = new TileEntityPoweredPortal(_chunk);
                        return false;
                }

                if (type == (TileEntityType)SCoreTileEntity.TileEntityAoE)
                {
                    __result = new TileEntityAoE(_chunk);
                    return false;
                }
                return true;
            }
        }
    }
}