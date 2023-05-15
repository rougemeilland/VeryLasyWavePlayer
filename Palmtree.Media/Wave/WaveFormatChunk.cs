using System;
using System.Collections.Generic;
using System.Linq;

namespace Palmtree.Media.Wave
{
    internal class WaveFormatChunk
    {
        public const uint ChunkId = 0x20746d66;

        private readonly WaveFormatExtendedInfo _extendedInfo;

        private WaveFormatChunk(
            int totalChunkSize,
            WaveFormatTag formatType,
            ushort channels,
            uint samplesPerSeconds,
            uint averageBytesPerSeconds,
            ushort blockSize,
            ushort bitsPerSample,
            WaveFormatExtendedInfo extendedInfo)
        {
            TotalBytes = totalChunkSize;
            FormatType = formatType;
            Channels = channels;
            SamplesPerSeconds = samplesPerSeconds;
            AverageBytesPerSeconds = averageBytesPerSeconds;
            BlockSize = blockSize;
            BitsPerSample = bitsPerSample;
            _extendedInfo = extendedInfo;
        }

        public WaveFormatTag FormatType { get; }
        public ushort Channels { get; }
        public uint SamplesPerSeconds { get; }
        public uint AverageBytesPerSeconds { get; }

        /// <remarks>
        ///  == <see cref="Channels"/>  * <see cref="BitsPerSample"/> / 8
        /// </remarks>
        public ushort BlockSize { get; }
        public ushort BitsPerSample { get; }

        public int ValidBitsPerSample
        {
            get
            {
                if (_extendedInfo is null)
                {
                    switch (BitsPerSample)
                    {
                        case 8:
                        case 16:
                            return BitsPerSample;
                        default:
                            throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=(null)");
                    }
                }
                else if (_extendedInfo is WaveFormatExtensible extendedFormatExtensible)
                {
                    return extendedFormatExtensible.Samples;
                }
                else
                {
                    throw new BadMediaFormatException("Unknown valid bits per sample.");
                }
            }
        }

