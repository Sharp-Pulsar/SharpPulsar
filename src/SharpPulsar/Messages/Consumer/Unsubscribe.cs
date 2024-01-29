
namespace SharpPulsar.Messages.Consumer
{
    public readonly record struct Unsubscribe(bool Force);
    public readonly record struct UnsubscribeTopicName(string TopicName, bool Force);
}
