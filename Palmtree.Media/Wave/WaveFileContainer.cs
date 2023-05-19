using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Palmtree.IO;

namespace Palmtree.Media.Wave
{
    public class WaveFileContainer
    {
        private const int _riffChunkId = 0x46464952;
        private const int _waveFormatId = 0x45564157;

        private WaveFileContainer(WaveFormatChunk format, WaveDataChunk data, WaveFactChunk fact, WaveId3TagChunk id3, WaveListChunk list, IEnumerable<WaveUnknownChunk> others)
        {
            Format = format ?? throw new ArgumentNullException(nameof(format));
            Data = data ?? throw new ArgumentNullException(nameof(data));
            Fact = fact;
            Id3 = id3;
            List = list;
            Others = others.ToList();
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

        public ReadOnlySpan<byte> SampleData
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Data.RawSampleData;
        }

        public int TotalFrames
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Fact is null ? Data.GetSampleLength(Format.BlockSize) : (int)Fact.SampleLength;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetFrameNumberFromTime(TimeSpan time)
            => checked((int)Math.Floor(time.TotalSeconds * Format.SamplesPerSeconds));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan GetTimeFromFrameNumber(int frame)
            => TimeSpan.FromSeconds((double)frame / Format.SamplesPerSeconds);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetSampleDataRegion(int frame, int frameLength)
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

        public static WaveFileContainer Deserialize(ReadOnlySpan<byte> buffer)
        {
            try
            {
                if (buffer.Slice(0, 4).AsUint32Le() != _riffChunkId)
                    throw new BadMediaFormatException("Input stream is not in RIFF format.");
                var chunkSize = checked((int)buffer.Slice(4, 4).AsUint32Le());
                if (chunkSize < 4)
                    throw new BadMediaFormatException("Input stream is too short.");
                if (checked(8 + chunkSize) != buffer.Length)
                    throw new BadMediaFormatException("The end of RIFF data has been reached, but there is still data left in the stream.");
                buffer = buffer.Slice(8);

                if (buffer.Slice(0, 4).AsUint32Le() != _waveFormatId)
                    throw new BadImageFormatException("Input stream is not in WAVE format.");
                buffer = buffer.Slice(4);

                var formatChunk = (WaveFormatChunk)null;
                var dataChunk = (WaveDataChunk)null;
                var id3Chunk = (WaveId3TagChunk)null;
                var factChunk = (WaveFactChunk)null;
                var listChunk = (WaveListChunk)null;
                var otherChunks = new List<WaveUnknownChunk>();
                while (!buffer.IsEmpty)
                {
                    if (buffer.Length < 8)
                        throw new BadMediaFormatException("Too short chunk found. Maybe your wave stream is corrupted.");

                    var chunkId = buffer.Slice(0, 4).AsUint32Le();
                    switch (chunkId)
                    {
                        case WaveFormatChunk.ChunkId:
                            formatChunk = WaveFormatChunk.Deserialize(buffer);
                            buffer = buffer.Slice(formatChunk.TotalBytes);
                            break;
                        case WaveDataChunk.ChunkId:
                            dataChunk = WaveDataChunk.Deserialize(buffer);
                            buffer = buffer.Slice(dataChunk.TotalBytes);
                            break;
                        case WaveId3TagChunk.ChunkId:
                            id3Chunk = WaveId3TagChunk.Deserialize(buffer);
                            buffer = buffer.Slice(id3Chunk.TotalBytes);
                            break;
                        case WaveFactChunk.ChunkId:
                            factChunk = WaveFactChunk.Deserialize(buffer);
                            buffer = buffer.Slice(factChunk.TotalBytes);
                            break;
                        case WaveListChunk.ChunkId:
                            listChunk = WaveListChunk.Deserialize(buffer);
                            buffer = buffer.Slice(listChunk.TotalBytes);
                            break;
                        default:
                        {
                            var unknownChunk = WaveUnknownChunk.Deserialize(buffer);
                            buffer = buffer.Slice(unknownChunk.TotalBytes);
                            otherChunks.Add(unknownChunk);
                            break;
                        }
                    }
                }

                return
                    formatChunk is null
                    ? throw new BadMediaFormatException("The wave stream does not contain chunk \"fmt\".")
                    : dataChunk is null
                    ? throw new BadMediaFormatException("The wave stream does not contain chunk \"data\".")
                    : new WaveFileContainer(formatChunk, dataChunk, factChunk, id3Chunk, listChunk, otherChunks);
            }
            catch (UnexpectedEndOfFileException ex)
            {
                throw new BadMediaFormatException("The end of the stream was reached unexpectedly. The input stream is probably corrupted.", ex);
            }
        }

        public Span<byte> Serialize()
            => Serialize(Data.Serialize());

        public Span<byte> Serialize(TimeSpan from, TimeSpan length)
        {
            var frameFrom = GetFrameNumberFromTime(from);
            var frameTo = GetFrameNumberFromTime(from + length);
            return Serialize(frameFrom, frameTo - frameFrom);
        }

        public Span<byte> Serialize(int from, int count)
            => Serialize(Data.Serialize(Format.BlockSize, from, count));

        private Span<byte> Serialize(IEnumerable<ReadOnlySpan<byte>> dataChunk)
        {
            var segments = Array.Empty<ReadOnlySpan<byte>>().AsEnumerable();
            segments = segments.Concat(Format.Serialize());
            if (!(Fact is null))
                segments = segments.Concat(Fact.Serialize());
            if (!(Id3 is null))
                segments = segments.Concat(Id3.Serialize());
            if (!(List is null))
                segments = segments.Concat(List.Serialize());
            foreach (var otherChunk in Others)
                segments = segments.Concat(otherChunk.Serialize());
            segments = segments.Concat(dataChunk).ToList();
            var totalSize = segments.Sum(segment => segment.Length);
            var containerBytes = new byte[12 + totalSize];
            var container = containerBytes.AsSpan();
            container.Slice(0, 4).StoreUint32Le(_riffChunkId);
            container.Slice(4, 4).StoreUint32Le(checked((uint)(4 + containerBytes.Length)));
            container.Slice(8, 4).StoreUint32Le(_waveFormatId);
            var buffer = container.Slice(12);
            foreach (var segment in segments)
            {
                segment.CopyTo(buffer);
                buffer = buffer.Slice(segment.Length);
            }

            return container;
        }

        internal WaveFormatChunk Format { get; }
        internal WaveDataChunk Data { get; }
        internal WaveFactChunk Fact { get; }
        internal WaveId3TagChunk Id3 { get; }
        internal WaveListChunk List { get; }
        internal IEnumerable<WaveUnknownChunk> Others {get;}
    }
}
