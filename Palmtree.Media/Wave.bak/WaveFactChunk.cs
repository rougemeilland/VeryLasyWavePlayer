using System.IO;

namespace Palmtree.Media.Wave
{
    public class WaveFactChunk
    {
        private WaveFactChunk(int totalChunkSize, uint sampleLength)
        {
            TotalBytes = totalChunkSize;
            SampleLength = sampleLength;
        }

        public uint SampleLength { get; }

        public static WaveFactChunk ReadFromStream(Stream inStream)
        {
            var chunkSize = inStream.ReadUint32Le();
            if (chunkSize != 4)
                throw new BadMediaFormatException("The length of the \"fact\" chunk is incorrect. Maybe your wavestream is corrupted.");
            var sampleLength = inStream.ReadUint32Le();
            return new WaveFactChunk(checked((int)chunkSize + 8), sampleLength);
        }

        internal int TotalBytes { get; }
    }
}
