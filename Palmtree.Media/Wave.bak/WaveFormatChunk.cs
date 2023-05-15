using System;
using System.IO;

namespace Palmtree.Media.Wave
{
    public class WaveFormatChunk
    {
        private readonly WaveFormatExtendedInfo? _extendedInfo;

        private WaveFormatChunk(
            int totalChunkSize,
            WaveFormatTag formatType,
            ushort channels,
            uint samplesPerSeconds,
            uint averageBytesPerSeconds,
            ushort blockSize,
            ushort bitsPerSample,
            WaveFormatExtendedInfo? extendedInfo)
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
                    return BitsPerSample switch
                    {
                        8 => 8,
                        16 => 16,
                        _ => throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=(null)"),
                    };
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
                    return BitsPerSample switch
                    {
                        8 => WaveSampleDataType.Unsigned8bit,
                        16 => WaveSampleDataType.LittleEndianSigned16bit,
                        _ => throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=(null)"),
                    };
                }
                else if (_extendedInfo is WaveFormatExtensible extendedFormatExtensible)
                {
                    if (extendedFormatExtensible.SubFormat == WaveFormatExtensible.SUBTYPE_PCM)
                    {
                        return BitsPerSample switch
                        {
                            8 => WaveSampleDataType.Unsigned8bit,
                            16 => WaveSampleDataType.LittleEndianSigned16bit,
                            24 => WaveSampleDataType.LittleEndianSigned24bit,
                            32 => WaveSampleDataType.LittleEndianSigned32bit,
                            64 => WaveSampleDataType.LittleEndianSigned64bit,
                            _ => throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=\"{extendedFormatExtensible.SubFormat}\""),
                        };
                    }
                    else if (extendedFormatExtensible.SubFormat == WaveFormatExtensible.SUBTYPE_IEEE_FLOAT)
                    {
                        return BitsPerSample switch
                        {
                            32 => WaveSampleDataType.LittleEndian32bitFloat,
                            64 => WaveSampleDataType.LittleEndian64bitFloat,
                            _ => throw new BadMediaFormatException($"Wrong number of bitsPerSample. Maybe your wave stream is corrupted.: bitsPerSample={BitsPerSample}, subFormat=\"{extendedFormatExtensible.SubFormat}\""),
                        };
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
                    return Channels switch
                    {
                        1 => ChannelLayout.AUDIO_SPEAKER_MONO,
                        2 => ChannelLayout.AUDIO_SPEAKER_STEREO,
                        _ => throw new BadMediaFormatException($"Wrong number of channels. Maybe your wave stream is corrupted.: channels={Channels}, ExtendedInfo=(null)"),
                    };
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

        public static WaveFormatChunk ReadFromStream(Stream inStream)
        {
            var chunkSize = inStream.ReadUint32Le();
            if (chunkSize < 16)
                throw new BadImageFormatException();
            var waveFormatTag = (WaveFormatTag)inStream.ReadUint16Le();
            var channels = inStream.ReadUint16Le();
            var samplesPerSeconds = inStream.ReadUint32Le();
            var averageBytesPerSeconds = inStream.ReadUint32Le();
            var blockSize = inStream.ReadUint16Le();
            var bitsPerSample = inStream.ReadUint16Le();
            if (waveFormatTag is not WaveFormatTag.WAVE_FORMAT_PCM and not WaveFormatTag.WAVE_FORMAT_EXTENSIBLE)
                throw new BadMediaFormatException($"Unsupported WAVE format.: formatTag={waveFormatTag}");
            if (waveFormatTag == WaveFormatTag.WAVE_FORMAT_PCM && chunkSize != 16)
                throw new BadMediaFormatException($"Wrong size of \"fmt\" chunk.: formatTag={waveFormatTag}, chunkSize={chunkSize}");
            if (chunkSize == 16)
            {
                return
                    new WaveFormatChunk(
                        checked((int)chunkSize + 8),
                        waveFormatTag,
                        channels,
                        samplesPerSeconds,
                        averageBytesPerSeconds,
                        blockSize,
                        bitsPerSample,
                        null);
            }
            else if (chunkSize < 18)
            {
                throw new BadMediaFormatException("The length of the \"fmt\" chunk is incorrect. Maybe your wavestream is corrupted.");
            }
            else
            {
                var extendedInfoSize = inStream.ReadUint16Le();
                unsafe
                {
                    Span<byte> extendedInfoBuffer = stackalloc byte[extendedInfoSize];
                    _ = inStream.ReadBytesExactly(extendedInfoBuffer);
                    var extendedInfo = WaveFormatExtendedInfo.Parse(waveFormatTag, extendedInfoBuffer);
                    return
                        new WaveFormatChunk(
                            checked((int)chunkSize + 8),
                            waveFormatTag,
                            channels,
                            samplesPerSeconds,
                            averageBytesPerSeconds,
                            blockSize,
                            bitsPerSample,
                            extendedInfo);
                }
            }
        }

        internal int TotalBytes { get; }
    }
}
