using System;
using System.Collections.Generic;

namespace Palmtree.Media.Wave
{
    internal class WaveFormatExtensible
        : WaveFormatExtendedInfo
    {
        public static readonly Guid SUBTYPE_PCM = new Guid("00000001-0000-0010-8000-00aa00389b71");
        public static readonly Guid SUBTYPE_IEEE_FLOAT = new Guid("00000003-0000-0010-8000-00aa00389b71");

        private WaveFormatExtensible(ushort samples, uint channelMask, Guid subFormat)
        {
            Samples = samples;
            ChannelMask = (ChannelLayout)channelMask;
            SubFormat = subFormat;
        }

        /// <summary>
        /// wValidBitsPerSample / wSamplesPerBlock / wReserved の何れか。
        /// </summary>
        /// <remarks>
        /// 以下の何れかの意味を持つ。非圧縮の PCM フォーマットの場合は 1 の意味のはず。
        /// <list type="number">
        /// <item>
        /// <term>wValidBitsPerSample</term>
        /// <description>0ではない場合、コンテナに格納されているサンプルデータの有効ビット数。このサンプルデータのうち上位からこのビット数分だけが有効である。wValidBitsPerSampleが 8 の倍数ではない場合、サンプルデータの下位の端数ビット数分は 0 でなければならない。</description>
        /// </item>
        /// <item>
        /// <term>wSamplesPerBlock</term>
        /// <description>0ではない場合、1 個の圧縮ブロックに含まれるサンプルの数。ただし圧縮ブロックに含まれるサンプルの数が可変である場合を除く。</description>
        /// </item>
        /// <item>
        /// <term>wSamplesPerBlock</term>
        /// <description>常に 0 である。</description>
        /// </item>
        /// </list>
        /// </remarks>
        public ushort Samples { get; }
        public ChannelLayout ChannelMask { get; }

        /// <summary>
        /// サンプルデータの型を判別するための追加情報。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// <term><see cref="SUBTYPE_PCM"/> と等しい場合</term>
        /// <description>
        /// サンプルデータの型は以下の何れか。
        /// <list type="bullet">
        /// <item>signed 24bit (little endian)</item>
        /// <item>signed 32bit (little endian)</item>
        /// <item>signed 64 bit (little endian)</item>
        /// </list>
        /// </description>
        /// </item>
        /// <item>
        /// <term><see cref="SUBTYPE_IEEE_FLOAT"/> と等しい場合</term>
        /// <description>
        /// サンプルデータの型は以下の何れか。
        /// <list type="bullet">
        /// <item>32bit single float (little endian)</item>
        /// <item>64bit double float (little endian)</item>
        /// </list>
        /// </description>
        /// </item>
        /// </list>
        /// </remarks>
        public Guid SubFormat { get; }

        internal override int TotalBytes => 22;

        internal static WaveFormatExtendedInfo Deserialize(ReadOnlySpan<byte> data)
        {
            var samples = data.Slice(0, 2).AsUint16Le();
            var channelMask = data.Slice(2, 4).AsUint32Le();
            var subFormat = new Guid(data.Slice(6, 16).ToArray());
            return new WaveFormatExtensible(samples, channelMask, subFormat);
        }

        internal override IEnumerable<ReadOnlySpan<byte>> Serialize()
        {
            var byteBuffer = new byte[22];
            var buffer = byteBuffer.AsSpan();
            buffer.Slice(0, 2).StoreUint16Le(Samples);
            buffer.Slice(2, 4).StoreUint32Le(checked((ushort)ChannelMask));
            buffer.Slice(6, 16).StoreGuid(SubFormat);
            yield return buffer;
        }
    }
}
