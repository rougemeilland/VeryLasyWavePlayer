using System;
using System.Runtime.CompilerServices;

namespace Palmtree
{
    public readonly struct ReadOnlySpan<ELEMENT_T>
    {
        public static readonly ReadOnlySpan<ELEMENT_T> Empty = new ReadOnlySpan<ELEMENT_T>(Array.Empty<ELEMENT_T>(), 0, 0);

        internal readonly ELEMENT_T[] InternalSourceArray;
        internal readonly int InternalOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ReadOnlySpan(ELEMENT_T[] array, int offset, int count)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0 || offset > array.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || checked(offset + count) > array.Length)
                throw new ArgumentOutOfRangeException(nameof(count));

            InternalSourceArray = array;
            InternalOffset = offset;
            Length = count;
        }

        public ELEMENT_T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
                => index >= 0 && index < Length
                    ? InternalSourceArray[checked(InternalOffset + index)]
                    : throw new IndexOutOfRangeException($"The value of \"index\" is out of range.: index={index}");
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        public bool IsEmpty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<ELEMENT_T> Slice(int offset)
        {
            if (offset < 0 || offset > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            return new ReadOnlySpan<ELEMENT_T>(InternalSourceArray, InternalOffset + offset, Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<ELEMENT_T> Slice(int offset, int count)
        {
            if (offset < 0 || offset > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            return new ReadOnlySpan<ELEMENT_T>(InternalSourceArray, InternalOffset + offset, count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CopyTo(Span<ELEMENT_T> destination)
        {
            if (destination.Length < Length)
                throw new ArgumentException($"Insufficient copy destination space.: this.{nameof(Length)}={Length}, {nameof(destination)}.{nameof(destination.Length)}={destination.Length}", nameof(destination));
            Array.Copy(InternalSourceArray, InternalOffset, destination.InternalSourceArray, destination.InternalOffset, Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ELEMENT_T[] ToArray()
        {
            var newArray = new ELEMENT_T[Length];
            Array.Copy(InternalSourceArray, InternalOffset, newArray, 0, Length);
            return newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<ELEMENT_T>(ELEMENT_T[] buffer)
            => new ReadOnlySpan<ELEMENT_T>(buffer, 0, buffer.Length);
    }
}
