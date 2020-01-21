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
        private CommandPartitionedTopicMetadataBuilder(CommandPartitionedTopicMetadata metadata)
        {
            _metadata = metadata;
        }
        public CommandPartitionedTopicMetadataBuilder SetOriginalAuthData(string originalAuthData)
        {
            _metadata.OriginalAuthData = originalAuthData;
            return new CommandPartitionedTopicMetadataBuilder(_metadata);
        }
        public CommandPartitionedTopicMetadataBuilder SetOriginalAuthMethod(string originalAuthMethod)
        {
            _metadata.OriginalAuthMethod = originalAuthMethod;
            return new CommandPartitionedTopicMetadataBuilder(_metadata);
        }
        public CommandPartitionedTopicMetadataBuilder SetOriginalPrincipal(string originalPrincipal)
        {
            _metadata.OriginalPrincipal = originalPrincipal;
            return new CommandPartitionedTopicMetadataBuilder(_metadata);
        }
        public CommandPartitionedTopicMetadataBuilder SetRequestId(long requestId)
        {
            _metadata.RequestId = (ulong)requestId;
            return new CommandPartitionedTopicMetadataBuilder(_metadata);
        }
        public CommandPartitionedTopicMetadataBuilder SetTopic(string topic)
        {
            _metadata.Topic = topic;
            return new CommandPartitionedTopicMetadataBuilder(_metadata);
        }
        
        public CommandPartitionedTopicMetadata Build()
        {
            return _metadata;
        }
    }
}
