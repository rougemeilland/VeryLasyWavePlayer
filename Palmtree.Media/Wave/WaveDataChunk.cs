using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Palmtree.Media.Wave
{
    internal class WaveDataChunk
    {
        public const uint ChunkId = 0x61746164;

        private readonly byte[] _sampleData;

        private WaveDataChunk(int totalChunkSize, ReadOnlySpan<byte> sampleData)
        {
            TotalBytes = totalChunkSize;

            _sampleData = new byte[sampleData.Length];
            sampleData.CopyTo(_sampleData);
            RawSampleData = _sampleData;
        }

        public ReadOnlySpan<byte> RawSampleData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public int GetSampleLength(int blockSize)
        {
            var q = Math.DivRem(_sampleData.Length, blockSize, out var r);
            if (r != 0)
                throw new BadImageFormatException("Total size of sample data is not a multiple of block size. Maybe your wave stream is corrupted.");
            return q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetSampleDataRegion(int blockSize, int frame, int frameLength)
            => _sampleData.AsReadOnlySpan(blockSize * frame, blockSize * frameLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetSampleDataAsU8(int blockSize, int channel, int frame)
            => _sampleData[frame * blockSize + channel];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetSampleDataAsS16LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan(frame * blockSize + channel * sizeof(short), sizeof(short)).AsInt16Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleDataAsS24LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan(frame * blockSize + channel * 3, 3).AsInt24Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleDataAsS32LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan(frame * blockSize + channel * sizeof(int), sizeof(int)).AsInt32Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetSampleDataAsS64LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan(frame * blockSize + channel * sizeof(long), sizeof(long)).AsInt64Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSampleDataAsF32LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan(frame * blockSize + channel * sizeof(float), sizeof(float)).AsSingleLe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetSampleDataAsF64LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan(frame * blockSize + channel * sizeof(double), sizeof(double)).AsDoubleLe();

        public static WaveDataChunk Deserialize(ReadOnlySpan<byte> buffer)
        {
            var chunkId = buffer.Slice(0, 4).AsUint32Le();
            if (chunkId != ChunkId)
                throw new ArgumentException($"A \"{nameof(buffer)}\" is not a \"data\" chunk.", nameof(buffer));
            var chunkSize = checked((int)buffer.Slice(4, 4).AsUint32Le());
            if (checked(8 + chunkSize) > buffer.Length)
                throw new BadMediaFormatException($"The size of the \"data\" chunk is too large. Maybe your wave stream is corrupted.: chunkSize={chunkSize}");
            buffer = buffer.Slice(8, chunkSize);

            return
                new WaveDataChunk(
                    (chunkSize & 1) != 0 ? checked(chunkSize + 1 + 8) : checked(chunkSize + 8),
                    buffer);
        }

        public IEnumerable<ReadOnlySpan<byte>> Serialize()
            => Serialize(RawSampleData);

        public IEnumerable<ReadOnlySpan<byte>> Serialize(int blockSize, int from, int count)
            => Serialize(GetSampleDataRegion(blockSize, from, count));

        internal int TotalBytes { get; }

        private IEnumerable<ReadOnlySpan<byte>> Serialize(ReadOnlySpan<byte> sampleData)
        {
            var headerBuffer = new byte[12];
            var header = headerBuffer.AsSpan();
            header.Slice(0, 4).StoreUint32Le(ChunkId);
            header.Slice(4, 4).StoreUint32Le(checked((uint)sampleData.Length));
            yield return header;
            yield return sampleData;
            if ((sampleData.Length & 1) != 0)
                yield return new byte[] { 0 };
        }
    }
}
