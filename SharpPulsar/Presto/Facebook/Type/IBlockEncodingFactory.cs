
namespace SharpPulsar.Presto.Facebook.Type
{
    /// <summary>
    /// From com.facebook.presto.spi.block.BlockEncodingFactory.java
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBlockEncodingFactory<T> where T : IBlockEncoding
    {
        /// <summary>
        /// Gets the unique name of this encoding.
        /// </summary>
        /// <returns></returns>
        string GetName();

        /// <summary>
        /// Reads the encoding from the specified input.
        /// 
        /// TODO: Input is SliceInput
        /// </summary>
        /// <returns></returns>
        T ReadEncoding(dynamic input);

        /// <summary>
        /// Writes this encoding to the output stream.
        /// TODO: output is SliceOutput
        /// </summary>
        /// <param name="serde"></param>
        /// <param name="output"></param>
        /// <param name="blockEncoding"></param>
        void WriteEncoding(IBlockEncodingSerde serde, dynamic output, T blockEncoding);
    }
}
