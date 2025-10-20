
namespace SharpPulsar.Messages.Consumer
{
    public record Unsubscribe(bool Force);
    public record UnsubscribeTopicName(string TopicName, bool Force);
}
