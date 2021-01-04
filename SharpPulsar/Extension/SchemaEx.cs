using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SharpPulsar.Extension
{
    public static class SchemaEx
    {
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
        public static sbyte[] ToSBytes(this byte[] bytes)
        {
            return (sbyte[])(object)bytes;
        }
        public static byte[] ToBytes(this sbyte[] bytes)
        {
            return (byte[])(object)bytes;
        }
        public static short Int16FromBigEndian(this short num)
        {
            return IPAddress.HostToNetworkOrder(num);
        }
    }
}
