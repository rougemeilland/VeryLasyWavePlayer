using System;
using System.Runtime.CompilerServices;

namespace Palmtree
{
    public static class ByteDataExtensions
    {
        #region Conversion from Span<byte> to each type

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short AsInt16Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt16Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short AsInt16Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt16Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort AsUint16Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint16Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort AsUint16Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint16Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt24Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt24Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt24Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt24Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint24Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint24Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint24Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint24Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt32Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt32Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt32Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt32Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint32Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint32Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint32Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint32Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AsInt64Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt64Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AsInt64Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsInt64Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AsUint64Le(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint64Le();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AsUint64Be(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsUint64Be();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsSingleLe(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsSingleLe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsSingleBe(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsSingleBe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AsDoubleLe(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsDoubleLe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AsDoubleBe(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsDoubleBe();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid AsGuid(this Span<byte> buffer)
            => ((ReadOnlySpan<byte>)buffer).AsGuid();

        #endregion

        #region Conversion from ReadOnlySpan<byte> to each type

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short AsInt16Le(this ReadOnlySpan<byte> buffer)
            => unchecked((short)buffer.AsUint16Le());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short AsInt16Be(this ReadOnlySpan<byte> buffer)
            => unchecked((short)buffer.AsUint16Be());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort AsUint16Le(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(ushort))
                throw GetInvalidBufferSizeException(sizeof(ushort), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return (ushort)((p[0] << 0) | (p[1] << 8));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort AsUint16Be(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(ushort))
                throw GetInvalidBufferSizeException(sizeof(ushort), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return (ushort)((p[1] << 0) | (p[0] << 8));
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt24Le(this ReadOnlySpan<byte> buffer)
            => FromUint24ToInt24(buffer.AsUint24Le());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt24Be(this ReadOnlySpan<byte> buffer)
            => FromUint24ToInt24(buffer.AsUint24Be());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint24Le(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != 3)
                throw GetInvalidBufferSizeException(3, nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return ((uint)p[0] << 0) | ((uint)p[1] << 8) | ((uint)p[2] << 16);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint24Be(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != 3)
                throw GetInvalidBufferSizeException(3, nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return ((uint)p[2] << 0) | ((uint)p[1] << 8) | ((uint)p[0] << 16);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt32Le(this ReadOnlySpan<byte> buffer)
            => unchecked((int)buffer.AsUint32Le());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int AsInt32Be(this ReadOnlySpan<byte> buffer)
            => unchecked((int)buffer.AsUint32Be());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint32Le(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(uint))
                throw GetInvalidBufferSizeException(sizeof(uint), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return ((uint)p[0] << 0) | ((uint)p[1] << 8) | ((uint)p[2] << 16) | ((uint)p[3] << 24);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint AsUint32Be(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(uint))
                throw GetInvalidBufferSizeException(sizeof(uint), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return ((uint)p[3] << 0) | ((uint)p[2] << 8) | ((uint)p[1] << 16) | ((uint)p[0] << 24);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AsInt64Le(this ReadOnlySpan<byte> buffer)
            => unchecked((long)buffer.AsUint64Le());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long AsInt64Be(this ReadOnlySpan<byte> buffer)
            => unchecked((long)buffer.AsUint64Be());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AsUint64Le(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(ulong))
                throw GetInvalidBufferSizeException(sizeof(ulong), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return ((ulong)p[0] << 0) | ((ulong)p[1] << 8) | ((ulong)p[2] << 16) | ((ulong)p[3] << 24) | ((ulong)p[4] << 32) | ((ulong)p[5] << 40) | ((ulong)p[6] << 48) | ((ulong)p[7] << 56);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong AsUint64Be(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(ulong))
                throw GetInvalidBufferSizeException(sizeof(ulong), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return ((ulong)p[7] << 0) | ((ulong)p[6] << 8) | ((ulong)p[5] << 16) | ((ulong)p[4] << 24) | ((ulong)p[3] << 32) | ((ulong)p[2] << 40) | ((ulong)p[1] << 48) | ((ulong)p[0] << 56);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsSingleLe(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(float))
                throw GetInvalidBufferSizeException(sizeof(float), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    float value;
                    Copy4bytesLe(p, (byte*)&value);
                    return value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AsSingleBe(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(float))
                throw GetInvalidBufferSizeException(sizeof(float), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    float value;
                    Copy4bytesBe(p, (byte*)&value);
                    return value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AsDoubleLe(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(double))
                throw GetInvalidBufferSizeException(sizeof(double), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    double value;
                    Copy8bytesLe(p, (byte*)&value);
                    return value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double AsDoubleBe(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != sizeof(double))
                throw GetInvalidBufferSizeException(sizeof(double), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    double value;
                    Copy8bytesBe(p, (byte*)&value);
                    return value;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid AsGuid(this ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length != 16)
                throw GetInvalidBufferSizeException(16, nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    return
                        new Guid(
                            ((uint)p[0] << 0) | ((uint)p[1] << 8) | ((uint)p[2] << 16) | ((uint)p[3] << 24),
                            (ushort)((p[4] << 0) | (p[5] << 8)),
                            (ushort)((p[6] << 0) | (p[7] << 8)),
                            p[8],
                            p[9],
                            p[10],
                            p[11],
                            p[12],
                            p[13],
                            p[14],
                            p[15]);
                }
            }
        }

        #endregion

        #region Conversion from each type to Span<byte>

        public static void StoreInt16Le(this Span<byte> buffer, short value)
            => buffer.StoreUint16Le(unchecked((ushort)value));

        public static void StoreInt16Be(this Span<byte> buffer, short value)
            => buffer.StoreUint16Be(unchecked((ushort)value));

        public static void StoreUint16Le(this Span<byte> buffer, ushort value)
        {
            if (buffer.Length != sizeof(ushort))
                throw GetInvalidBufferSizeException(sizeof(ushort), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[0] = (byte)(value >> 0);
                    p[1] = (byte)(value >> 8);
                }
            }
        }

        public static void StoreUint16Be(this Span<byte> buffer, ushort value)
        {
            if (buffer.Length != sizeof(ushort))
                throw GetInvalidBufferSizeException(sizeof(ushort), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[1] = (byte)(value >> 0);
                    p[0] = (byte)(value >> 8);
                }
            }
        }

        public static void StoreInt24Le(this Span<byte> buffer, int value)
            => buffer.StoreUint24Le(unchecked((uint)value & 0x00ffffffU));

        public static void StoreInt24Be(this Span<byte> buffer, int value)
            => buffer.StoreUint24Be(unchecked((uint)value & 0x00ffffffU));

        public static void StoreUint24Le(this Span<byte> buffer, uint value)
        {
            if (buffer.Length != 3)
                throw GetInvalidBufferSizeException(3, nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[0] = (byte)(value >> 0);
                    p[1] = (byte)(value >> 8);
                    p[2] = (byte)(value >> 16);
                }
            }
        }

        public static void StoreUint24Be(this Span<byte> buffer, uint value)
        {
            if (buffer.Length != 3)
                throw GetInvalidBufferSizeException(3, nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[2] = (byte)(value >> 0);
                    p[1] = (byte)(value >> 8);
                    p[0] = (byte)(value >> 16);
                }
            }
        }

        public static void StoreInt32Le(this Span<byte> buffer, int value)
            => buffer.StoreUint32Le(unchecked((uint)value));

        public static void StoreInt32Be(this Span<byte> buffer, int value)
            => buffer.StoreUint32Be(unchecked((uint)value));

        public static void StoreUint32Le(this Span<byte> buffer, uint value)
        {
            if (buffer.Length != sizeof(uint))
                throw GetInvalidBufferSizeException(sizeof(uint), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[0] = (byte)(value >> 0);
                    p[1] = (byte)(value >> 8);
                    p[2] = (byte)(value >> 16);
                    p[3] = (byte)(value >> 24);
                }
            }
        }

        public static void StoreUint32Be(this Span<byte> buffer, uint value)
        {
            if (buffer.Length != sizeof(uint))
                throw GetInvalidBufferSizeException(sizeof(uint), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[3] = (byte)(value >> 0);
                    p[2] = (byte)(value >> 8);
                    p[1] = (byte)(value >> 16);
                    p[0] = (byte)(value >> 24);
                }
            }
        }

        public static void StoreInt64Le(this Span<byte> buffer, long value)
            => buffer.StoreUint64Le(unchecked((ulong)value));

        public static void StoreInt64Be(this Span<byte> buffer, long value)
            => buffer.StoreUint64Be(unchecked((ulong)value));

        public static void StoreUint64Le(this Span<byte> buffer, ulong value)
        {
            if (buffer.Length != sizeof(ulong))
                throw GetInvalidBufferSizeException(sizeof(ulong), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[0] = (byte)(value >> 0);
                    p[1] = (byte)(value >> 8);
                    p[2] = (byte)(value >> 16);
                    p[3] = (byte)(value >> 24);
                    p[4] = (byte)(value >> 32);
                    p[5] = (byte)(value >> 40);
                    p[6] = (byte)(value >> 48);
                    p[7] = (byte)(value >> 56);
                }
            }
        }

        public static void StoreUint64Be(this Span<byte> buffer, ulong value)
        {
            if (buffer.Length != sizeof(ulong))
                throw GetInvalidBufferSizeException(sizeof(ulong), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    p[7] = (byte)(value >> 0);
                    p[6] = (byte)(value >> 8);
                    p[5] = (byte)(value >> 16);
                    p[4] = (byte)(value >> 24);
                    p[3] = (byte)(value >> 32);
                    p[2] = (byte)(value >> 40);
                    p[1] = (byte)(value >> 48);
                    p[0] = (byte)(value >> 56);
                }
            }
        }

        public static void StoreSingleLe(this Span<byte> buffer, float value)
        {
            if (buffer.Length != sizeof(float))
                throw GetInvalidBufferSizeException(sizeof(float), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    Copy4bytesLe((byte*)&value, p);
                }
            }
        }

        public static void StoreSingleBe(this Span<byte> buffer, float value)
        {
            if (buffer.Length != sizeof(float))
                throw GetInvalidBufferSizeException(sizeof(float), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    Copy4bytesBe((byte*)&value, p);
                }
            }
        }

        public static void StoreDoubleLe(this Span<byte> buffer, double value)
        {
            if (buffer.Length != sizeof(double))
                throw GetInvalidBufferSizeException(sizeof(double), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    Copy8bytesLe((byte*)&value, p);
                }
            }
        }

        public static void StoreDoubleBe(this Span<byte> buffer, double value)
        {
            if (buffer.Length != sizeof(double))
                throw GetInvalidBufferSizeException(sizeof(double), nameof(buffer));
            unsafe
            {
                fixed (byte* p = &buffer.InternalSourceArray[buffer.InternalOffset])
                {
                    Copy8bytesBe((byte*)&value, p);
                }
            }
        }

        public static void StoreGuid(this Span<byte> buffer, Guid value)
        {
            var bytes = value.ToByteArray();
            if (buffer.Length != bytes.Length)
                throw GetInvalidBufferSizeException(bytes.Length, nameof(buffer));
            bytes.CopyTo(buffer);
        }

        #endregion

        #region private methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Copy2bytesLe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[0] = source[0];
                destination[1] = source[1];
            }
            else
            {
                destination[1] = source[0];
                destination[0] = source[1];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static unsafe void Copy2bytesBe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[1] = source[0];
                destination[0] = source[1];
            }
            else
            {
                destination[0] = source[0];
                destination[1] = source[1];
            }
        }

        internal static unsafe void Copy3bytesLe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[0] = source[0];
                destination[1] = source[1];
                destination[2] = source[2];
            }
            else
            {
                destination[2] = source[0];
                destination[1] = source[1];
                destination[0] = source[2];
            }
        }

        internal static unsafe void Copy3bytesBe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[2] = source[0];
                destination[1] = source[1];
                destination[0] = source[2];
            }
            else
            {
                destination[0] = source[0];
                destination[1] = source[1];
                destination[2] = source[2];
            }
        }

