using System;
namespace SharpPulsar.Extension
{
    /// <summary>
    /// Provides extension methods to the <see cref="Endianness"/> enumeration.
    /// </summary>
    public static class EndiannessExtension
    {
        /// <summary>
        /// Gets the native endianness of the computer architecture where the code is executing.
        /// </summary>
        public static Endianness NativeEndianness => BitConverter.IsLittleEndian ? Endianness.LittleEndian : Endianness.BigEndian;

        /// <summary>
        /// Determines whether this endianness is 'big-endian'.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>true if this endianness is 'big-endian'; otherwise false.</returns>
        public static bool IsBigEndian(this Endianness endianness) => endianness == Endianness.BigEndian;

        /// <summary>
        /// Determines whether this endianness is 'little-endian'.
        /// </summary>
        /// <param name="endianness">The endianness.</param>
        /// <returns>true if this endianness is 'little-endian'; otherwise false.</returns>
        public static bool IsLittleEndian(this Endianness endianness) => endianness == Endianness.LittleEndian;
    }
}
