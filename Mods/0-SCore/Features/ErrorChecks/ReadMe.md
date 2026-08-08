The Error Checks feature is a collection of Harmony patches that guard against known crashes and data-loss bugs
during chunk, prefab and tile entity loading, plus a few diagnostics for problems that are otherwise very hard to
locate. Every patch is individually switchable under `<property class="ErrorHandling">` in `blocks.xml`, and each
flag is described in `Config/ReadMe.md`.

Most of these are fixes and are on by default. The diagnostics are off by default and exist to be turned on for one
session while chasing a specific report.

## 1\. Diagnosing "Dropping TE with unknown/outdated type"

### The problem with the vanilla warning

A save carrying tile entities from a mod that is no longer installed - or from a format the game no longer reads -
produces a run of these on load:

```
WRN Dropping TE with unknown/outdated type: None
WRN Dropping TE with unknown/outdated type: None
WRN Dropping TE with unknown/outdated type: 99
WRN Dropping TE with unknown/outdated type: 1
WRN Dropping TE with unknown/outdated type: 128
ERR EXCEPTION: In load chunk (chunkX=-25 chunkZ=49)
EXC Outdated loot data
```

It names no position, so there is nothing to act on. Worse, **the type numbers after the first one are not real.**

`Chunk.read` loops over the chunk's tile entities like this:

```csharp
TileEntityType type = (TileEntityType)_br.ReadInt32();
TileEntity tileEntity = TileEntity.InstantiateFromRead(_br, eStreamMode, type, this, null, GetBlock);
```

When `InstantiateFromRead` does not recognise the type it logs the warning and returns `null` **without consuming
that tile entity's payload**. The reader is left parked in the middle of a record, so the next iteration reads
payload bytes as though they were the next tile entity's type. A single bad tile entity desyncs the rest of the
chunk.

That has two consequences worth internalising:

* Only the **first** warning in a chunk is evidence. Everything after it is shifted data, and its type number is a
  coincidence. Do not go looking for "the mod that uses type 128".
* `Outdated loot data` is part of the same cascade, not a separate fault. Once desynced, a garbage type eventually
  collides with a legacy loot type, `TileEntityLegacyUtils` accepts it, and the parser throws when the version field
  it read out of the noise falls outside the supported range. Fix the first drop and the exception goes with it.

### Turning the diagnostic on

```xml
<property name="LogDroppedTileEntityDetail" value="true"/>
```

Then load the save and reproduce. The flag only affects logging - the tile entity is still dropped and load
behaviour is unchanged - but it is verbose on a badly affected save, so turn it off again afterwards.

### Reading the output

```
WRN [SCore] Dropped TE #1 in chunk (-25, 49) [blocks x -400..-385, z 784..799] type=None(0) streamOffset=48213 poi=house_old_ranch_02
WRN [SCore]   payload: teVersion=19 localPos=(3, 41, 12) worldPos=(-397, 41, 796) block=cntStoreShelf01
WRN [SCore]   ^ FIRST drop in this chunk - this is the one to chase. Vanilla does not consume a dropped TE's
              payload, so every TE warning after this one in this chunk is shifted data. Remove or re-save the
              block above and the rest should clear.
WRN [SCore] Dropped TE #2 in chunk (-25, 49) [blocks x -400..-385, z 784..799] type=Loot(1) streamOffset=48227 poi=house_old_ranch_02
WRN [SCore]   cascade - drop #1 left the reader mid-record, so this type value is misread data, not a real tile
              entity type. Ignore it.
```

| Field | Meaning |
| ----------- | ----------- |
| `Dropped TE #n` | Position in this chunk's run of drops. Only `#1` is evidence. |
| `chunk (x, z)` | Chunk coordinates, with the world block range they cover. |
| `type=Name(n)` | Enum name **and** raw value. The name alone hides the number, and `None` reads as "nothing was there" when it actually means the type field held `0`. |
| `streamOffset` | Byte offset in the chunk stream, for correlating drops against each other. |
| `poi` | POI occupying that chunk, when one can be resolved. Best effort - see limits below. |
| `payload:` | Decoded from the unread payload. `worldPos` and `block` name the offending tile entity. |
| `cascade` | This entry is misread data caused by an earlier drop. |

