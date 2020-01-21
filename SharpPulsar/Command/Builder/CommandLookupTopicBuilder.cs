using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandLookupTopicBuilder
    {
        private CommandLookupTopic _topic;
        public CommandLookupTopicBuilder()
        {
            _topic = new CommandLookupTopic();
        }
        
        public CommandLookupTopicBuilder SetAuthoritative(bool authoritative)
        {
            _topic.Authoritative = authoritative;
            return this;
        }
        public CommandLookupTopicBuilder SetOriginalAuthData(string originalAuthData)
        {
            _topic.OriginalAuthData = originalAuthData;
            return this;
        }
        public CommandLookupTopicBuilder SetOriginalAuthMethod(string originalAuthMethod)
        {
            _topic.OriginalAuthMethod = originalAuthMethod;
            return this;
        }
        public CommandLookupTopicBuilder SetOriginalPrincipal(string originalPrincipal)
        {
            _topic.OriginalPrincipal = originalPrincipal;
            return this;
        }
        public CommandLookupTopicBuilder SetRequestId(long requestId)
        {
            _topic.RequestId = (ulong)requestId;
            return this;
        }
        public CommandLookupTopicBuilder SetTopic(string topic)
        {
            _topic.Topic = topic;
            return this;
        }
        public CommandLookupTopic Build()
        {
            return _topic;
        }
    }
}
