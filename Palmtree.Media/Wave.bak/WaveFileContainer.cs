using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Palmtree.Media.Wave
{
    public class WaveFileContainer
    {
        private WaveFileContainer(WaveFormatChunk format, WaveDataChunk data, WaveFactChunk? fact, WaveId3TagChunk? id3, WaveListChunk? list)
        {
            Format = format;
            Data = data;
            Fact = fact;
            Id3 = id3;
            List = list;
        }

        public int SamplesPerSeconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => checked((int)Format.SamplesPerSeconds);
        }

        public int Channels
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Format.Channels;
        }

        public int ValidBitsPerSample
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Format.ValidBitsPerSample;
        }

        public WaveSampleDataType SampleDataType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Format.SampleDataType;
        }

        public ChannelLayout ChannelLayout
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Format.ChannelLayout;
        }

        public ReadOnlyMemory<byte> SampleData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Data.SampleData;
        }

        public int TotalFrames
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Fact is not null ? (int)Fact.SampleLength : Data.GetSampleLength(Format.BlockSize);
        }

        public int GetFrameNumberFromTime(TimeSpan time)
            => checked((int)Math.Floor(time.TotalSeconds * Format.SamplesPerSeconds));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlyMemory<byte> GetSampleDataRegion(int frame, int frameLength)
            => Data.GetSampleDataRegion(Format.BlockSize, frame, frameLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetSampleDataAsU8(int channel, int frame)
            => Data.GetSampleDataAsU8(Format.BlockSize, channel, frame);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetSampleDataAsS16LE(int channel, int frame)
            => Data.GetSampleDataAsS16LE(Format.BlockSize, channel, frame);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleDataAsS24LE(int channel, int frame)
            => Data.GetSampleDataAsS24LE(Format.BlockSize, channel, frame);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSampleDataAsS32LE(int channel, int frame)
            => Data.GetSampleDataAsS32LE(Format.BlockSize, channel, frame);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetSampleDataAsS64LE(int channel, int frame)
            => Data.GetSampleDataAsS64LE(Format.BlockSize, channel, frame);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSampleDataAsF32LE(int channel, int frame)
            => Data.GetSampleDataAsF32LE(Format.BlockSize, channel, frame);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetSampleDataAsF64LE(int channel, int frame)
            => Data.GetSampleDataAsF64LE(Format.BlockSize, channel, frame);

        public static WaveFileContainer ReadFromStream(Stream inStream)
        {
            try
            {
                if (inStream.ReadUint32Le() != 0x46464952)
                    throw new BadMediaFormatException("Input stream is not in RIFF format.");
                var chunkSize = checked((int)inStream.ReadUint32Le());
                if (chunkSize < 4)
                    throw new BadMediaFormatException("Input stream is too short.");
                if (inStream.ReadUint32Le() != 0x45564157)
                    throw new BadImageFormatException("Input stream is not in WAVE format.");
                chunkSize -= 4;

                var formatChunk = (WaveFormatChunk?)null;
                var dataChunk = (WaveDataChunk?)null;
                var id3Chunk = (WaveId3TagChunk?)null;
                var factChunk = (WaveFactChunk?)null;
                var listChunk = (WaveListChunk?)null;
                while (true)
                {
                    if (chunkSize == 0)
                        break;
                    if (chunkSize < 4)
                        throw new BadMediaFormatException("Incorrect chunk size. Maybe your wavestream is corrupted.");
                    var chunkId = inStream.ReadUint32Le();
                    switch (chunkId)
                    {
                        case 0x20746d66: // fmt
                            formatChunk = WaveFormatChunk.ReadFromStream(inStream);
                            chunkSize -= formatChunk.TotalBytes;
                            break;
                        case 0x61746164: // data
                            dataChunk = WaveDataChunk.ReadFromStream(inStream);
                            chunkSize -= dataChunk.TotalBytes;
                            break;
                        case 0x20336469: // id3
                            id3Chunk = WaveId3TagChunk.ReadFromStream(inStream);
                            chunkSize -= id3Chunk.TotalBytes;
                            break;
                        case 0x74636166: // fact
                            factChunk = WaveFactChunk.ReadFromStream(inStream);
                            chunkSize -= factChunk.TotalBytes;
                            break;
                        case 0x5453494c: // LIST
                            listChunk = WaveListChunk.ReadFromStream(inStream);
                            chunkSize -= listChunk.TotalBytes;
                            break;
                        default:
                            throw new BadMediaFormatException($"An unsupported chunk was found.: 0x{chunkId:x8}(\"{(char)(byte)(chunkId >> 0)}{(char)(byte)(chunkId >> 8)}{(char)(byte)(chunkId >> 16)}{(char)(byte)(chunkId >> 24)}\")");
                    }
                }

                return
                    inStream.ReadByteOrEos() is not null
                    ? throw new BadMediaFormatException("The end of RIFF data has been reached, but there is still data left in the stream.")
                    : formatChunk is null
                    ? throw new BadMediaFormatException("The wave stream does not contain chunk \"fmt\".")
                    : dataChunk is null
                    ? throw new BadMediaFormatException("The wave stream does not contain chunk \"data\".")
                    : new WaveFileContainer(formatChunk, dataChunk, factChunk, id3Chunk, listChunk);
            }
            catch (UnexpectedEndOfFileException ex)
            {
                throw new BadMediaFormatException("The end of the stream was reached unexpectedly. The input stream is probably corrupted.", ex);
            }
        }

        internal WaveFormatChunk Format { get; }
        internal WaveDataChunk Data { get; }
        internal WaveFactChunk? Fact { get; }
        internal WaveId3TagChunk? Id3 { get; }
        internal WaveListChunk? List { get; }
    }
}
