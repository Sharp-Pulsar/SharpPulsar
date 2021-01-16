using System.Net;

namespace SharpPulsar.Messages.Requests
{
    public sealed class GetConnection
    {
        public string Topic { get; }
        public GetConnection(string topic)
        {
            Topic = topic;
        }
    }
    public sealed class GetConnectionForAddress
    {
        public string TargetBroker { get; }
        public DnsEndPoint EndPoint { get; }
        public GetConnectionForAddress(DnsEndPoint endPoint, string targetBroker)
        {
            EndPoint = endPoint;
            TargetBroker = targetBroker;
        }
    }
}
