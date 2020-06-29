using System;
using System.Text;

namespace SharpPulsar.Precondition
{
    public static class Condition
    {
        public static void NotNullOrEmpty(string value, string parameterName, string message = "")
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(parameterName, message);
            }
        }
        public static void OutOfRange(bool expression, string parameterName, string message = "")
        {
            if (expression == false)
            {
                throw new ArgumentOutOfRangeException(parameterName, message);
            }
        }

        public static void Check(bool expression, string message)
        {
            if (expression == false)
            {
                throw new ArgumentException(message);
            }
        }
        
        public static void CheckNoTNull(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException();
            }

        }
        public static void CheckArgument(bool check, string message, params object[] args)
        {
            if (!check)
            {
                throw new ArgumentException(string.Format(message, args));
            }
        }
        public static void CheckArgument(bool check)
        {
            if (!check)
            {
                throw new ArgumentException();
            }
        }
        public static bool CanEncode(string value)
        {
            return IsAscii(value);
        }
        private static bool IsAscii(this string value)
        {
            // ASCII encoding replaces non-ascii with question marks, so we use UTF8 to see if multi-byte sequences are there
            return Encoding.UTF8.GetByteCount(value) == value.Length;
        }
        public static void CheckPositionIndexes(int start, int end, int size)
        {
            // Carefully optimized for execution by hotspot (explanatory comment above)
            if (start < 0 || end < start || end > size)
            {
                throw new ArgumentOutOfRangeException(BadPositionIndexes(start, end, size));
            }
        }
        public static int CheckPositionIndex(int index, int size)
        {
            return CheckPositionIndex(index, size, "index");
        }

        public static int CheckPositionIndex(int index, int size, string desc)
        {
            // Carefully optimized for execution by hotspot (explanatory comment above)
            if (index < 0 || index > size)
            {
                throw new ArgumentOutOfRangeException(BadPositionIndex(index, size, desc));
            }

            return index;
        }
        private static string BadPositionIndex(int index, int size, string desc)
        {
            if (index < 0)
            {
                return $"{desc} ({index}) must not be negative.";
            }
            else if (size < 0)
            {
                throw new ArgumentOutOfRangeException("size", $"Negative Size: {size}.");
            }
            else
            {
                // index > Size
                return $"{desc} ({index}) must not be greater than Size ({size}).";
            }
        }

        private static string BadPositionIndexes(int start, int end, int size)
        {
            if (start < 0 || start > size)
            {
                return BadPositionIndex(start, size, "start index");
            }

            if (end < 0 || end > size)
            {
                return BadPositionIndex(end, size, "end index");
            }

            // end < start
            return $"End index ({end}) must not be less than start index ({start}).";
        }


    }
}
