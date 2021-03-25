using Akka.Actor;
using System.Net;

namespace SharpPulsar.Messages.Requests
{
    public sealed class GetConnection
    {
        public string Topic { get; }
        public DnsEndPoint LogicalEndPoint { get; }
        public DnsEndPoint PhusicalEndPoint { get; }
        public IActorRef Sender { get; }
        public GetConnection(DnsEndPoint logicalEndPoint)
        {
            LogicalEndPoint = logicalEndPoint;
        }
        public GetConnection(DnsEndPoint logicalEndPoint, DnsEndPoint physicalEndpoint)
        {
            LogicalEndPoint = logicalEndPoint;
            PhusicalEndPoint = physicalEndpoint;
        }
        public GetConnection(string topic)
        {
            Topic = topic;
        }
    }
    public sealed class GetConnectionResponse
    {
        public IActorRef ClientCnx { get; }
        public GetConnectionResponse(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
    public sealed class ReleaseConnection
    {
        public IActorRef ClientCnx { get; }
        public ReleaseConnection(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
    public sealed class CleanupConnection
    {
        public DnsEndPoint Address { get; }
        public int ConnectionKey { get; }
        public CleanupConnection(DnsEndPoint address, int connectionKey)
        {
            Address = address;
            ConnectionKey = connectionKey;
        }
    }
    public sealed class GetPoolSizeResponse
    {
        public int PoolSize { get; }
        public GetPoolSizeResponse(int pool)
        {
            PoolSize = pool;
        }
    }
    public sealed class GetPoolSize
    {
        public static GetPoolSize Instance = new GetPoolSize();
    }
}
