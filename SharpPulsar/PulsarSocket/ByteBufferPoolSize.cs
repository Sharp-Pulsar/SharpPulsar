using static Akka.IO.Inet;

namespace SharpPulsar.PulsarSocket
{
    public class ByteBufferPoolSize : SocketOption
    {
        public int ByteBufferPoolSizeBytes { get; }

        public ByteBufferPoolSize(int sizeBytes)
        {
            ByteBufferPoolSizeBytes = sizeBytes;
        }
    }
}