        internal static unsafe void Copy4bytesLe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[0] = source[0];
                destination[1] = source[1];
                destination[2] = source[2];
                destination[3] = source[3];
            }
            else
            {
                destination[3] = source[0];
                destination[2] = source[1];
                destination[1] = source[2];
                destination[0] = source[3];
            }
        }

        internal static unsafe void Copy4bytesBe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[3] = source[0];
                destination[2] = source[1];
                destination[1] = source[2];
                destination[0] = source[3];
            }
            else
            {
                destination[0] = source[0];
                destination[1] = source[1];
                destination[2] = source[2];
                destination[3] = source[3];
            }
        }

        internal static unsafe void Copy8bytesLe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[0] = source[0];
                destination[1] = source[1];
                destination[2] = source[2];
                destination[3] = source[3];
                destination[4] = source[4];
                destination[5] = source[5];
                destination[6] = source[6];
                destination[7] = source[7];
            }
            else
            {
                destination[7] = source[0];
                destination[6] = source[1];
                destination[5] = source[2];
                destination[4] = source[3];
                destination[3] = source[4];
                destination[2] = source[5];
                destination[1] = source[6];
                destination[0] = source[7];
            }
        }

        internal static unsafe void Copy8bytesBe(byte* source, byte* destination)
        {
            if (BitConverter.IsLittleEndian)
            {
                destination[7] = source[0];
                destination[6] = source[1];
                destination[5] = source[2];
                destination[4] = source[3];
                destination[3] = source[4];
                destination[2] = source[5];
                destination[1] = source[6];
                destination[0] = source[7];
            }
            else
            {
                destination[0] = source[0];
                destination[1] = source[1];
                destination[2] = source[2];
                destination[3] = source[3];
                destination[4] = source[4];
                destination[5] = source[5];
                destination[6] = source[6];
                destination[7] = source[7];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int FromUint24ToInt24(uint value)
            => (value & 0x00800000U) != 0
                ? unchecked((int)(value | 0xff000000U))
                : unchecked((int)(value & 0x00ffffffU));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Exception GetInvalidBufferSizeException(int sizeofType, string parameterName)
            => new ArgumentException($"The buffer length must be {sizeofType} bytes.", parameterName);

        #endregion
    }
}
