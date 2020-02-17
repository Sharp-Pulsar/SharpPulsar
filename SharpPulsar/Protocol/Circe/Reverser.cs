using System;

namespace SharpPulsar.Protocol.Circe
{
    public static class Reverser
    {
        public static bool IsLittleEndianArch()
        {
            return BitConverter.IsLittleEndian;
        }

        public static bool IsBigEndianArch()
        {
            if (BitConverter.IsLittleEndian) return false;
            return true;
        }

        /// <summary>
        ///  reverse byte order (16-bit). This can be used to change between little-endian and big-endian.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt16 ReverseBytes(UInt16 value)
        {
            return (UInt16)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        /// <summary>
        /// reverse byte order (32-bit). This can be used to change between little-endian and big-endian.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// reverse byte order (64-bit). This can be used to change between little-endian and big-endian.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static UInt64 ReverseBytes(UInt64 value)
        {
            return (value & 0x00000000000000FFUL) << 56 |
                   (value & 0x000000000000FF00UL) << 40 |
                   (value & 0x0000000000FF0000UL) << 24 |
                   (value & 0x00000000FF000000UL) << 8 |
                   (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                   (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        public static Int16 ReverseBytes(Int16 value)
        {
            return (Int16)ReverseBytes((UInt16)value);
        }

        public static Int32 ReverseBytes(Int32 value)
        {
            return (Int32)ReverseBytes((UInt32)value);
        }

        public static Int64 ReverseBytes(Int64 value)
        {
            return (Int64)ReverseBytes((UInt64)value);
        }
    }
}
