using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Palmtree.Media.Wave
{
    public class WaveDataChunk
    {
        private readonly byte[] _sampleData;

        private WaveDataChunk(int totalChunkSize, byte[] sampleData)
        {
            TotalBytes = totalChunkSize;
            SampleData = sampleData;
            _sampleData = sampleData;
        }

        public ReadOnlyMemory<byte> SampleData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public int GetSampleLength(int blockSize)
        {
            var (q, r) = Math.DivRem(_sampleData.Length, blockSize);
            if (r != 0)
                throw new BadImageFormatException("Total size of sample data is not a multiple of block size. Maybe your wave stream is corrupted.");
            return q;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> GetSampleDataRegion(int blockSize, int frame, int frameLength)
            => _sampleData.AsMemory().Slice(blockSize * frame, blockSize * frameLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetSampleDataAsU8(int blockSize, int channel, int frame)
            => _sampleData[frame * blockSize + channel];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetSampleDataAsS16LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan().Slice(frame * blockSize + channel * sizeof(short), sizeof(short)).InternalAsInt16Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleDataAsS24LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan().Slice(frame * blockSize + channel * 3, 3).AsInt24Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleDataAsS32LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan().Slice(frame * blockSize + channel * sizeof(int), sizeof(int)).InternalAsInt32Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetSampleDataAsS64LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan().Slice(frame * blockSize + channel * sizeof(long), sizeof(long)).InternalAsInt64Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSampleDataAsF32LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan().Slice(frame * blockSize + channel * sizeof(float), sizeof(float)).InternalAsSingleLe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetSampleDataAsF64LE(int blockSize, int channel, int frame)
            => _sampleData.AsSpan().Slice(frame * blockSize + channel * sizeof(double), sizeof(double)).InternalAsDoubleLe();

        public static WaveDataChunk ReadFromStream(Stream inStream)
        {
            var chunkSize = inStream.ReadUint32Le();
            var data = new byte[chunkSize];
            _ = inStream.ReadBytesExactly(data);
            if ((chunkSize & 1) != 0)
            {
                _ = inStream.ReadByteExactly();
                return new WaveDataChunk(checked((int)chunkSize + 1 + 8), data);
            }
            else
            {
                return new WaveDataChunk(checked((int)chunkSize + 8), data);
            }
        }

        internal int TotalBytes { get; }
    }
}
