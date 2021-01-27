using SharpPulsar.SocketImpl;
using System.Threading.Tasks;

namespace SharpPulsar.Extension
{
    public static class SocketClientEx
    {
        public static Task Execute(this SocketClient client, byte[] cmd)
        {
            return Task.Run(() => client.SendMessageAsync(cmd));
        }
    }
}
