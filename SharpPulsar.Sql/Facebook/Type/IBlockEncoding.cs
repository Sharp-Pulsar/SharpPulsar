using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Sql.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.BlockEncoding.java
    /// </summary>
    public interface IBlockEncoding
    {
        /// <summary>
        /// Gets the unique name of this encoding.
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// Read a block from the specified input.  The returned
        /// block should begin at the specified position.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IBlock ReadBlock(SliceInput input);

        /// <summary>
        /// Write the specified block to the specified output
        /// </summary>
        /// <param name="sliceOutput"></param>
        /// <param name="block"></param>
        void WriteBlock(SliceOutput sliceOutput, IBlock block);

        /// <summary>
        /// Return associated factory
        /// </summary>
        /// <returns></returns>
        IBlockEncodingFactory<T> GetFactory<T>() where T : IBlockEncoding;
    }
}
