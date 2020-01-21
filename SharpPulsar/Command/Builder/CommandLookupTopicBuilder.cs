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
        private CommandLookupTopicBuilder(CommandLookupTopic topic)
        {
            _topic = topic;
        }
        public CommandLookupTopicBuilder SetAuthoritative(bool authoritative)
        {
            _topic.Authoritative = authoritative;
            return new CommandLookupTopicBuilder(_topic);
        }
        public CommandLookupTopicBuilder SetOriginalAuthData(string originalAuthData)
        {
            _topic.OriginalAuthData = originalAuthData;
            return new CommandLookupTopicBuilder(_topic);
        }
        public CommandLookupTopicBuilder SetOriginalAuthMethod(string originalAuthMethod)
        {
            _topic.OriginalAuthMethod = originalAuthMethod;
            return new CommandLookupTopicBuilder(_topic);
        }
        public CommandLookupTopicBuilder SetOriginalPrincipal(string originalPrincipal)
        {
            _topic.OriginalPrincipal = originalPrincipal;
            return new CommandLookupTopicBuilder(_topic);
        }
        public CommandLookupTopicBuilder SetRequestId(long requestId)
        {
            _topic.RequestId = (ulong)requestId;
            return new CommandLookupTopicBuilder(_topic);
        }
        public CommandLookupTopicBuilder SetTopic(string topic)
        {
            _topic.Topic = topic;
            return new CommandLookupTopicBuilder(_topic);
        }
        public CommandLookupTopic Build()
        {
            return _topic;
        }
    }
}
