using System;
using System.Collections.Generic;
using System.IO;

// Versioned, component-framed container for the SCore-specific section of an entity's save
// record (EntityAliveSDX / EntityAliveSDXV4).
//
// Why this exists: EntityCreationData stores each entity's Write() output behind a ushort
// length prefix. Vanilla truncates the length modulo 65536 while still writing the full byte
// array, so any entity record over 65535 bytes misaligns the chunk's entity list on the next
// load — and ChunkSnapshotUtil.LoadChunk responds to that exception by deleting the whole
// chunk. Framing each component with its own length lets a single bad or oversized component
// be dropped or skipped without losing the NPC, and lets the writer shed components to stay
// under the vanilla limit so the chunk always stays loadable.
public static class SdxEntityPersistence
{
    // Little-endian on stream: DE C0 ED FE. A legacy record starts with a 7-bit-encoded string
    // length (the NPC's name), which cannot produce this byte sequence for any realistic name,
    // so the magic reliably separates the new format from legacy saves.
    public const uint Magic = 0xFEEDC0DE;
    public const byte FormatVersion = 1;

    // EntityCreationData.write's ushort length prefix — the hard cap for the ENTIRE entity
    // record (base class data included), not just the SCore section.
    public const int EcdBlobLimit = ushort.MaxValue;

    public const byte ComponentIdentity = 1;
    public const byte ComponentQuestJournal = 2;
    public const byte ComponentPatrol = 3;
    public const byte ComponentBuffsProgression = 4;
    public const byte ComponentWeapon = 5;
    public const byte ComponentLoot = 6;

    public const int NeverDrop = int.MaxValue;

    private const int SectionHeaderBytes = 6;   // magic(4) + version(1) + count(1)
    private const int ComponentFrameBytes = 5;  // id(1) + length(4)

    private class Component
    {
        public byte Id;
        public string Name;
        public int DropOrder;
        public byte[] Payload;
    }

    public class Section
    {
        private readonly string _owner;
        private readonly List<Component> _components = new List<Component>();

        public Section(string owner)
        {
            _owner = owner;
        }

        // Serializes one component into its own buffer. A component whose writer throws is
        // dropped whole (never half-written into the record) so the rest of the section stays
        // parseable.
        public void Add(byte id, string name, int dropOrder, Action<PooledBinaryWriter> writer)
        {
            try
            {
                using (var buffer = new MemoryStream())
                {
                    using (var bw = MemoryPools.poolBinaryWriter.AllocSync(false))
                    {
                        bw.SetBaseStream(buffer);
                        writer(bw);
                        bw.Flush();
                    }

                    _components.Add(new Component {
                        Id = id, Name = name, DropOrder = dropOrder, Payload = buffer.ToArray()
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"{_owner}.Write: failed to serialize '{name}' component; dropping it from this save: {ex.Message}");
            }
        }

        public void WriteTo(BinaryWriter bw)
        {
            // Bytes already written by base.Write and earlier fields; the ushort limit applies
            // to the whole record. The stream is EntityCreationData's per-entity MemoryStream,
            // so Position equals the record size so far.
            long bytesUsed = bw.BaseStream.CanSeek ? bw.BaseStream.Position : 0;

            while (bytesUsed + SectionSize() > EcdBlobLimit)
            {
                Component drop = null;
                foreach (var component in _components)
                {
                    if (component.DropOrder == NeverDrop) continue;
                    if (drop == null || component.DropOrder < drop.DropOrder) drop = component;
                }

                if (drop == null)
                {
                    Log.Error($"{_owner}.Write: record is {bytesUsed + SectionSize()} bytes even with all droppable components removed; over the {EcdBlobLimit} byte vanilla limit, the chunk holding this entity is at risk on next load.");
                    break;
                }

                _components.Remove(drop);
                Log.Error($"{_owner}.Write: record would exceed the {EcdBlobLimit} byte vanilla save limit; dropping '{drop.Name}' ({drop.Payload.Length} bytes) to keep the chunk loadable.");
            }

            bw.Write(Magic);
            bw.Write(FormatVersion);
            bw.Write((byte)_components.Count);
            foreach (var component in _components)
            {
                bw.Write(component.Id);
                bw.Write(component.Payload.Length);
                bw.Write(component.Payload);
            }
        }

        private long SectionSize()
        {
            long total = SectionHeaderBytes;
            foreach (var component in _components) total += ComponentFrameBytes + component.Payload.Length;
            return total;
        }
    }

    // Reads a section written by Section.WriteTo. Returns false — with the stream position
    // restored — when the data is a legacy (pre-framing) record, so the caller can run its old
    // sequential read. Each component parses from its own bounded slice: one bad component is
    // skipped with a warning and cannot misalign the ones after it.
    public static bool TryReadSection(BinaryReader br, string owner, Action<byte, PooledBinaryReader> parseComponent)
    {
        var stream = br.BaseStream;

        // Every real caller hands us EntityCreationData's MemoryStream. A non-seekable stream
        // can't be probed-and-rewound, so treat it as the new format and let the framing guards
        // below contain any surprise.
        long start = stream.CanSeek ? stream.Position : -1L;
        if (stream.CanSeek && stream.Length - start < SectionHeaderBytes) return false;

        if (br.ReadUInt32() != Magic)
        {
            if (!stream.CanSeek)
            {
                Log.Error($"{owner}.Read: expected framed record on non-seekable stream; entity extras skipped.");
                return true;
            }
            stream.Position = start;
            return false;
        }

        br.ReadByte(); // format version; only version 1 exists so far
        int count = br.ReadByte();
        for (int i = 0; i < count; i++)
        {
            if (stream.CanSeek && stream.Length - stream.Position < ComponentFrameBytes)
            {
                Log.Warning($"{owner}.Read: record truncated at component {i + 1}/{count}; remaining components skipped.");
                break;
            }

            byte id = br.ReadByte();
            int length = br.ReadInt32();
            if (length < 0 || (stream.CanSeek && length > stream.Length - stream.Position))
            {
                Log.Warning($"{owner}.Read: component {id} declares invalid length {length}; remaining components skipped.");
                break;
            }

            var payload = br.ReadBytes(length);
            try
            {
                using (var buffer = new MemoryStream(payload))
                using (var sub = MemoryPools.poolBinaryReader.AllocSync(false))
                {
                    sub.SetBaseStream(buffer);
                    parseComponent(id, sub);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"{owner}.Read: failed to read component {id}; skipping it: {ex.Message}");
            }
        }

        return true;
    }
}
