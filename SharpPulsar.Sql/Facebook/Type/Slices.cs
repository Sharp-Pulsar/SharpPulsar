using System;
using System.Collections.Generic;
using System.Text;
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
