using SharpPulsar.Common.PulsarApi;

namespace SharpPulsar.Command.Builder
{
    public class CommandPartitionedTopicMetadataBuilder
    {
        private readonly CommandPartitionedTopicMetadata _metadata;
        public CommandPartitionedTopicMetadataBuilder()
        {
            _metadata = new CommandPartitionedTopicMetadata();
        }
       
        public CommandPartitionedTopicMetadataBuilder SetOriginalAuthData(string originalAuthData)
        {
            _metadata.OriginalAuthData = originalAuthData;
            return this;
        }
        public CommandPartitionedTopicMetadataBuilder SetOriginalAuthMethod(string originalAuthMethod)
        {
            _metadata.OriginalAuthMethod = originalAuthMethod;
            return this;
        }
        public CommandPartitionedTopicMetadataBuilder SetOriginalPrincipal(string originalPrincipal)
        {
            _metadata.OriginalPrincipal = originalPrincipal;
            return this;
        }
        public CommandPartitionedTopicMetadataBuilder SetRequestId(long requestId)
        {
            _metadata.RequestId = (ulong)requestId;
            return this;
        }
        public CommandPartitionedTopicMetadataBuilder SetTopic(string topic)
        {
            _metadata.Topic = topic;
            return this;
        }
        
        public CommandPartitionedTopicMetadata Build()
        {
            return _metadata;
        }
    }
}