        public WaveSampleDataType SampleDataType
        {
            get
            {
                if (_extendedInfo is null)
                {
                    switch (BitsPerSample)
                    {
                        case 8:
                            return WaveSampleDataType.Unsigned8bit;
                        case 16:
                            return WaveSampleDataType.LittleEndianSigned16bit;
                        default:
                            throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=(null)");
                    }
                }
                else if (_extendedInfo is WaveFormatExtensible extendedFormatExtensible)
                {
                    if (extendedFormatExtensible.SubFormat == WaveFormatExtensible.SUBTYPE_PCM)
                    {
                        switch (BitsPerSample)
                        {
                            case 8:
                                return WaveSampleDataType.Unsigned8bit;
                            case 16:
                                return WaveSampleDataType.LittleEndianSigned16bit;
                            case 24:
                                return WaveSampleDataType.LittleEndianSigned24bit;
                            case 32:
                                return WaveSampleDataType.LittleEndianSigned32bit;
                            case 64:
                                return WaveSampleDataType.LittleEndianSigned64bit;
                            default:
                                throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=\"{extendedFormatExtensible.SubFormat}\"");
                        }
                    }
                    else if (extendedFormatExtensible.SubFormat == WaveFormatExtensible.SUBTYPE_IEEE_FLOAT)
                    {
                        switch (BitsPerSample)
                        {
                            case 32:
                                return WaveSampleDataType.LittleEndian32bitFloat;
                            case 64:
                                return WaveSampleDataType.LittleEndian64bitFloat;
                            default:
                                throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=\"{extendedFormatExtensible.SubFormat}\"");
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"Unsupported sub format.: subFormat=\"{extendedFormatExtensible.SubFormat}\"");
                    }
                }
                else
                {
                    throw new BadMediaFormatException("Unknown sample data type.");
                }
            }
        }

        public ChannelLayout ChannelLayout
        {
            get
            {
                if (_extendedInfo is null)
                {
                    switch (Channels)
                    {
                        case 1:
                            return ChannelLayout.AUDIO_SPEAKER_MONO;
                        case 2:
                            return ChannelLayout.AUDIO_SPEAKER_STEREO;
                        default:
                            throw new BadMediaFormatException($"Wrong number of channels. Maybe your wave stream is corrupted.: channels={Channels}, ExtendedInfo=(null)");
                    }
                }
                else if (_extendedInfo is WaveFormatExtensible extendedFormatExtensible)
                {
                    return extendedFormatExtensible.ChannelMask;
                }
                else
                {
                    throw new BadMediaFormatException("Unknown channel layout.");
                }
            }
        }

        public static WaveFormatChunk Deserialize(ReadOnlySpan<byte> buffer)
        {
            var chunkId = buffer.Slice(0, 4).AsUint32Le();
            if (chunkId != ChunkId)
                throw new ArgumentException($"A \"{nameof(buffer)}\" is not a \"fmt\" chunk.", nameof(buffer));
            var chunkSize = checked((int)buffer.Slice(4, 4).AsUint32Le());
            if (checked(8 + chunkSize) > buffer.Length)
                throw new BadMediaFormatException($"The size of the \"fmt\" chunk is too large. Maybe your wave stream is corrupted.: chunkSize={chunkSize}");
            buffer = buffer.Slice(8, chunkSize);

            if (chunkSize < 16)
                throw new BadImageFormatException("\"fmt\" chunk is too short. Maybe your wave stream is corrupted.");
            var waveFormatTag = (WaveFormatTag)buffer.Slice(0, 2).AsUint16Le();
            var channels = buffer.Slice(2, 2).AsUint16Le();
            var samplesPerSeconds = buffer.Slice(4, 4).AsUint32Le();
            var averageBytesPerSeconds = buffer.Slice(8, 4).AsUint32Le();
            var blockSize = buffer.Slice(12, 2).AsUint16Le();
            var bitsPerSample = buffer.Slice(14, 2).AsUint16Le();
            buffer = buffer.Slice(16);
            if (waveFormatTag != WaveFormatTag.WAVE_FORMAT_PCM && waveFormatTag != WaveFormatTag.WAVE_FORMAT_EXTENSIBLE)
                throw new BadMediaFormatException($"Unsupported WAVE format.: formatTag={waveFormatTag}");
            if (waveFormatTag == WaveFormatTag.WAVE_FORMAT_PCM && chunkSize != 16)
                throw new BadMediaFormatException($"Wrong size of \"fmt\" chunk.: formatTag={waveFormatTag}, chunkSize={chunkSize}");
            if (buffer.IsEmpty)
            {
                return
                    new WaveFormatChunk(
                        checked(chunkSize + 8),
                        waveFormatTag,
                        channels,
                        samplesPerSeconds,
                        averageBytesPerSeconds,
                        blockSize,
                        bitsPerSample,
                        null);
            }
            else
            {
                if (buffer.Length < 2)
                    throw new BadMediaFormatException("The length of the \"fmt\" chunk is incorrect. Maybe your wavestream is corrupted.");
                var extendedInfoSize = buffer.Slice(0, 2).AsUint16Le();
                if (checked(2 + extendedInfoSize) != buffer.Length)
                    throw new BadMediaFormatException("The length of the \"fmt\" chunk is incorrect. Maybe your wavestream is corrupted.");
                var extendedInfo = WaveFormatExtendedInfo.Deserialize(waveFormatTag, buffer.Slice(2));
                return
                    new WaveFormatChunk(
                        checked(chunkSize + 8),
                        waveFormatTag,
                        channels,
                        samplesPerSeconds,
                        averageBytesPerSeconds,
                        blockSize,
                        bitsPerSample,
                        extendedInfo);
            }
        }

        internal IEnumerable<ReadOnlySpan<byte>> Serialize()
        {
            var basicHeaderBytes = new byte[8 + 18];
            var basicHeader = basicHeaderBytes.AsSpan();
            basicHeader.Slice(0, 4).StoreUint32Le(ChunkId);
            basicHeader.Slice(8, 2).StoreUint16Le(checked((ushort)FormatType));
            basicHeader.Slice(10, 2).StoreUint16Le(Channels);
            basicHeader.Slice(12, 4).StoreUint32Le(SamplesPerSeconds);
            basicHeader.Slice(16, 4).StoreUint32Le(AverageBytesPerSeconds);
            basicHeader.Slice(20, 2).StoreUint16Le(BlockSize);
            basicHeader.Slice(22, 2).StoreUint16Le(BitsPerSample);

            switch (FormatType)
            {
                case WaveFormatTag.WAVE_FORMAT_PCM:
                    basicHeader.Slice(4, 4).StoreUint32Le(16);
                    yield return basicHeader.Slice(0, 8 + 16);
                    yield break;
                case WaveFormatTag.WAVE_FORMAT_EXTENSIBLE:
                {
                    var extendedHeaderSegemts = _extendedInfo is null ? new ReadOnlySpan<byte>[0] : _extendedInfo.Serialize();
                    var extendedHeaderSize = extendedHeaderSegemts.Sum(segment => segment.Length);
                    basicHeader.Slice(4, 4).StoreUint32Le(checked((ushort)(18 + extendedHeaderSize)));
                    basicHeader.Slice(24, 2).StoreUint16Le(checked((ushort)extendedHeaderSize));
                    yield return basicHeader.Slice(0, 8 + 18);
                    foreach (var segment in extendedHeaderSegemts)
                        yield return segment;
                    yield break;
                }
                default:
                    throw new BadMediaFormatException($"Unsupported WAVE format.: formatTag={FormatType}");
            }
        }

        internal int TotalBytes { get; }
    }
}
