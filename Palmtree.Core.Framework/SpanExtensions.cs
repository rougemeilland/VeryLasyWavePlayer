using System;

namespace Palmtree
{
    public static class SpanExtensions
    {
        public static Span<ELEMENT_T> AsSpan<ELEMENT_T>(this ELEMENT_T[] array, int offset = 0)
            => offset >= 0 && offset <= array.Length
                ? new Span<ELEMENT_T>(array, offset, array.Length)
                : throw new ArgumentOutOfRangeException(nameof(offset));

        public static Span<ELEMENT_T> AsSpan<ELEMENT_T>(this ELEMENT_T[] array, int offset, int count)
            => offset < 0 || offset > array.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0 || offset + count > array.Length
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : new Span<ELEMENT_T>(array, offset, count);

        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this Span<ELEMENT_T> span)
            => new ReadOnlySpan<ELEMENT_T>(span.InternalSourceArray, span.InternalOffset, span.Length);

        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] array, int offset = 0)
            => offset < 0 || offset > array.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : new ReadOnlySpan<ELEMENT_T>(array, offset, array.Length);

        public static ReadOnlySpan<ELEMENT_T> AsReadOnlySpan<ELEMENT_T>(this ELEMENT_T[] array, int offset, int count)
            => offset < 0 || offset > array.Length
                ? throw new ArgumentOutOfRangeException(nameof(offset))
                : count < 0 || offset + count > array.Length
                ? throw new ArgumentOutOfRangeException(nameof(count))
                : new ReadOnlySpan<ELEMENT_T>(array, offset, count);

        public static void CopyTo<ELEMENT_T>(this ELEMENT_T[] source, Span<ELEMENT_T> destination)
        {
            if (destination.Length < source.Length)
                throw new ArgumentException($"Insufficient copy destination space.: {nameof(source)}.{nameof(source.Length)}={source.Length}, {nameof(destination)}.{nameof(destination.Length)}={destination.Length}", nameof(destination));
            Array.Copy(source, 0, destination.InternalSourceArray, destination.InternalOffset, source.Length);
        }
    }
}
