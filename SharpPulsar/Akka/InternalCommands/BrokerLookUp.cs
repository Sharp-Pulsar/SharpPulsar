
using SharpPulsar.Protocol.Proto;

namespace SharpPulsar.Akka.InternalCommands
{
    public class BrokerLookUp
    {
        public BrokerLookUp(string message, bool authoritative, CommandLookupTopicResponse.Types.LookupType response, string brokerServiceUrl, string brokerServiceUrlTls, long requestId)
        {
            Message = message;
            Authoritative = authoritative;
            Response = response;
            BrokerServiceUrl = brokerServiceUrl;
            BrokerServiceUrlTls = brokerServiceUrlTls;
            RequestId = requestId;
        }
        public long RequestId { get; }
        public string Message { get; }
        public bool Authoritative { get; }
        public CommandLookupTopicResponse.Types.LookupType Response { get; }
        public string BrokerServiceUrl { get; }
        public string BrokerServiceUrlTls { get; }
    }
}
