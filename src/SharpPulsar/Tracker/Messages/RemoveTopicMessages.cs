namespace SharpPulsar.Tracker.Messages
{
    public sealed class RemoveTopicMessages
    {
        public RemoveTopicMessages(string topicName)
        {
            TopicName = topicName;   
        }

        public string TopicName { get; }
    }
}
