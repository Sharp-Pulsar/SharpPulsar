
namespace SharpPulsar.Messages
{
    public sealed class ConnectionClosed
    {
        public ClientCnx ClientCnx { get; }
        public ConnectionClosed(ClientCnx clientCnx)
        {
            ClientCnx = clientCnx;
        }
    }
}
