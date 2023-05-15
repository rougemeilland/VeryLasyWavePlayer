using System;
using System.IO;

namespace Palmtree.Media.Wave
{
    public class WaveId3TagChunk
    {
        private WaveId3TagChunk(int totalChunkSize, byte[] rawData)
        {
            TotalBytes = totalChunkSize;
            RawData = rawData;
        }

        public ReadOnlyMemory<byte> RawData { get; }

        public static WaveId3TagChunk ReadFromStream(Stream inStream)
        {
            var chunkSize = inStream.ReadUint32Le();
            var data = new byte[chunkSize];
            _ = inStream.ReadBytesExactly(data);
            if ((chunkSize & 1) != 0)
            {
                _ = inStream.ReadByteExactly();
                return new WaveId3TagChunk(checked((int)chunkSize + 8 + 1), data);
            }
            else
            {
                return new WaveId3TagChunk(checked((int)chunkSize + 8), data);
            }
        }

        internal int TotalBytes { get; }
    }
}
