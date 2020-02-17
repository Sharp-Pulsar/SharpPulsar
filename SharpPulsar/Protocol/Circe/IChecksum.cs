
namespace SharpPulsar.Protocol.Circe
{
    public interface IChecksum
    {
        /// <summary>
        /// Returns the current checksum
        /// </summary>
        /// <returns>long</returns>
        public long GetValue();
        /// <summary>
        /// Resets the checksum to its initial value
        /// </summary>
        public void Reset();

        /// <summary>
        /// Updates the current checksum with the specified array of bytes
        /// </summary>
        /// <param name="b"></param>
        /// <param name="off"></param>
        /// <param name="len"></param>
        public void Update(sbyte[] b, int off, int len);
        /// <summary>
        /// Updates the current checksum with the specified byte
        /// </summary>
        /// <param name="b"></param>
        public void Update(int b);

    }
}