
namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.BlockEncodingSerde.java
    /// </summary>
    public interface IBlockEncodingSerde
    {
        /// <summary>
        /// Read a block encoding from the input.
        /// 
        /// TODO: input is SliceInput
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        IBlockEncoding ReadBlockEncoding(dynamic input);

        /// <summary>
        /// Write a blockEncoding to the output.
        /// 
        /// TODO: output is SliceOutput
        /// </summary>
        /// <param name="output"></param>
        /// <param name="encoding"></param>
        void WriteBlockEncoding(dynamic output, IBlockEncoding encoding);
    }
}
