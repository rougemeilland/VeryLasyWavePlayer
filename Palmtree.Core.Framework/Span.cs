using System;
using System.Runtime.CompilerServices;

namespace Palmtree
{
    public readonly struct Span<ELEMENT_T>
    {
        public static readonly Span<ELEMENT_T> Empty = new Span<ELEMENT_T>(Array.Empty<ELEMENT_T>(), 0, 0);

        internal readonly ELEMENT_T[] InternalSourceArray;
        internal readonly int InternalOffset;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Span(ELEMENT_T[] array, int offset, int count)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));
            if (offset < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (offset + count > array.Length)
                throw new IndexOutOfRangeException($"The \"offset\" and \"count\" values point outside the bounds of the array.: {nameof(array)}.{nameof(array.Length)}={array.Length}, {nameof(offset)}={offset}, {nameof(count)}={count}");

            InternalSourceArray = array;
            InternalOffset = offset;
            Length = count;
        }

        public ELEMENT_T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException($"The value of \"index\" is out of range.: index={index}");
                return InternalSourceArray[InternalOffset + index];
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (index < 0 || index >= Length)
                    throw new IndexOutOfRangeException($"The value of \"index\" is out of range.: index={index}");
                InternalSourceArray[InternalOffset + index] = value;
            }
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
        public Span<ELEMENT_T> Slice(int offset)
        {
            if (offset < 0 || offset > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            return new Span<ELEMENT_T>(InternalSourceArray, InternalOffset + offset, Length - offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<ELEMENT_T> Slice(int offset, int count)
        {
            if (offset < 0 || offset > Length)
                throw new ArgumentOutOfRangeException(nameof(offset));
            if (count < 0 || offset + count > Length)
                throw new ArgumentOutOfRangeException(nameof(count));
            return new Span<ELEMENT_T>(InternalSourceArray, InternalOffset + offset, count);
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
        public static implicit operator ReadOnlySpan<ELEMENT_T>(Span<ELEMENT_T> span)
            => new ReadOnlySpan<ELEMENT_T>(span.InternalSourceArray, span.InternalOffset, span.Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Span<ELEMENT_T>(ELEMENT_T[] buffer)
            => new Span<ELEMENT_T>(buffer, 0, buffer.Length);
    }
}
