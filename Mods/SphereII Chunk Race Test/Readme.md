# SphereII Chunk Race Test

**DESTRUCTIVE debug tool — never install on a real world.** This modlet exists to reproduce,
on demand, the vanilla thread-safety race in the chunk load path so the failure (and SCore's
fix for it) can be demonstrated deterministically. It is not part of any release pipeline and
should never be shipped to players.

## Background

`RegionFileManager` funnels every chunk load through one shared `RegionFileChunkReader` — a
single read buffer, a cached `DeflateInputStream`, and one load `MemoryStream` — with no
locking. Normally only the chunk generation thread loads chunks, but chunk resets
(`regionreset`, `worldchunkreset`, `ActionResetRegions` game events) load chunks from the
main thread through the same reader. When two loads overlap, the shared buffers tear
mid-decompression:

```
EXCEPTION: In load chunk (chunkX=... chunkZ=...)
EXC invalid distance too far back      (or: Wrong chunk header!)
```

...and the failed load then **deletes the chunk from the region file**. The chunk regenerates
fresh; player builds and any NPCs standing in it are gone.

In normal play the collision window is milliseconds per chunk, so the bug appears as rare,
unexplained corruption. This command removes the timing luck by hammering
`ChunkSnapshotUtil.LoadChunk` from several threads at once (bypassing the chunk cache, so
every call goes through the shared reader).

## Usage

1. **Copy a well-explored save** (dense region files, e.g. explored city areas). Work only on
   the copy.
2. To demonstrate the vanilla bug: disable the guard in SCore's `blocks.xml`
   (`RegionFileLoadChunkLock` = false) or remove SCore entirely.
3. Load the world, open the console:

   ```
   chunkracetest confirm [seconds] [threads]
   ```

   Defaults: 10 seconds, 2 threads. Expect a stream of `EXCEPTION: In load chunk` within
   seconds. `error_backup_*.bak` files appearing in the save's region folder are the hit
   counter.
4. To verify the fix: re-enable `RegionFileLoadChunkLock` (and set
   `LogRegionFileLoadChunkLock` = true), restart, run the same command. The run should be
   clean, with `RegionFileLoadChunkLock: LoadChunk waiting...` lines showing each prevented
   collision.

The before/after log pair is suitable evidence for a TFP bug report: same save, same
command, race present without the lock and absent with it.

## Building

Old-style csproj referencing the game's `Managed` folder two levels up (same layout as the
other modlets). Build with MSBuild; the DLL lands in the modlet root where the game's mod
loader picks it up:

```
MSBuild.exe "SphereII Chunk Race Test.csproj" -p:Configuration=Release
```
