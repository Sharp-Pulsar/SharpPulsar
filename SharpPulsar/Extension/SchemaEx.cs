using System;
using System.Net;

namespace SharpPulsar.Extension
{
    public static class SchemaEx
    {
        private static DateTime UTCEPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static long ConvertToMsTimestamp(this DateTime dateTime)
        {
            var elapsed = dateTime - UTCEPOCH;
            var ms = (long)elapsed.TotalMilliseconds;
            return ms;
        }
        public static DateTime ConvertToDateTime(this long msTimestamp)
        {
            var dt = UTCEPOCH.AddMilliseconds(msTimestamp);
            return dt;
        }
        public static long LongToBigEndian(this long num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
        public static int IntToBigEndian(this int num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
        public static short Int16ToBigEndian(this short num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
        public static long LongFromBigEndian(this long num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
        public static int IntFromBigEndian(this int num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
        public static short Int16FromBigEndian(this short num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
        public static byte[] ToByteArray(object[] tempObjectArray)
        {
            byte[] byteArray = new byte[tempObjectArray.Length];
            for (int index = 0; index < tempObjectArray.Length; index++)
                byteArray[index] = (byte)tempObjectArray[index];
            return byteArray;
        }
    }
}
