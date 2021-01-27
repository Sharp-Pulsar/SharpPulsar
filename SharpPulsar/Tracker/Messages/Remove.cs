using SharpPulsar.Interfaces;

namespace SharpPulsar.Tracker.Messages
{
    public sealed class Remove
    {
        public Remove(IMessageId messageId)
        {
            MessageId = messageId;
        }

        public IMessageId MessageId { get; }
    }
    public sealed class RemoveTopicMessages
    {
        public RemoveTopicMessages(string topic)
        {
            Topic = topic;
        }

        public string Topic { get; }
    }
}
