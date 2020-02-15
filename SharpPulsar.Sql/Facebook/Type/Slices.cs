using System;
using SharpPulsar.Sql.Precondition;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From io.airlift.slice.Slices.java
    /// </summary>
    public class Slices
    {
        #region Public Fields

        public static readonly Slice EmptySlice = new Slice();

        #endregion

        #region Private Fields

        private static readonly int MaxArraySize = Int32.MaxValue - 8;

        private static readonly int SliceAllocThreshold = 524288; // 2^19

        private static readonly double SliceAllowSkew = 1.25; // Must be > 1

        #endregion

        #region Constructors

        private Slices()
        { }

        #endregion

        #region Public Static Methods
        public static Slice EnsureSize(Slice existingSlice, int minWritableBytes)
        {
            if (existingSlice == null)
            {
                return Allocate(minWritableBytes);
            }

            if (minWritableBytes <= existingSlice.Length)
            {
                return existingSlice;
            }

            int newCapacity;
            if (existingSlice.Length == 0)
            {
                newCapacity = 1;
            }
            else
            {
                newCapacity = existingSlice.Length;
            }
            while (newCapacity < minWritableBytes)
            {
                if (newCapacity < SliceAllocThreshold)
                {
                    newCapacity <<= 1;
                }
                else
                {
                    newCapacity *= (int)SliceAllowSkew; // double to int cast is saturating
                    if (newCapacity > MaxArraySize && minWritableBytes <= MaxArraySize)
                    {
                        newCapacity = MaxArraySize;
                    }
                }
            }

            Slice newSlice = Allocate(newCapacity);
            newSlice.SetBytes(0, existingSlice, 0, existingSlice.Length);
            return newSlice;
        }
        public static Slice Allocate(int capacity)
        {
            if (capacity == 0)
            {
                return EmptySlice;
            }

            ParameterCondition.Check(capacity <= MaxArraySize, $"Cannot allocate slice largert than {MaxArraySize} bytes.");

            return new Slice(new byte[capacity]);
        }

        public static Slice CopyOf(Slice slice, int offset, int length)
        {
            ParameterCondition.CheckPositionIndexes(offset, offset + length, slice.Length);

            Slice copy = Slices.Allocate(length);
            copy.SetBytes(0, slice, offset, length);

            return copy;
        }

        #endregion
    }
}
