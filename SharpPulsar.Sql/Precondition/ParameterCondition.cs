using System;
using System.Collections.Generic;
using System.Text;

namespace SharpPulsar.Sql.Precondition
{
    public static class ParameterCondition
    {
        public static T RequireNonNull<T>(T value, string parameterName, string message = "") where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName, message);
            }

            return value;
        }
        public static void CheckArgument(bool check, string message, params object[] args)
        {
            if (!check)
            {
                throw new ArgumentException(string.Format(message, args));
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
    }
}
