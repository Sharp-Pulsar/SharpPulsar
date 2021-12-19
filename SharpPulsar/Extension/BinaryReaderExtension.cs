using System.IO;

namespace SharpPulsar.Extension
{
    public static class BinaryReaderExtension
    {
        #region Strings
        /// <summary>
        /// Reads bytes until a \0 is reached, returns the string 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var sb = new System.Text.StringBuilder();
            char c;
            while ((c = (char)reader.ReadByte()) != 0)
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reads a whole buffer of bytes, builds one null terminated string contained within
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static string ReadStringBuffer(this BinaryReader reader, int bufferSize)
        {
            var buffer = reader.ReadChars(bufferSize);
            var length = 0;

            for (var i = 0; i != bufferSize; i++)
            {
                if (buffer[i] != 0) length++; else break;
            }

            return new string(buffer, 0, length);
        }
        #endregion

        #region Peeks
        /// <summary>
        /// Reads a byte without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static byte PeekByte(this BinaryReader reader)
        {
            var val = reader.PeekByte();
            reader.BaseStream.Position--;
            return val;
        }

        /// <summary>
        /// Reads a short without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static short PeekInt16(this BinaryReader reader)
        {
            var val = reader.ReadInt16();
            reader.BaseStream.Position -= 2;
            return val;
        }

        /// <summary>
        /// Reads an int without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static int PeekInt32(this BinaryReader reader)
        {
            var val = reader.ReadInt32();
            reader.BaseStream.Position -= 4;
            return val;
        }

        /// <summary>
        /// Reads a long without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static long PeekInt64(this BinaryReader reader)
        {
            var val = reader.ReadInt64();
            reader.BaseStream.Position -= 8;
            return val;
        }

        /// <summary>
        /// Reads a float without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static float PeekSingle(this BinaryReader reader)
        {
            var val = reader.ReadSingle();
            reader.BaseStream.Position -= 4;
            return val;
        }

        /// <summary>
        /// Reads a double without consuming the read
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static double PeekDouble(this BinaryReader reader)
        {
            var val = reader.ReadDouble();
            reader.BaseStream.Position -= 8;
            return val;
        }
        #endregion

        #region Skips
        /// <summary>
        /// Align the position of the buffer to the line, a line being 0x10 bytes.
        /// Will stay at the same position if already aligned, or next aligned offset if not.
        /// </summary>
        public static void AlignToLine(this BinaryReader reader)
        {
            var mod = reader.BaseStream.Position % 0x10;
            if (mod != 0)
            {
                reader.BaseStream.Position += 0x10 - mod;
            }
        }

        /// <summary>
        /// Skips a byte
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipByte(this BinaryReader reader)
        {
            reader.BaseStream.Position++;
        }

        /// <summary>
        /// Skips a byte, will log a warning if expected is given and the byte doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipByte(this BinaryReader reader, byte expected)
        {
            var b = reader.ReadByte();
            if (b != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: byte at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 1, b, expected, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips bytes
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipBytes(this BinaryReader reader, int count)
        {
            reader.BaseStream.Position += count;
        }

        /// <summary>
        /// Skips bytes, will log a warning if expected is given and one of the bytes doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipBytes(this BinaryReader reader, int count, byte expected)
        {
            for (var i = 0; i != count; i++)
            {
                var b = reader.ReadByte();
                if (b != expected)
                {
                    //Debug.LogWarning(string.Format("SKIP UNEXPECTED: byte at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 1, b, expected, reader.BaseStream.Length));
                }
            }
        }

        /// <summary>
        /// Skips a short
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt16(this BinaryReader reader)
        {
            reader.BaseStream.Position += 2;
        }

        /// <summary>
        /// Skips a short, will log a warning if excpected is given and the short doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt16(this BinaryReader reader, short expected)
        {
            var s = reader.ReadInt16();
            if (s != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: int16 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 2, s, expected, reader.BaseStream.Length));
            }

        }

        /// <summary>
        /// Skips an unsigned short
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt16(this BinaryReader reader)
        {
            reader.BaseStream.Position += 2;
        }

        /// <summary>
        /// Skips an unsigned short, will log a warning if excpected is given and the short doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt16(this BinaryReader reader, ushort expected)
        {
            var s = reader.ReadUInt16();
            if (s != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: int16 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 2, s, expected, reader.BaseStream.Length));
            }

        }

        /// <summary>
        /// Skips a int
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt32(this BinaryReader reader)
        {
            reader.BaseStream.Position += 4;
        }

        /// <summary>
        /// Skips an int, will log a warning if excpected is given and the int doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt32(this BinaryReader reader, int expected)
        {
            var i = reader.ReadInt32();
            if (i != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: int32 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, i, expected, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips an unsigned int
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt32(this BinaryReader reader)
        {
            reader.BaseStream.Position += 4;
        }

        /// <summary>
        /// Skips an unsigned int, will log a warning if excpected is given and the int doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt32(this BinaryReader reader, uint expected)
        {
            var i = reader.ReadUInt32();
            if (i != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: int32 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, i, expected, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips a long
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt64(this BinaryReader reader)
        {
            reader.BaseStream.Position += 8;
        }

        /// <summary>
        /// Skips a long, will log a warning if excpected is given and the long doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipInt64(this BinaryReader reader, long expected)
        {
            var l = reader.ReadInt64();
            if (l != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: int64 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 8, l, expected, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips an unsigned long
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt64(this BinaryReader reader)
        {
            reader.BaseStream.Position += 8;
        }

        /// <summary>
        /// Skips an unsigned long, will log a warning if excpected is given and the long doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipUInt64(this BinaryReader reader, ulong expected)
        {
            var l = reader.ReadUInt64();
            if (l != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: int64 at offset {0:X} was {1:X} ({1}), expected {2:X} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 8, l, expected, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips a float
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipSingle(this BinaryReader reader)
        {
            reader.BaseStream.Position += 4;
        }

        /// <summary>
        /// Skips a float, will log a warning if excpected is given and the float doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipSingle(this BinaryReader reader, float expected)
        {
            var f = reader.ReadSingle();
            if (f != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: single at offset {0:X} was {1} ({1}), expected {2} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 4, f, expected, reader.BaseStream.Length));
            }
        }

        /// <summary>
        /// Skips a double
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipDouble(this BinaryReader reader)
        {
            reader.BaseStream.Position += 8;
        }

        /// <summary>
        /// Skips a double, will log a warning if excpected is given and the float doesn't match it
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="expected"></param>
        public static void SkipDouble(this BinaryReader reader, double expected)
        {
            var d = reader.ReadDouble();
            if (d != expected)
            {
                //Debug.LogWarning(string.Format("SKIP UNEXPECTED: single at offset {0:X} was {1} ({1}), expected {2} ({2}). (Stream Length {3:X})", reader.BaseStream.Position - 8, d, expected, reader.BaseStream.Length));
            }
        }
        #endregion
    }
}
