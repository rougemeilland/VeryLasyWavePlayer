using System;
using System.Collections.Generic;

namespace Palmtree.Media.Wave
{
    internal class WaveFactChunk
    {
        public const uint ChunkId = 0x74636166;

        private WaveFactChunk(int totalChunkSize, uint sampleLength)
        {
            TotalBytes = totalChunkSize;
            SampleLength = sampleLength;
        }

        public uint SampleLength { get; }

        public static WaveFactChunk Deserialize(ReadOnlySpan<byte> buffer)
        {
            var chunkId = buffer.Slice(0, 4).AsUint32Le();
            if (chunkId != ChunkId)
                throw new ArgumentException($"A \"{nameof(buffer)}\" is not a \"fact\" chunk.", nameof(buffer));
            var chunkSize = checked((int)buffer.Slice(4, 4).AsUint32Le());
            if (checked(8 + chunkSize) > buffer.Length)
                throw new BadMediaFormatException($"The size of the \"fact\" chunk is too large. Maybe your wave stream is corrupted.: chunkSize={chunkSize}");
            buffer = buffer.Slice(8, chunkSize);

            if (buffer.Length != 4)
                throw new BadMediaFormatException("The length of the \"fact\" chunk is incorrect. Maybe your wavestream is corrupted.");
            buffer = buffer.Slice(0, 4);
            var sampleLength = buffer.AsUint32Le();
            return new WaveFactChunk(checked(chunkSize + 8), sampleLength);
        }

        public IEnumerable<ReadOnlySpan<byte>> Serialize()
        {
            var byteBuffer = new byte[12];
            var buffer = byteBuffer.AsSpan();
            buffer.Slice(0, 4).StoreUint32Le(ChunkId);
            buffer.Slice(4, 4).StoreUint32Le(4);
            buffer.Slice(8, 4).StoreUint32Le(SampleLength);
            yield return buffer;
        }

        internal int TotalBytes { get; }
    }
}
