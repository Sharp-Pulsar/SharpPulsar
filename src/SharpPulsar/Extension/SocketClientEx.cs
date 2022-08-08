using SharpPulsar.SocketImpl;
using System.Buffers;
using System.Threading.Tasks;

namespace SharpPulsar.Extension
{
    public static class SocketClientEx
    {
        public static Task Execute(this SocketClient client, ReadOnlySequence<byte> cmd)
        {
            return Task.Run(() => client.SendMessage(cmd));
        }
    }
}
