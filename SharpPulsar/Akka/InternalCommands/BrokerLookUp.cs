
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.InternalCommands
{
    public class BrokerLookUp
    {
        public BrokerLookUp(string message, bool authoritative, CommandLookupTopicResponse.LookupType response, string brokerServiceUrl, string brokerServiceUrlTls, long requestId, bool useProxy)
        {
            Message = message;
            Authoritative = authoritative;
            Response = response;
            BrokerServiceUrl = brokerServiceUrl;
            BrokerServiceUrlTls = brokerServiceUrlTls;
            RequestId = requestId;
            UseProxy = useProxy;
        }
        public long RequestId { get; }
        public string Message { get; }
        public bool Authoritative { get; }
        public bool UseProxy { get; }
        public CommandLookupTopicResponse.LookupType Response { get; }
        public string BrokerServiceUrl { get; }
        public string BrokerServiceUrlTls { get; }
    }
}
