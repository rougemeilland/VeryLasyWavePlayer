using System;
using System.IO;

namespace Palmtree.Media.Wave
{
    public class WaveListChunk
    {
        private WaveListChunk(int totalChunkSize, byte[] rawData)
        {
            TotalBytes = totalChunkSize;
            RawData = rawData;
        }

        public static WaveListChunk ReadFromStream(Stream inStream)
        {
            var chunkSize = inStream.ReadUint32Le();
            var data = new byte[chunkSize];
            _ = inStream.ReadBytesExactly(data);
            if ((chunkSize & 1) != 0)
            {
                _ = inStream.ReadByteExactly();
                return new WaveListChunk(checked((int)chunkSize + 8 + 1), data);
            }
            else
            {
                return new WaveListChunk(checked((int)chunkSize + 8), data);
            }
        }

        public ReadOnlyMemory<byte> RawData { get; }

        internal int TotalBytes { get; }
    }
}
