using System;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ErrorChecks.Harmony {
    // Vanilla logs "Dropping TE with unknown/outdated type: X" and nothing else, which is not enough
    // to find the offending block. Worse, the bare type value actively misleads.
    //
    // Chunk.read's tile entity loop is:
    //     type = _br.ReadInt32();
    //     te   = TileEntity.InstantiateFromRead(_br, ..., type, this, ...);
    //
    // On an unrecognised type InstantiateFromRead logs the warning and returns null WITHOUT
    // consuming that tile entity's payload. The reader is therefore left parked in the middle of a
    // record, and the next loop iteration reads payload bytes as if they were the next TE's type.
    // One bad tile entity desyncs the rest of the chunk: the run of warnings that follows is shifted
    // garbage, and the plausible-looking type numbers in it are meaningless. It also explains the
    // "Outdated loot data" exception that usually ends the run - a garbage type eventually lands on
    // a legacy loot type and TileEntityLegacyUtils tries to parse the remaining noise as a container.
    //
    // So: only the FIRST drop in a chunk is real evidence. This patch labels it as such, and adds the
    // chunk, the POI, the stream offset, and - by peeking the unconsumed payload - the tile entity's
    // own block position and the block sitting there.
    //
    // Diagnostics only. The tile entity is still dropped and vanilla behaviour is unchanged.
    public class TileEntityDroppedTypeDiagnostics {
        private static readonly string AdvFeatureClass = "ErrorHandling";
        private static readonly string Feature = "LogDroppedTileEntityDetail";

        // Chunks are read start to finish on one thread, but several worker threads load chunks at
        // once, so the run counter is per thread rather than shared.
        [ThreadStatic] private static long _currentChunkKey;
        [ThreadStatic] private static bool _haveCurrentChunk;
        [ThreadStatic] private static int _dropsThisChunk;

        [HarmonyPatch(typeof(TileEntity))]
        [HarmonyPatch("InstantiateFromRead")]
        public class TileEntityInstantiateFromReadDiagnostics {
            public static void Postfix(TileEntity __result, PooledBinaryReader _br,
                TileEntity.StreamModeRead _eStreamMode, TileEntityType _type, Chunk _chunk) {
                if (__result != null) return;
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                // TryReadLegacyType returns null for these two by design - the record is read and
                // discarded, no warning is logged, and the stream stays in sync. Not a drop.
                if (_eStreamMode == TileEntity.StreamModeRead.Persistency &&
                    (_type == TileEntityType.GoreBlock || _type == TileEntityType.Trader)) return;

                try {
                    Report(_br, _type, _chunk);
                }
                catch (Exception ex) {
                    // A diagnostic must never be the thing that breaks a chunk load.
                    Log.Warning($"[SCore] TE drop diagnostics failed: {ex.Message}");
                }
            }

            // Vanilla lets legacy read failures ("Outdated loot data") escape to Chunk.read, which
            // reports the chunk but not which tile entity was being read. Add that, then let the
            // exception continue unchanged.
            public static void Finalizer(Exception __exception, PooledBinaryReader _br,
                TileEntityType _type, Chunk _chunk) {
                if (__exception == null) return;
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

                try {
                    Log.Warning(
                        $"[SCore] TE read threw while parsing type={Describe(_type)} in {DescribeChunk(_chunk)} " +
                        $"at streamOffset={StreamOffset(_br)}{PoiSuffix(_chunk)} - {__exception.Message}");

                    if (_dropsThisChunk > 0)
                        Log.Warning(
                            "[SCore]   this chunk already dropped " + _dropsThisChunk + " TE(s); the stream " +
                            "was desynced before this read, so the type above is almost certainly a misread. " +
                            "Fix the first drop and this exception should go with it.");
                }
                catch {
                    // Never mask the original exception with a logging failure.
                }
            }
        }

        private static void Report(PooledBinaryReader _br, TileEntityType _type, Chunk _chunk) {
            var key = _chunk?.Key ?? long.MinValue;
            if (!_haveCurrentChunk || _currentChunkKey != key) {
                _currentChunkKey = key;
                _haveCurrentChunk = true;
                _dropsThisChunk = 0;
            }

            _dropsThisChunk++;
            var first = _dropsThisChunk == 1;

            Log.Warning(
                $"[SCore] Dropped TE #{_dropsThisChunk} in {DescribeChunk(_chunk)} type={Describe(_type)} " +
                $"streamOffset={StreamOffset(_br)}{PoiSuffix(_chunk)}");

            if (!first) {
                Log.Warning(
                    "[SCore]   cascade - drop #1 left the reader mid-record, so this type value is " +
                    "misread data, not a real tile entity type. Ignore it.");
                return;
            }

            // Only worth peeking on the first drop; after that the offset is meaningless.
            if (TryPeekHeader(_br, out var version, out var localPos)) {
                var world = _chunk == null
                    ? localPos
                    : new Vector3i(_chunk.X * 16 + localPos.x, localPos.y, _chunk.Z * 16 + localPos.z);
                Log.Warning(
                    $"[SCore]   payload: teVersion={version} localPos={localPos} worldPos={world} " +
                    $"block={DescribeBlock(world)}");
            }
            else {
                Log.Warning(
                    "[SCore]   payload did not decode as a tile entity header, so this chunk's TE " +
                    "section was already out of sync before this entry. NO BLOCK POSITION IS KNOWN " +
                    "for this entry - the coordinates above are the whole chunk's extent, not a " +
                    "location, so do not go looking at the block sitting there. The fault is " +
                    "upstream: something earlier in this chunk read fewer bytes than it wrote. " +
                    "A tile entity that loads successfully without consuming its payload does this " +
                    "and logs nothing.");
            }

            Log.Warning(
                "[SCore]   ^ FIRST drop in this chunk - this is the one to chase. Vanilla does not " +
                "consume a dropped TE's payload, so every TE warning after this one in this chunk is " +
                "shifted data. Remove or re-save the block above and the rest should clear.");
        }

        // The dropped payload is still sitting in the stream. TileEntity.read writes a ushort version
        // followed by the tile entity's chunk-local position, so if those bytes decode to a sane
        // header we can name the exact block. Restores the position either way - PooledBinaryReader
        // reads exact byte counts through FillBuffer and keeps no look-ahead, so this is invisible to
        // the caller.
        private static bool TryPeekHeader(PooledBinaryReader _br, out int version, out Vector3i localPos) {
            version = -1;
            localPos = Vector3i.zero;

            var stream = _br?.BaseStream;
            if (stream == null || !stream.CanSeek || !stream.CanRead) return false;

            var origin = stream.Position;
            try {
                version = _br.ReadUInt16();
                localPos = new Vector3i(_br.ReadInt32(), _br.ReadInt32(), _br.ReadInt32());

                // Chunk-local x/z are 0-15 and y spans the world height. Anything outside that means
                // we are not looking at a header.
                return version > 0 && version <= 64 &&
                       localPos.x >= 0 && localPos.x < 16 &&
                       localPos.z >= 0 && localPos.z < 16 &&
                       localPos.y >= 0 && localPos.y < 256;
            }
            catch {
                return false;
            }
            finally {
                stream.Position = origin;
            }
        }

        private static string Describe(TileEntityType _type) {
            // The enum name alone hides the number, and "None" reads as "nothing was there" when it
            // really means the type field held 0.
            return $"{_type.ToStringCached()}({(int)_type})";
        }

        private static string DescribeChunk(Chunk _chunk) {
            if (_chunk == null) return "chunk (unknown)";
            var minX = _chunk.X * 16;
            var minZ = _chunk.Z * 16;
            return $"chunk ({_chunk.X}, {_chunk.Z}) [blocks x {minX}..{minX + 15}, z {minZ}..{minZ + 15}]";
        }

        private static string StreamOffset(PooledBinaryReader _br) {
            try {
                var stream = _br?.BaseStream;
                if (stream == null || !stream.CanSeek) return "n/a";
                return stream.Position.ToString();
            }
            catch {
                return "n/a";
            }
        }

        private static string DescribeBlock(Vector3i _worldPos) {
            try {
                var world = GameManager.Instance?.World;
                if (world == null) return "unknown (world not loaded)";

                var blockValue = world.GetBlock(_worldPos);
                if (blockValue.isair) return "air (block already gone)";

                var block = Block.list[blockValue.type];
                return block == null
                    ? $"unknown type {blockValue.type}"
                    : $"{block.GetBlockName()} (type {blockValue.type})";
            }
            catch (Exception ex) {
                return $"unavailable ({ex.Message})";
            }
        }

        // Best effort. The chunk is being loaded on a worker thread, so this reads a list that world
        // generation may still be writing to - hence the catch. Purely informational.
        private static string PoiSuffix(Chunk _chunk) {
            if (_chunk == null) return string.Empty;
            try {
                var decorator = GameManager.Instance?.GetDynamicPrefabDecorator();
                if (decorator == null) return string.Empty;

                var centre = new Vector3i(_chunk.X * 16 + 8, 0, _chunk.Z * 16 + 8);
                var prefab = decorator.GetPrefabAtPosition(centre);
                return prefab == null ? " poi=none" : $" poi={prefab.name}";
            }
            catch {
                return string.Empty;
            }
        }
    }
}
