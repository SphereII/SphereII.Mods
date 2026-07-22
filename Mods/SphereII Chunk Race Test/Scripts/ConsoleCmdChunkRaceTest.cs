using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

// Deterministic reproducer for the vanilla shared-reader race in the chunk load path.
//
// RegionFileManager funnels every chunk load through one RegionFileChunkReader — a single
// read buffer, cached DeflateInputStream and load MemoryStream — with no locking. In normal
// play only the generation thread loads chunks, but chunk resets (regionreset /
// worldchunkreset, ActionResetRegions game events) load from the main thread too, and
// overlapping loads tear the shared buffers: "EXC invalid distance too far back",
// "Wrong chunk header!", followed by the chunk being DELETED from the region file.
//
// This command removes the timing luck: it hammers snapshotUtil.LoadChunk from multiple
// threads at once. Calling LoadChunk directly matters — it bypasses GetChunkSync's chunk
// cache and its currently-being-saved spin guard, so every call goes through the shared
// reader. On an unpatched game expect a stream of load exceptions within seconds; with
// SCore's RegionFileLoadChunkLock enabled the same run is clean (and with
// LogRegionFileLoadChunkLock also on, contention lines show each prevented hit).
public class ConsoleCmdChunkRaceTest : ConsoleCmdAbstract
{
    private static int running;

    public override string[] getCommands()
    {
        return new[] { "chunkracetest" };
    }

    public override string getDescription()
    {
        return "DESTRUCTIVE TEST: hammers the chunk load path from multiple threads to reproduce the shared-reader race. Throwaway saves only.";
    }

    public override string getHelp()
    {
        return "chunkracetest confirm [seconds] [threads]\n" +
               "  seconds: how long to hammer (default 10)\n" +
               "  threads: concurrent loader threads (default 2)\n" +
               "Loads random already-saved chunks from disk on several threads at once.\n" +
               "On an unpatched game this reproduces 'EXCEPTION: In load chunk' /\n" +
               "'invalid distance too far back' within seconds. EVERY HIT PERMANENTLY\n" +
               "DELETES THE AFFECTED CHUNK (it regenerates fresh, player builds and NPCs\n" +
               "in it are lost). Run only on a disposable copy of a save.";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var console = SingletonMonoBehaviour<SdtdConsole>.Instance;
        var world = GameManager.Instance != null ? GameManager.Instance.World : null;
        if (world == null)
        {
            console.Output("chunkracetest: no world loaded.");
            return;
        }

        if (_params.Count < 1 || !string.Equals(_params[0], "confirm", StringComparison.OrdinalIgnoreCase))
        {
            console.Output("chunkracetest is DESTRUCTIVE: every race hit deletes a chunk from this save.");
            console.Output("Run it only on a throwaway copy of a world, then repeat with 'confirm':");
            console.Output("  chunkracetest confirm [seconds] [threads]");
            return;
        }

        int seconds = 10;
        int threads = 2;
        if (_params.Count > 1 && int.TryParse(_params[1], out var s) && s > 0) seconds = s;
        if (_params.Count > 2 && int.TryParse(_params[2], out var t) && t > 1) threads = t;

        if (Interlocked.CompareExchange(ref running, 1, 0) != 0)
        {
            console.Output("chunkracetest: a run is already in progress.");
            return;
        }

        var provider = world.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
        var rfm = provider != null ? provider.m_RegionFileManager : null;
        if (rfm == null)
        {
            console.Output("chunkracetest: no RegionFileManager on this world's chunk provider.");
            Interlocked.Exchange(ref running, 0);
            return;
        }

        List<long> keys;
        var chunksInSaveDir = rfm.chunksInSaveDir;
        lock (chunksInSaveDir)
        {
            keys = new List<long>(chunksInSaveDir.Keys);
        }

        if (keys.Count == 0)
        {
            console.Output("chunkracetest: no chunks in the save directory yet. Explore a little first so region files exist.");
            Interlocked.Exchange(ref running, 0);
            return;
        }

        console.Output(string.Format(
            "chunkracetest: hammering {0} saved chunks with {1} threads for {2}s. " +
            "Watch the log for 'EXCEPTION: In load chunk' (unpatched) or " +
            "'RegionFileLoadChunkLock' contention lines (patched). Summary follows in the log.",
            keys.Count, threads, seconds));

        long totalLoads = 0;
        long nullLoads = 0;
        int liveThreads = threads;

        for (int i = 0; i < threads; i++)
        {
            int threadIndex = i;
            var worker = new Thread(() =>
            {
                var rnd = new System.Random(unchecked((Environment.TickCount * 397) ^ threadIndex));
                var timer = Stopwatch.StartNew();
                long loads = 0;
                long nulls = 0;
                try
                {
                    while (timer.Elapsed.TotalSeconds < seconds)
                    {
                        long key = keys[rnd.Next(keys.Count)];
                        Chunk chunk = null;
                        try
                        {
                            chunk = rfm.snapshotUtil.LoadChunk(rfm.saveDirectory, key);
                        }
                        catch (Exception ex)
                        {
                            // LoadChunk normally swallows its own failures; anything escaping is notable.
                            Log.Warning("chunkracetest: LoadChunk threw past its own handler: " + ex.Message);
                        }

                        loads++;
                        if (chunk == null) nulls++;
                        else MemoryPools.PoolChunks.FreeSync(chunk);
                    }
                }
                finally
                {
                    Interlocked.Add(ref totalLoads, loads);
                    Interlocked.Add(ref nullLoads, nulls);
                    if (Interlocked.Decrement(ref liveThreads) == 0)
                    {
                        Log.Out(string.Format(
                            "chunkracetest: done. {0} loads on {1} threads, {2} returned null. " +
                            "Nulls beyond the first occurrence of a key mean that chunk was already " +
                            "torn and DELETED earlier in the run — check for 'EXCEPTION: In load chunk' " +
                            "above and error_backup_* files in the region folder.",
                            Interlocked.Read(ref totalLoads), threads, Interlocked.Read(ref nullLoads)));
                        Interlocked.Exchange(ref running, 0);
                    }
                }
            });
            worker.IsBackground = true;
            worker.Name = "ChunkRaceTest-" + threadIndex;
            worker.Start();
        }
    }
}
