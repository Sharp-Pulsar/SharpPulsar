using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using SharpPulsar.Sql.Precondition;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From io.airlift.slice.Slice.java
    /// 
    /// TODO: Finish implementing
    /// </summary>
    public sealed class Slice : IComparable<Slice>
    {
        #region Private Fields

        private static readonly int InstanceSize = 0;

        private static readonly byte[] Compact = new byte[0];

        private static readonly object NotCompact = null;

        // sun.misc.Unsafe.ARRAY_BYTE_BASE_OFFSET;
        private static readonly int ArrayByteBaseOffset = 0;

        #endregion

        #region Public Properties

        /// <summary>
        /// Base object for relative addresses.  If null, the address is an
        /// absolute location in memory.
        /// </summary>
        public object Base { get; }

        /// <summary>
        /// If base is null, address is the absolute memory location of data for
        /// this slice; otherwise, address is the offset from the base object.
        /// This base plus relative offset addressing is taken directly from
        /// the Unsafe interface.
        /// Note: if base object is a byte array, this address ARRAY_BYTE_BASE_OFFSET,
        /// since the byte array data starts AFTER the byte array object header.
        /// </summary>
        public long Address { get; }

        public int Size { get; }

        public int Length => Size;

        /// <summary>
        /// Bytes retained by the slice
        /// </summary>
        public long RetainedSize { get; }

        /// <summary>
        /// Reference has two use cases:
        /// 1. It can be an object this slice must hold onto to assure that the
        /// underlying memory is not freed by the garbage collector.
        /// It is typically a ByteBuffer object, but can be any object.
        /// This is not needed for arrays, since the array is referenced by {@code base}.
        /// 2. If reference is not used to prevent garbage collector from freeing the
        /// underlying memory, it will be used to indicate if the slice is compact.
        /// When
        /// { @code reference == COMPACT }, the slice is considered as compact.
        /// Otherwise, it will be null.
        /// A slice is considered compact if the base object is an heap array and
        /// it contains the whole array.
        /// Thus, for the first use case, the slice is always considered as not compact.
        /// </summary>
        public object Reference { get; }

        #endregion

        #region Constructors

        public Slice()
        {
            Base = null;
            Address = 0;
            Size = 0;
            RetainedSize = InstanceSize;
            Reference = Compact;
        }

        /// <summary>
        /// Creates a slice over the specified array.
        /// </summary>
        /// <param name="base"></param>
        public Slice(byte[] @base)
        {
            Base = @base ?? throw new ArgumentNullException("base");
            Address = ArrayByteBaseOffset;
            Size = @base.Length;
            RetainedSize = InstanceSize + Marshal.SizeOf(@base);
            Reference = Compact;
        }

        /// <summary>
        /// Creates a slice over the specified array range.
        /// </summary>
        /// <param name="base"></param>
        /// <param name="offset">The array position at which the slice begins</param>
        /// <param name="length">The number of array positions to include in the slice</param>
        public Slice(byte[] @base, int offset, int length)
        {
            ParameterCondition.OutOfRange(offset < @base.Length && offset + length < @base.Length, "offset");

            Base = @base ?? throw new ArgumentNullException("base");
            Address = ArrayByteBaseOffset + offset;
            Size = length;
            RetainedSize = InstanceSize + Marshal.SizeOf(@base);
            Reference = (offset == 0 && length == @base.Length) ? Compact : NotCompact;
        }
        /// <summary>
        /// Gets a byte at the specified absolute {@code index} in this buffer.
        /// </summary>
        /// <exception cref="IndexOutOfBoundsException"> if the specified {@code index} is less than {@code 0} or
        /// {@code index + 1} is greater than {@code this.length()} </exception>
        public sbyte GetByte(int index)
        {
            CheckIndexLength(index, sizeof(sbyte));
            return GetByteUnchecked(index);
        }
        public void SetByte(int index, int value)
        {
            CheckIndexLength(index, sizeof(byte));
            SetByteUnchecked(index, value);
        }
        public long GetLongUnchecked(int index)
        {
            return unchecked(Address + index);
        }
        public int GetIntUnchecked(int index)
        {
            return unchecked((int)Address + index);
        }
        public void SetIntUnchecked(int index, int value)
        {
            @unsafe.putInt(Base, Address + index, value);
        }
        public Slice ToSlice(int index, int length)
        {
            if ((index == 0) && (length == Length))
            {
                return this;
            }
            CheckIndexLength(index, length);
            if (length == 0)
            {
                return Slices.EmptySlice;
            }

            if (Reference == Compact)
            {
                return new Slice(Base, Address + index, length, RetainedSize, NotCompact);
            }
            return new Slice(Base, Address + index, length, RetainedSize, Reference);
        }
        private void CheckIndexLength(int index, int length)
        {
            ParameterCondition.CheckPositionIndexes(index, index + length, Length);
        }
        /// <summary>
        /// Creates a slice for directly accessing the base object.
        /// </summary>
        public Slice(object @base, long address, int size, long retainedSize, object reference)
        {
            if (address <= 0)
            {
                throw new System.ArgumentException($"Invalid address: {address}");
            }
            if (size <= 0)
            {
                throw new System.ArgumentException($"Invalid size: {size}");
            }
            ParameterCondition.CheckArgument((address + size) >= size, "Address + size is greater than 64 bits");

            this.Reference = reference;
            this.Base = @base;
            this.Address = address;
            this.Size = size;
            // INSTANCE_SIZE is not included, as the caller is responsible for including it.
            this.RetainedSize = retainedSize;
        }
        public sbyte GetByteUnchecked(int index)
        {
            return unchecked(Convert.ToSByte(Address + index));
        }
        #endregion

        #region Public Methods

        public int CompareTo(Slice other)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// A slice is considered compact if the base object is an array and it contains the whole array.
        /// As a result, it cannot be a view of a bigger slice.
        /// </summary>
        /// <returns></returns>
        public bool IsCompact()
        {
            return Reference == Compact;
        }

        public void SetBytes(int index, Slice source)
        {
            SetBytes(index, source, 0, source.Length);
        }

        public void SetBytes(int index, Slice source, int sourceIndex, int length)
        {
            ParameterCondition.CheckPositionIndexes(sourceIndex, sourceIndex + length, source.Length);

            throw new NotImplementedException();
            //CopyMemory(source.base, source.Address + sourceIndex, base, this.Address + index, length);
        }

        public void SetBytes(int index, byte[] source)
        {
            SetBytes(index, source, 0, source.Length);
        }

        public void SetBytes(int index, byte[] source, int sourceIndex, int length)
        {
            ParameterCondition.CheckPositionIndexes(sourceIndex, sourceIndex + length, source.Length);

            throw new NotImplementedException();
            //CopyMemory(source, (long)ARRAY_BYTE_BASE_OFFSET + sourceIndex, base, this.Address + index, length);
        }

        #endregion

        #region Private Methods

        private static void CopyMemory(object src, long srcAddress, object dest, long destAddress, int length)
        {
            // The Unsafe Javadoc specifies that the transfer size is 8 iff length % 8 == 0
            // so ensure that we copy big chunks whenever possible, even at the expense of two separate copy operations
            int bytesToCopy = length - (length % 8);

            // This is a bad workaround for a real implementation using unsafe code.
            dest = src;
        }

        #endregion
    }
}
