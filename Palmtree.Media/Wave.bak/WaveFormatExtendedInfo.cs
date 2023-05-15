using System;

namespace Palmtree.Media.Wave
{
    public abstract class WaveFormatExtendedInfo
    {
        public static WaveFormatExtendedInfo Parse(WaveFormatTag formatTag, ReadOnlySpan<byte> data)
            => formatTag switch
            {
                WaveFormatTag.WAVE_FORMAT_EXTENSIBLE => WaveFormatExtensible.Parse(data),
                _ => throw new BadMediaFormatException("The format of the additional information in the \"fmt\" chunk is unknown."),
            };
    }
}
