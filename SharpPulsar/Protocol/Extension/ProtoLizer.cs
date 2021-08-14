using System.IO;
using System.Net;
using ProtoBuf;

namespace SharpPulsar.Protocol.Extension
{
    public static class ProtoLizer
    {
        public static byte[] ToByteArrays<T>(this T obj)
        {
            using var memStream = new MemoryStream();
            Serializer.Serialize(memStream, obj); 
            return memStream.ToArray();
        }
        public static int ByteLength<T>(this T obj)
        {
            using var memStream = new MemoryStream();
            Serializer.Serialize(memStream, obj);
            return memStream.ToArray().Length;
        }
        public static T FromByteArray<T>(this byte[] protoBytes)
        {
            using var ms = new MemoryStream();
            ms.Write(protoBytes, 0, protoBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            return Serializer.Deserialize<T>(ms);
        }
        public static short MagicNumber => 0x0e01;
        public static int Int32ToBigEndian(int num) => IPAddress.HostToNetworkOrder(num);

        public static int Int32FromBigEndian(int num) => IPAddress.NetworkToHostOrder(num);

        private static int Int16FromBigEndian(short num) => IPAddress.NetworkToHostOrder(num);
    }
}
