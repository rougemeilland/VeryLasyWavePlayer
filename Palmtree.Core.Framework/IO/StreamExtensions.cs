using System.IO;
using System.Runtime.CompilerServices;

namespace Palmtree.IO
{
    public static class StreamExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByteExactly(this Stream inStream)
        {
            var value = inStream.ReadByte();
            if (value < 0)
                throw new UnexpectedEndOfFileException("The end of the stream was reached unexpectedly.");
            return (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> ReadBytesExactly(this Stream inStream, Span<byte> buffer)
        {
            var length = InternalReadBytes(inStream, buffer.InternalSourceArray, buffer.InternalOffset, buffer.Length);
            return
                length <= 0
                ? Span<byte>.Empty
                : length == buffer.Length
                ? buffer.Slice(0, length)
                : throw new UnexpectedEndOfFileException("The end of the stream was reached unexpectedly.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte? ReadByteOrEos(this Stream inStream)
        {
            var value = inStream.ReadByte();
            if (value < 0)
                return null;
            return (byte)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> ReadBytesExactlyOrEos(this Stream inStream, Span<byte> buffer)
        {
            var length = InternalReadBytes(inStream, buffer.InternalSourceArray, buffer.InternalOffset, buffer.Length);
            return
                length <= 0
                ? Span<byte>.Empty
                : length == buffer.Length
                ? buffer.Slice(0, length)
                : throw new UnexpectedEndOfFileException("The end of the stream was reached unexpectedly.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Write(this Stream outStream, Span<byte> buffer)
            => outStream.Write(buffer.InternalSourceArray, buffer.InternalOffset, buffer.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream AsInputStream(this Span<byte> buffer)
            => new MemoryStream(buffer.InternalSourceArray, buffer.InternalOffset, buffer.Length, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Stream AsInputStream(this ReadOnlySpan<byte> buffer)
            => new MemoryStream(buffer.InternalSourceArray, buffer.InternalOffset, buffer.Length, false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int InternalReadBytes(Stream inStream, byte[] buffer, int offset, int count)
        {
            var pos = offset;
            var remain = count;
            while (remain > 0)
            {
                var length = inStream.Read(buffer, pos, remain);
                if (length <= 0)
                    break;
                pos += length;
                remain -= length;
            }

            return count - remain;
        }
    }
}
