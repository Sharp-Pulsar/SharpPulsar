using System.IO;
using System.Text;
using Google.Protobuf;

namespace SharpPulsar.Akka.Network
{
    internal static class ProtoDeserialization
    {
        public static T ToObject<T>(this byte[] buf) where T : IMessage<T>, new()
        {
            if (buf == null)
                return default(T);
            var str = Encoding.UTF8.GetString(buf);
            using MemoryStream ms = new MemoryStream();
            ms.Write(buf, 0, buf.Length);
            ms.Seek(0, SeekOrigin.Begin);

            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(ms);
        }
    }
}
