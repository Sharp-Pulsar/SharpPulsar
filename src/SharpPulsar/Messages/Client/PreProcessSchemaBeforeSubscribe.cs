using SharpPulsar.Interfaces;

namespace SharpPulsar.Messages.Client
{
    public record struct PreProcessSchemaBeforeSubscribe<T>(ISchema<T> Schema, string TopicName);
}
