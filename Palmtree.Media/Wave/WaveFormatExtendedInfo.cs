using System.Collections.Generic;

namespace Palmtree.Media.Wave
{
    internal abstract class WaveFormatExtendedInfo
    {
        public static WaveFormatExtendedInfo Deserialize(WaveFormatTag formatTag, ReadOnlySpan<byte> data)
        {
            switch (formatTag)
            {
                case WaveFormatTag.WAVE_FORMAT_EXTENSIBLE:
                    return WaveFormatExtensible.Deserialize(data);
                default:
                    throw new BadMediaFormatException("The format of the additional information in the \"fmt\" chunk is unknown.");
            }
        }

        internal abstract IEnumerable<ReadOnlySpan<byte>> Serialize();

        internal abstract int TotalBytes { get; }
    }
}