### Where the block position comes from

Because the payload is never consumed, it is still sitting in the stream when the warning fires. `TileEntity.read`
begins with a `ushort` version followed by the tile entity's chunk-local `Vector3i`, so the first drop peeks those
bytes and sanity-checks them (version ≤ 64, local x/z within 0-15, y within 0-255). If they decode, the warning
names the exact world position and the block standing there. The peek saves and restores the stream position, and
`PooledBinaryReader` reads exact byte counts with no look-ahead, so it is invisible to the caller.

If the peek reports that the payload did not decode, that is itself informative, and it points somewhere quite
different - see below.

**When the peek fails there is no block position at all.** The only coordinates on the line are the chunk's block
extent, and its low corner is not a location - nothing is known to be wrong there. Reading it as one sends you to
an arbitrary block that happens to sit at the chunk boundary, which has already cost one person an afternoon
inspecting an innocent door. The line says so explicitly for that reason.

### Acting on it

**If the payload decoded**, `worldPos` and `block` identify the block whose tile entity can no longer be read.
Usually one of:

* A block from a mod that is no longer installed. Reinstalling it lets the save load cleanly so the block can be
  removed properly, which is the tidiest route.
* A block in a POI that was saved under an older format. Re-saving the prefab rewrites its tile entities.
* A block that is simply gone, reported as `air (block already gone)`. The tile entity outlived its block; removing
  the stale record is the fix.

Deleting the region file containing that chunk also clears it, at the cost of everything else in the region.

**If the payload did not decode**, the tile entity named is not the problem - the reader was already at the wrong
offset when it reached it. The usual cause is a *successfully* loaded tile entity earlier in the same chunk whose
payload was never consumed, which produces no warning of its own because a tile entity was returned. Look for a
custom `TileEntity.InstantiateFromRead` patch that constructs its type and returns without calling
`read(_br, _eStreamMode)`.

That is not hypothetical: SCore's own patch for `TileEntityAoE` and `TileEntityPoweredPortal` did exactly this
until 3.1.15. `Chunk.save` writes every tile entity as a type followed by a payload, and `InstantiateFromRead` is
contracted to read that payload back. Skipping it left the stream short by the size of the record - 22 bytes for a
plain AoE - so one `DecoAoE` block in a POI desynced the rest of the chunk, taking the tile entity section, the
sleeper volumes and the wall volumes with it. Every visible symptom appeared downstream of the actual fault.

The tell is worth remembering: **a first drop whose payload does not decode means look upstream, not at the block
named.** Anything that constructs a tile entity in that patch owes the stream a `read`.

### Limits

* Diagnostics only. This does not repair the save or resynchronise the stream - it tells you where to look.
* The POI name is best effort. Chunks load on a worker thread while world generation may still be writing the
  prefab list, so the lookup is guarded and omitted rather than risked if it fails.
* The peek is a heuristic. It validates the header before trusting it, but a payload whose leading bytes happen to
  look like a valid header can still produce a plausible-looking wrong position. Treat `#1` as a strong lead, not
  proof.

## 2\. The remaining Error Checks patches

The rest of this feature is fixes rather than diagnostics, each behind its own flag and documented in
`Config/ReadMe.md` under **Error Handling**. In brief:

| Flag | Guards against |
| ----------- | ----------- |
| `FixOrphanedPoweredTileEntities` | A powered TE whose block is gone throwing during `Chunk.read`, causing vanilla to delete the whole chunk. |
| `FixLegacyTileEntityNullChunk` | Legacy sign migration dereferencing a null chunk, skipping a prefab's entire active block data. |
| `RegionFileLoadChunkLock` | Concurrent region file reads tearing chunk data. |
| `WarnRenderMapLiveServer` | `rendermap` opening a second reader over a live save. |
| `FixCompanionEntryListDrift` | The companion HUD list creeping down the screen. |

Consult `Config/ReadMe.md` for the full list and the reasoning behind each default.
