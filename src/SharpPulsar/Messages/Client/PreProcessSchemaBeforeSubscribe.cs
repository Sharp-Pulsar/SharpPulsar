using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Client
{
    public sealed class PreProcessSchemaBeforeSubscribe<T>
    {
        public ISchema<T> Schema { get; }
        public string TopicName { get; }
        public PreProcessSchemaBeforeSubscribe(ISchema<T> schema, string topicName)
        {
            Schema = schema;
            TopicName = topicName;
        }
    }
}
