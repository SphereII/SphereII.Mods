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
    TileEntityAoE = 201,
    TileEntityAliveV2=202
}


namespace Harmony.TileEntities
{
    public class TileEntityAddition
    {

        // Chunk.save writes each tile entity as its type followed by its payload:
        //     _bw.Write((int)te.GetTileEntityType());
        //     te.write(_bw, ...);
        // Chunk.read consumes the type and hands it to InstantiateFromRead, whose contract is to
        // construct the tile entity AND read that payload back off the stream before returning.
        //
        // This prefix used to construct the tile entity and return immediately, never calling read.
        // The payload was left in the stream, so the reader ended up parked mid-record and every
        // following byte was interpreted at the wrong offset: the next tile entity's "type" was
        // actually payload bytes, and the sleeper/wall volume data after the tile entity section
        // desynced with it. A single AoE block, whose inherited base payload is 22 bytes in
        // Persistency mode, was enough to corrupt the remainder of the chunk. Symptoms were a run of
        // "Dropping TE with unknown/outdated type", "chunk sleeper volumeId invalid", and an
        // eventual exception that made vanilla discard the chunk - all of it downstream, and none of
        // it pointing back here.
        //
        // The saved data was always correct; only the read side was short. Reading the payload makes
        // existing saves load properly again.
        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("InstantiateFromRead")]
        public class TileEntityInstantiate
        {
            public static bool Prefix(ref TileEntity __result, PooledBinaryReader _br,
                TileEntity.StreamModeRead _eStreamMode, TileEntityType _type, Chunk _chunk)
            {
                TileEntity tileEntity;

                if (_type == (TileEntityType)SCoreTileEntity.TileEntityPoweredPortal)
                {
                    tileEntity = new TileEntityPoweredPortal(_chunk);
                }
                else if (_type == (TileEntityType)SCoreTileEntity.TileEntityAoE)
                {
                    tileEntity = new TileEntityAoE(_chunk);
                }
                else
                {
                    return true;
                }

                // Neither of these is a TileEntityComposite, so this matches the plain branch of the
                // method being replaced. A composite would need the _blockIdMapping overload instead.
                tileEntity.read(_br, _eStreamMode);
                __result = tileEntity;
                return false;
            }
        }
    }
}