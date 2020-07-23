namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.Block.java
    /// </summary>
    public interface IBlock
    {
        /// <summary>
        /// Gets the length of the value at the {@code position}.
        /// This method must be implemented if @{code getSlice} is implemented.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        int GetSliceLength(int position);

        /// <summary>
        /// Gets a byte at {@code offset} in the value at {@code position}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        byte GetByte(int position, int offset);

        /// <summary>
        /// Gets a little endian short at {@code offset} in the value at {@code position}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        short GetShort(int position, int offset);

        /// <summary>
        /// Gets a little endian int at {@code offset} in the value at {@code position}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        int GetInt(int position, int offset);

        /// <summary>
        /// Gets a little endian long at {@code offset} in the value at {@code position}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        long GetLong(int position, int offset);

        /// <summary>
        /// Gets a slice at {@code offset} in the value at {@code position}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        Slice GetSlice(int position, int offset, int length);

        /// <summary>
        /// Gets an object in the value at {@code position}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object GetObject(int position, System.Type type);

        /// <summary>
        /// Is the byte sequences at {@code offset} in the value at {@code position} equal
        /// to the byte sequence at {@code otherOffset} in {@code otherSlice}.
        /// This method must be implemented if @{code getSlice} is implemented.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <param name="otherSlice"></param>
        /// <param name="otherOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        bool BytesEqual(int position, int offset, Slice otherSlice, int otherOffset, int length);

        /// <summary>
        /// Compares the byte sequences at {@code offset} in the value at {@code position}
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="otherSlice"></param>
        /// <param name="otherOffset"></param>
        /// <param name="otherLength"></param>
        /// <returns></returns>
        int BytesCompare(int position, int offset, int length, Slice otherSlice, int otherOffset, int otherLength);

        /// <summary>
        /// Appends the byte sequences at {@code offset} in the value at {@code position}
        /// to {@code blockBuilder}.
        /// This method must be implemented if @{code getSlice} is implemented.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        void WriteBytesTo(int position, int offset, int length);

        /// <summary>
        /// Appends the value at {@code position} to {@code blockBuilder}.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="blockBuilder"></param>
        void WritePositionTo(int position, IBlockBuilder blockBuilder);

        /// <summary>
        /// Is the byte sequences at {@code offset} in the value at {@code position} equal
        /// to the byte sequence at {@code otherOffset} in the value at {@code otherPosition}
        /// in {@code otherBlock}.
        /// This method must be implemented if @{code getSlice} is implemented.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <param name="otherBlock"></param>
        /// <param name="otherPosition"></param>
        /// <param name="otherOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        bool Equals(int position, int offset, IBlock otherBlock, int otherPosition, int otherOffset, int length);

        /// <summary>
        /// Calculates the hash code the byte sequences at {@code offset} in the
        /// value at {@code position}.
        /// This method must be implemented if @{code getSlice} is implemented. 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        long Hash(int position, int offset, int length);

        /// <summary>
        /// Compares the byte sequences at {@code offset} in the value at {@code position}
        /// to the byte sequence at {@code otherOffset} in the value at {@code otherPosition}
        /// in {@code otherBlock}.
        /// This method must be implemented if @{code getSlice} is implemented.
        /// </summary>
        /// <param name="leftPosition"></param>
        /// <param name="leftOffset"></param>
        /// <param name="leftLength"></param>
        /// <param name="rightBlock"></param>
        /// <param name="rightPosition"></param>
        /// <param name="rightOffset"></param>
        /// <param name="rightLength"></param>
        /// <returns></returns>
        int CompareTo(int leftPosition, int leftOffset, int leftLength, IBlock rightBlock, int rightPosition, int rightOffset, int rightLength);

        /// <summary>
        /// Gets the value at the specified position as a single element block.  The method
        /// must copy the data into a new block.
        /// 
        /// This method is useful for operators that hold on to a single value without
        /// holding on to the entire block.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        IBlock GetSingleValueBlock(int position);

        /// <summary>
        /// Returns the number of positions in this block.
        /// </summary>
        /// <returns></returns>
        int GetPositionCount();

        /// <summary>
        /// Returns the logical size of this block in memory.
        /// </summary>
        /// <returns></returns>
        long GetSizeInBytes();

        /// <summary>
        /// Returns the logical size of {@code block.getRegion(position, length)} in memory.
        /// The method can be expensive. Do not use it outside an implementation of Block.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        long GetRegionSizeInBytes(int position, int length);

        /// <summary>
        /// Returns the retained size of this block in memory.
        /// This method is called from the inner most execution loop and must be fast.
        /// </summary>
        /// <returns></returns>
        long GetRetainedSizeInBytes();

        /// <summary>
        /// {@code consumer} visits each of the internal data container and accepts the size for it.
        /// This method can be helpful in cases such as memory counting for internal data structure.
        /// Also, the method should be non-recursive, only visit the elements at the top level,
        /// and specifically should not call retainedBytesForEachPart on nested blocks
        /// {@code consumer} should be called at least once with the current block and
        /// must include the instance size of the current block
        /// </summary>
        /// <param name="consumer"></param>
        void RetainedBytesForEachPart(BiConsumer<object, long> consumer);

        /// <summary>
        /// Get the encoding for this block.
        /// </summary>
        IBlockEncoding GetEncoding();

        /// <summary>
        /// Returns a block containing the specified positions.
        /// Positions to copy are stored in a subarray within {@code positions}
        /// array that starts at { @code offset }
        /// and has length of { @code length }.
        /// All specified positions must be valid for this block.
        /// The returned block must be a compact representation of the original block.
        /// </summary>
        /// <param name="positions"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IBlock CopyPositions(int[] positions, int offset, int length);

        /// <summary>
        /// Returns a block starting at the specified position and extends for the
        /// specified length.The specified region must be entirely contained
        /// within this block.
        /// The region can be a view over this block.If this block is released
        /// the region block may also be released.  If the region block is released
        /// this block may also be released.
        /// </summary>
        /// <param name="positionOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IBlock GetRegion(int positionOffset, int length);

        /// <summary>
        /// Returns a block starting at the specified position and extends for the
        /// specified length.The specified region must be entirely contained
        /// within this block.
        /// The region returned must be a compact representation of the original block, unless their internal
        /// representation will be exactly the same.This method is useful for
        /// operators that hold on to a range of values without holding on to the
        /// entire block.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IBlock CopyRegion(int position, int length);

        /// <summary>
        /// Is the specified position null?
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        bool IsNull(int position);

        /// <summary>
        /// Assures that all data for the block is in memory.
        /// This allows streaming data sources to skip sections that are not
        /// accessed in a query.
        /// </summary>
        void AssureLoaded();
    }
}
