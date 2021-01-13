
namespace SharpPulsar.Messages
{    
    public sealed class ReleaseConnectionPool
    {
        public ClientCnx ClientCnx { get; }
        public ReleaseConnectionPool(ClientCnx cnx)
        {
            ClientCnx = cnx;
        }
    }
}
