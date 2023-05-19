using System.Collections.Generic;

namespace Palmtree.Media.Wave
{
    internal class WaveUnknownChunk
    {
        private WaveUnknownChunk(uint chunkId, int totalChunkSize, ReadOnlySpan<byte> rawData)
        {
            ChunkId = chunkId;
            TotalBytes = totalChunkSize;
            var rawDataBuffer = new byte[rawData.Length];
            rawData.CopyTo(rawDataBuffer);
            RawData = rawDataBuffer;
        }

        public uint ChunkId { get; }

        public ReadOnlySpan<byte> RawData { get; }

        public static WaveUnknownChunk Deserialize(ReadOnlySpan<byte> buffer)
        {
            var chunkId = buffer.Slice(0, 4).AsUint32Le();
            var chunkSize = checked((int)buffer.Slice(4, 4).AsUint32Le());
            if (checked(8 + chunkSize) > buffer.Length)
                throw new BadMediaFormatException($"The size of the \"note\" chunk is too large. Maybe your wave stream is corrupted.: chunkSize={chunkSize}");
            buffer = buffer.Slice(8, chunkSize);

            return
                new WaveUnknownChunk(
                    chunkId,
                    (chunkSize & 1) != 0 ? checked(chunkSize + 1 + 8) : checked(chunkSize + 8),
                    buffer);
        }

        public IEnumerable<ReadOnlySpan<byte>> Serialize()
        {
            var headerBuffer = new byte[12];
            var header = headerBuffer.AsSpan();
            header.Slice(0, 4).StoreUint32Le(ChunkId);
            header.Slice(4, 4).StoreUint32Le(checked((uint)RawData.Length));
            yield return header;
            yield return RawData;
            if ((RawData.Length & 1) != 0)
                yield return new byte[] { 0 };
        }

        internal int TotalBytes { get; }
    }
}
