using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.BlockBuilder.java
    /// </summary>
    public interface IBlockBuilder : IBlock
    {
        /// <summary>
        /// Write a byte to the current entry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IBlockBuilder WriteByte(int value);

        /// <summary>
        /// Write a short to the current entry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IBlockBuilder WriteShort(int value);

        /// <summary>
        ///  Write a int to the current entry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IBlockBuilder WriteInt(int value);

        /// <summary>
        /// Write a long to the current entry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IBlockBuilder WriteLong(long value);

        /// <summary>
        /// Write a byte sequences to the current entry
        /// </summary>
        /// <param name="source"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IBlockBuilder WriteBytes(Slice source, int sourceIndex, int length);

        /// <summary>
        /// Write an object to the current entry
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IBlockBuilder WriteObject(object value);

        /// <summary>
        /// Return a writer to the current entry. The caller can operate on the returned caller to 
        /// incrementally build the object. This is generally more efficient than building the object 
        /// elsewhere and call writeObject afterwards because a large chunk of memory could potentially 
        /// be unnecessarily copied in this process.
        /// </summary>
        /// <returns></returns>
        IBlockBuilder BeginBlockEntry();

        /// <summary>
        /// Create a new block from the current materialized block by keeping the same elements
        /// only with respect to {@code visiblePositions}.
        /// </summary>
        /// <param name="visiblePositions"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        IBlockBuilder GetPositions(int[] visiblePositions, int offset, int length);

        /// <summary>
        /// Write a byte to the current entry
        /// </summary>
        /// <returns></returns>
        IBlockBuilder CloseEntry();

        /// <summary>
        /// Appends a null value to the block.
        /// </summary>
        /// <returns></returns>
        IBlockBuilder AppendNull();

        /// <summary>
        /// Builds the block. This method can be called multiple times.
        /// </summary>
        /// <returns></returns>
        IBlock Build();

        /// <summary>
        /// Creates a new block builder of the same type based on the current usage statistics of this block builder.
        /// </summary>
        /// <returns></returns>
        IBlockBuilder NewBlockBuilderLike(BlockBuilderStatus blockBuilderStatus);
    }
}
