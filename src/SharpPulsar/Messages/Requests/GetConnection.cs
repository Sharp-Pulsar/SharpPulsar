using Akka.Actor;
using System.Net;

namespace SharpPulsar.Messages.Requests
{
    public readonly record struct GetConnection
    {
        public string Topic { get; }
        public DnsEndPoint LogicalEndPoint { get; }
        public DnsEndPoint PhusicalEndPoint { get; }
        public GetConnection(DnsEndPoint logicalEndPoint) => (Topic, LogicalEndPoint, PhusicalEndPoint) 
            = ("", logicalEndPoint, null);
        public GetConnection(DnsEndPoint logicalEndPoint, DnsEndPoint physicalEndpoint) 
            => (Topic, LogicalEndPoint, PhusicalEndPoint)
            = ("", logicalEndPoint, physicalEndpoint);
        public GetConnection(string topic)
            => (Topic, LogicalEndPoint, PhusicalEndPoint)
            = (topic, null, null);
    }
    public readonly record struct GetConnectionResponse
    {
        public IActorRef ClientCnx { get; }
        public GetConnectionResponse(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
    public readonly record struct ReleaseConnection
    {
        public IActorRef ClientCnx { get; }
        public ReleaseConnection(IActorRef cnx)
        {
            ClientCnx = cnx;
        }
    }
    public readonly record struct CleanupConnection
    {
        public DnsEndPoint Address { get; }
        public int ConnectionKey { get; }
        public CleanupConnection(DnsEndPoint address, int connectionKey)
        {
            Address = address;
            ConnectionKey = connectionKey;
        }
    }
    public readonly record struct GetPoolSizeResponse
    {
        public int PoolSize { get; }
        public GetPoolSizeResponse(int pool)
        {
            PoolSize = pool;
        }
    }
    public readonly record struct GetPoolSize
    {
        public static GetPoolSize Instance = new GetPoolSize();
    }
}
