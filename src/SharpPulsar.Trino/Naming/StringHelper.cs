﻿//-------------------------------------------------------------------------------------------

//
//	This class is used to convert some aspects of the Java String class.
//-------------------------------------------------------------------------------------------

namespace SharpPulsar.Trino.Naming
{
    public static class StringHelper
    {
        //----------------------------------------------------------------------------------
        //	This method replaces the Java String.substring method when 'start' is a
        //	method call or calculated value to ensure that 'start' is obtained just once.
        //----------------------------------------------------------------------------------
        public static string SubstringSpecial(this string self, int start, int end)
        {
            return self.Substring(start, end - start);
        }

        //------------------------------------------------------------------------------------
        //	This method is used to replace calls to the 2-arg Java String.startsWith method.
        //------------------------------------------------------------------------------------
        public static bool StartsWith(this string self, string prefix, int toffset)
        {
            return self.IndexOf(prefix, toffset, StringComparison.Ordinal) == toffset;
        }

        //------------------------------------------------------------------------------
        //	This method is used to replace most calls to the Java String.split method.
        //------------------------------------------------------------------------------
        public static string[] Split(this string self, string regexDelimiter, bool trimTrailingEmptyStrings)
        {
            var splitArray = System.Text.RegularExpressions.Regex.Split(self, regexDelimiter);

            if (trimTrailingEmptyStrings)
            {
                if (splitArray.Length > 1)
                {
                    for (var i = splitArray.Length; i > 0; i--)
                    {
                        if (splitArray[i - 1].Length > 0)
                        {
                            if (i < splitArray.Length)
                                Array.Resize(ref splitArray, i);

                            break;
                        }
                    }
                }
            }

            return splitArray;
        }

        //-----------------------------------------------------------------------------
        //	These methods are used to replace calls to some Java String constructors.
        //-----------------------------------------------------------------------------
        public static string NewString(byte[] bytes)
        {
            return NewString(bytes, 0, bytes.Length);
        }
        public static string NewString(byte[] bytes, int index, int count)
        {
            return System.Text.Encoding.UTF8.GetString(bytes, index, count);
        }
        public static string NewString(byte[] bytes, string encoding)
        {
            return NewString(bytes, 0, bytes.Length, encoding);
        }
        public static string NewString(byte[] bytes, int index, int count, string encoding)
        {
            return System.Text.Encoding.GetEncoding(encoding).GetString(bytes, index, count);
        }

        //--------------------------------------------------------------------------------
        //	These methods are used to replace calls to the Java String.getBytes methods.
        //--------------------------------------------------------------------------------
        public static byte[] GetBytes(this string self)
        {
            return GetSBytesForEncoding(System.Text.Encoding.UTF8, self);
        }
        public static byte[] GetBytes(this string self, System.Text.Encoding encoding)
        {
            return GetSBytesForEncoding(encoding, self);
        }
        public static byte[] GetBytes(this string self, string encoding)
        {
            return GetSBytesForEncoding(System.Text.Encoding.GetEncoding(encoding), self);
        }
        private static byte[] GetSBytesForEncoding(System.Text.Encoding encoding, string s)
        {
            var bytes = new byte[encoding.GetByteCount(s)];
            encoding.GetBytes(s, 0, s.Length, bytes, 0);
            return bytes;
        }

    }
}